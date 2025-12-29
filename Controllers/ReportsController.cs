using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParcelTrackingSystem.Models;
using ParcelTrackingSystem.Data;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ParcelTrackingSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("pdf/{type}")]
        public async Task<IActionResult> GetReportPdf(string type)
        {
            return await GenerateReport(type);
        }

        [HttpGet("pdf/range")]
        public async Task<IActionResult> GetRangeReport(DateTime startDate, DateTime endDate)
        {
            var parcels = await _context.ParcelCreations
                .Where(p => p.Date.Date >= startDate.Date && p.Date.Date <= endDate.Date)
                .ToListAsync();

            return await GeneratePdf(parcels,
                $"RANGE REPORT ({startDate:dd-MM-yyyy} to {endDate:dd-MM-yyyy})",
                "range-report", null);
        }

        [HttpGet("pdf/agent")]
        public async Task<IActionResult> GetAgentReport(string agentID)
        {
            var parcels = await _context.ParcelCreations
                .Where(p => p.AgentID == agentID)
                .ToListAsync();

           
            var agent = await _context.Agents.FirstOrDefaultAsync(a => a.AgentID == agentID);
            string agentName = agent?.Name ?? agentID;

            return await GeneratePdf(parcels,
                $"AGENT REPORT ({agentName})",
                $"agent-report-{agentID}",
                agentName);
        }
        [HttpGet("pdf/agent/range")]
        public async Task<IActionResult> GetAgentRangeReport(string agentID, DateTime startDate, DateTime endDate)
        {
            var parcels = await _context.ParcelCreations
                .Where(p => p.AgentID == agentID && p.Date.Date >= startDate.Date && p.Date.Date <= endDate.Date)
                .ToListAsync();

            var agent = await _context.Agents.FirstOrDefaultAsync(a => a.AgentID == agentID);
            string agentName = agent?.Name ?? agentID;

            return await GeneratePdf(parcels,
                $"AGENT REPORT ({agentName}) - {startDate:dd-MM-yyyy} to {endDate:dd-MM-yyyy}",
                $"agent-report-{agentID}-{DateTime.Now:ddMMyyyy}",
                agentName);
        }
        [HttpGet("pdf/earnings")]
        public async Task<IActionResult> GetEarningsReport(DateTime startDate, DateTime endDate)
        {
            var parcels = await _context.ParcelCreations
                .Where(p => p.Date.Date >= startDate.Date && p.Date.Date <= endDate.Date)
                .ToListAsync();

            return await GenerateEarningsPdf(parcels, startDate, endDate);
        }
        private async Task<IActionResult> GenerateEarningsPdf(List<ParcelCreation> parcels, DateTime startDate, DateTime endDate)
        {
            using var memoryStream = new MemoryStream();
            Document document = new Document(PageSize.A4, 20, 20, 40, 40);
            var writer = PdfWriter.GetInstance(document, memoryStream);
            writer.PageEvent = new PdfPageEvents();
            document.Open();

            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11);
            var bodyFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
            string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logo.jpg");
            if (System.IO.File.Exists(logoPath))
            {
                Image logo = Image.GetInstance(logoPath);
                logo.ScaleToFit(80, 80);
                logo.Alignment = Element.ALIGN_CENTER;
                document.Add(logo);
            }


            document.Add(new Paragraph("LiveEasy Parcel Delivery Tracker", titleFont) { Alignment = Element.ALIGN_CENTER });
            document.Add(new Paragraph($"EARNINGS REPORT ({startDate:dd-MM-yyyy} to {endDate:dd-MM-yyyy})", headerFont) { Alignment = Element.ALIGN_CENTER });
            document.Add(new Paragraph($"\nGenerated On: {DateTime.Now:dd-MM-yyyy hh:mm tt}\n\n", bodyFont));

           
            PdfPTable table = new PdfPTable(5) { WidthPercentage = 100 };
            table.AddCell(new PdfPCell(new Phrase("Parcel ID", headerFont)) { Padding = 5 });
            table.AddCell(new PdfPCell(new Phrase("Agent ID", headerFont)) { Padding = 5 });
            table.AddCell(new PdfPCell(new Phrase("Date", headerFont)) { Padding = 5 });
            table.AddCell(new PdfPCell(new Phrase("Remarks", headerFont)) { Padding = 5 });
            table.AddCell(new PdfPCell(new Phrase("Delivery Amount (Rs)", headerFont)) { Padding = 5 });

            decimal totalEarnings = 0;

            foreach (var p in parcels)
            {
                table.AddCell(new PdfPCell(new Phrase(p.ParcelID ?? "-", bodyFont)) { Padding = 5 });
                table.AddCell(new PdfPCell(new Phrase(p.AgentID ?? "-", bodyFont)) { Padding = 5 });
                table.AddCell(new PdfPCell(new Phrase(p.Date.ToString("dd-MM-yyyy"), bodyFont)) { Padding = 5 });
                table.AddCell(new PdfPCell(new Phrase(p.Remarks ?? "-", bodyFont)) { Padding = 5 });
                table.AddCell(new PdfPCell(new Phrase(p.DeliveryAmount.ToString("F2"), bodyFont)) { Padding = 5 });

                totalEarnings += Convert.ToDecimal(p.DeliveryAmount);
            }


            document.Add(table);

         
            PdfPTable totalTable = new PdfPTable(2) { WidthPercentage = 40, HorizontalAlignment = Element.ALIGN_RIGHT, SpacingBefore = 10 };
            totalTable.AddCell(new PdfPCell(new Phrase("Total Earnings", headerFont)) { Padding = 5 });
            totalTable.AddCell(new PdfPCell(new Phrase(totalEarnings.ToString("F2"), bodyFont)) { Padding = 5 });
            document.Add(totalTable);

            document.Close();

            return File(memoryStream.ToArray(), "application/pdf", $"earnings-report-{startDate:yyyyMMdd}-{endDate:yyyyMMdd}.pdf");
        }


        private async Task<IActionResult> GenerateReport(string type)
        {
            List<ParcelCreation> parcels;

            if (type == "daily")
            {
                DateTime today = DateTime.Today;
                parcels = await _context.ParcelCreations
                    .Where(p => p.Date.Date == today)
                    .ToListAsync();
            }
            else if (type == "delivered")
            {
                DateTime today = DateTime.Today;
                parcels = await _context.ParcelCreations
                    .Where(p => p.Status == "Delivered" && p.Date.Date == today)
                    .ToListAsync();
            }
            else if (type == "pending")
            {
                parcels = await _context.ParcelCreations
                    .Where(p => p.Status == "In Transit" || p.Status == "Picked Up")
                    .ToListAsync();
            }
            else
            {
                return BadRequest("Invalid report type");
            }

            return await GeneratePdf(parcels,
                $"{type.ToUpper()} REPORT",
                type, null);
        }

        private async Task<IActionResult> GeneratePdf(List<ParcelCreation> parcels,
                                                      string reportTitle,
                                                      string filenamePrefix,
                                                      string? agentName)
        {
            int totalParcels = parcels.Count;
            int deliveredCount = parcels.Count(p => p.Status == "Delivered");
            int inTransitCount = parcels.Count(p => p.Status == "In Transit");
            int pickedUpCount = parcels.Count(p => p.Status == "Picked Up");

            using var memoryStream = new MemoryStream();
            Document document = new Document(PageSize.A4, 20, 20, 40, 40);
            var writer = PdfWriter.GetInstance(document, memoryStream);
            writer.PageEvent = new PdfPageEvents();

            document.Open();

            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11);
            var bodyFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);

            string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logo.jpg");
            if (System.IO.File.Exists(logoPath))
            {
                Image logo = Image.GetInstance(logoPath);
                logo.ScaleToFit(80, 80);
                logo.Alignment = Element.ALIGN_CENTER;
                document.Add(logo);
            }

            document.Add(new Paragraph("LiveEasy Parcel Delivery Tracker", titleFont) { Alignment = Element.ALIGN_CENTER });
            document.Add(new Paragraph(reportTitle, headerFont) { Alignment = Element.ALIGN_CENTER });
            document.Add(new Paragraph($"Generated On: {DateTime.Now:dd-MM-yyyy hh:mm tt}\n\n", bodyFont));


            var supervisor = await _context.Supervisors.FirstOrDefaultAsync();
            string supervisorName = supervisor?.Name ?? "Supervisor";

            document.Add(new Paragraph($"Supervisor: {supervisorName}\n\n", bodyFont));

         
            if (!string.IsNullOrEmpty(agentName))
            {
                document.Add(new Paragraph($"Agent: {agentName}\n\n", bodyFont));
            }

        
            PdfPTable summaryTable = new PdfPTable(2)
            {
                WidthPercentage = 60,
                HorizontalAlignment = Element.ALIGN_LEFT,
                SpacingAfter = 15
            };

            PdfPCell SummaryCell(string text, bool isHeader = false)
            {
                var font = isHeader ? headerFont : bodyFont;
                return new PdfPCell(new Phrase(text, font)) { Padding = 6, BorderWidth = 1 };
            }

            summaryTable.AddCell(SummaryCell("Summary", true));
            summaryTable.AddCell(SummaryCell("Count", true));

            
            if (filenamePrefix == "daily" || filenamePrefix == "range-report" || filenamePrefix.StartsWith("agent-report"))
            {
                summaryTable.AddCell(SummaryCell("Total Parcels")); summaryTable.AddCell(SummaryCell(totalParcels.ToString()));
                summaryTable.AddCell(SummaryCell("Delivered Parcels")); summaryTable.AddCell(SummaryCell(deliveredCount.ToString()));
                summaryTable.AddCell(SummaryCell("In Transit Parcels")); summaryTable.AddCell(SummaryCell(inTransitCount.ToString()));
                summaryTable.AddCell(SummaryCell("Picked Up Parcels")); summaryTable.AddCell(SummaryCell(pickedUpCount.ToString()));
            }
            else if (filenamePrefix == "delivered")
            {
                summaryTable.AddCell(SummaryCell("Total Delivered Parcels")); summaryTable.AddCell(SummaryCell(deliveredCount.ToString()));
            }
            else if (filenamePrefix == "pending")
            {
                summaryTable.AddCell(SummaryCell("Total Parcels")); summaryTable.AddCell(SummaryCell(totalParcels.ToString()));
                summaryTable.AddCell(SummaryCell("In Transit Parcels")); summaryTable.AddCell(SummaryCell(inTransitCount.ToString()));
                summaryTable.AddCell(SummaryCell("Picked Up Parcels")); summaryTable.AddCell(SummaryCell(pickedUpCount.ToString()));
            }

            document.Add(summaryTable);

       
            PdfPTable table = new PdfPTable(9) { WidthPercentage = 100 };
            PdfPCell HeaderCell(string text) => new PdfPCell(new Phrase(text, headerFont)) { Padding = 5 };

            table.AddCell(HeaderCell("Parcel ID")); table.AddCell(HeaderCell("Sender")); table.AddCell(HeaderCell("Receiver"));
            table.AddCell(HeaderCell("Agent ID")); table.AddCell(HeaderCell("Status")); table.AddCell(HeaderCell("Contact"));
            table.AddCell(HeaderCell("Date")); table.AddCell(HeaderCell("Remarks"));
            table.AddCell(HeaderCell("Amount(Rs)"));

            foreach (var p in parcels)
            {
                table.AddCell(p.ParcelID ?? "-");
                table.AddCell(p.SenderName ?? "-");
                table.AddCell(p.ReceievrName ?? "-");
                table.AddCell(p.AgentID ?? "-");
                table.AddCell(p.Status ?? "-");
                table.AddCell(p.ReceiverContactNumber ?? "-");
                table.AddCell(p.Date.ToString("dd-MM-yyyy"));
                table.AddCell(p.Remarks ?? "-");
                table.AddCell( p.DeliveryAmount.ToString("F2"));


            }

            document.Add(table);
            document.Close();

            return File(memoryStream.ToArray(), "application/pdf", $"{filenamePrefix}-{DateTime.Now:ddMMyyyy}.pdf");
        }

        public class PdfPageEvents : PdfPageEventHelper
        {
            public override void OnEndPage(PdfWriter writer, Document document)
            {
                base.OnEndPage(writer, document);
                PdfPTable footer = new PdfPTable(1)
                {
                    TotalWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin
                };
                footer.DefaultCell.Border = 0;
                footer.AddCell(new Phrase($"Page {writer.PageNumber}", FontFactory.GetFont(FontFactory.HELVETICA, 8)));
                footer.WriteSelectedRows(0, -1, document.LeftMargin, document.BottomMargin - 5, writer.DirectContent);
            }
        }
    }

}
