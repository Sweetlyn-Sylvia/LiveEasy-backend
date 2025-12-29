using Microsoft.AspNetCore.Mvc;
using ParcelTrackingSystem.Data;
using System.Net.Http;
using System.Net.Http.Json;

namespace ParcelTrackingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RouteController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;

        public RouteController(AppDbContext context, IHttpClientFactory factory)
        {
            _context = context;
            _httpClient = factory.CreateClient();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("ParcelTrackingSystem/1.0");
        }

       
        public class NominatimResponse
        {
            public string lat { get; set; }
            public string lon { get; set; }
        }

        private async Task<(double lat, double lon)?> GetCoordinates(string address)
        {
            var url = $"https://nominatim.openstreetmap.org/search?format=json&q={Uri.EscapeDataString(address + ", Tamil Nadu, India")}";
            var result = await _httpClient.GetFromJsonAsync<List<NominatimResponse>>(url);

            if (result != null && result.Any())
            {
                return (double.Parse(result[0].lat), double.Parse(result[0].lon));
            }

            return null;
        }

    
        public class RouteNode
        {
            public string ParcelID { get; set; }
            public string Address { get; set; }
            public double Lat { get; set; }
            public double Lon { get; set; }
        }

        
        private double Distance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; 
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRadians(double deg) => deg * (Math.PI / 180);


        [HttpGet("smart-route/{agentID}")]
        public async Task<IActionResult> GetSmartRoute(string agentID)
        {
            
            var parcels = _context.ParcelCreations
                .Where(p => p.AgentID == agentID && p.Status != "Delivered")
                .OrderBy(p => p.Date)
                .ToList();

            if (!parcels.Any())
                return NotFound("No active parcels found.");

         
            var deliveryNodes = new List<RouteNode>();
            foreach (var parcel in parcels)
            {
                var coords = await GetCoordinates(parcel.ReceiverAddress);
                if (coords != null)
                {
                    deliveryNodes.Add(new RouteNode
                    {
                        ParcelID = parcel.ParcelID,
                        Address = parcel.ReceiverAddress,
                        Lat = coords.Value.lat,
                        Lon = coords.Value.lon
                    });
                }
            }

            if (!deliveryNodes.Any())
                return NotFound("No valid delivery locations found.");

           
            var startCoords = await GetCoordinates(parcels.First().SenderAddress);
            if (startCoords == null)
                return NotFound("Unable to determine start location.");

            double currentLat = startCoords.Value.lat;
            double currentLon = startCoords.Value.lon;

            var route = new List<object>();
            int step = 1;

           
            while (deliveryNodes.Any())
            {
                var next = deliveryNodes
                    .OrderBy(n => Distance(currentLat, currentLon, n.Lat, n.Lon))
                    .First();

                route.Add(new
                {
                    Step = step++,
                    Type = "Delivery",
                    next.ParcelID,
                    next.Address
                });

                currentLat = next.Lat;
                currentLon = next.Lon;

                deliveryNodes.Remove(next);
            }

            return Ok(route);
        }
    }
}
