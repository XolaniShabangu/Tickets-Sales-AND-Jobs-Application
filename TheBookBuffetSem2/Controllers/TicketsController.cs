using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TheBookBuffetSem2.Models;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.AspNet.Identity;
using System.IO;
using System.Net;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Net.Mail;

namespace TheBookBuffetSem2.Controllers
{
    public class TicketsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult ManageTickets(int eventId)
        {
            var eventDetails = db.Events.Find(eventId);
            if (eventDetails == null)
            {
                return HttpNotFound();
            }

            // Fetch all ticket categories for this event
            var tickets = db.Tickets.Where(t => t.EventID == eventId).ToList();

            var viewModel = new ManageTicketsViewModel
            {
                EventId = eventId,
                EventTitle = eventDetails.BookTitle,
                Tickets = tickets.Select(t => new TicketViewModel
                {
                    TicketID = t.TicketID,
                    Category = t.Category,
                    Price = t.Price,
                    TotalTickets = t.TotalTickets,         // Total number of tickets available for this category
                    RemainingTickets = t.RemainingTickets  // Remaining tickets available in this category
                }).ToList()
            };

            return View(viewModel);
        }


        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult CreateTickets(int eventId)
        {
            var eventDetails = db.Events.Find(eventId);
            if (eventDetails == null)
            {
                return HttpNotFound();
            }

            // Fetch the venue capacity
            var booking = db.Bookings.FirstOrDefault(b => b.EventId == eventId);
            var venueCapacity = booking?.Venue.Capacity ?? 0;

            var viewModel = new TicketManagementViewModel
            {
                EventID = eventId,
                EventName = eventDetails.BookTitle,
                VenueCapacity = venueCapacity,
                GeneralPrice = 0.00m,
                VIPPrice = 0.00m,
                VVIPPrice = 0.00m,
                GeneralTickets = 0,
                VIPTickets = 0,
                VVIPTickets = 0
            };

            return View(viewModel);
        }

       


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateTickets(TicketManagementViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Calculate the total number of tickets
                int totalTickets = model.GeneralTickets + model.VIPTickets + model.VVIPTickets;

                // Check if total tickets exceed venue capacity
                if (totalTickets > model.VenueCapacity)
                {
                    ModelState.AddModelError("", "The total number of tickets exceeds the venue capacity. Please adjust the numbers.");
                    return View(model);
                }

                // Remove existing tickets for this event
                var existingTickets = db.Tickets.Where(t => t.EventID == model.EventID).ToList();
                db.Tickets.RemoveRange(existingTickets);

                // Add new tickets
                db.Tickets.AddRange(new List<Ticket>
        {
            new Ticket
            {
                EventID = model.EventID,
                Category = "General",
                Price = model.GeneralPrice,
                TotalTickets = model.GeneralTickets,
                RemainingTickets = model.GeneralTickets
            },
            new Ticket
            {
                EventID = model.EventID,
                Category = "VIP",
                Price = model.VIPPrice,
                TotalTickets = model.VIPTickets,
                RemainingTickets = model.VIPTickets
            },
            new Ticket
            {
                EventID = model.EventID,
                Category = "VVIP",
                Price = model.VVIPPrice,
                TotalTickets = model.VVIPTickets,
                RemainingTickets = model.VVIPTickets
            }
        });

                db.SaveChanges();

                // Redirect to the event index page or a confirmation page
                return RedirectToAction("Index", "Event");
            }

            // Log or output the errors
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var error in errors)
            {
                System.Diagnostics.Debug.WriteLine(error.ErrorMessage);
            }

            // If there were validation errors, redisplay the form
            return View(model);
        }



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }


        //buy tickets

        // GET: Ticket/BuyTicket/{eventId}
        public ActionResult BuyTicket(int eventId)
        {
            var eventDetails = db.Events.Find(eventId);
            if (eventDetails == null)
            {
                return HttpNotFound();
            }

            // Get available tickets for this event
            var availableTickets = db.Tickets
                                     .Where(t => t.EventID == eventId && t.RemainingTickets > 0)
                                     .ToList();

            if (!availableTickets.Any())
            {
                TempData["ErrorMessage"] = "No tickets available for this event.";
                return RedirectToAction("UpcomingEvents", "Event");
            }

            var viewModel = new BuyTicketViewModel
            {
                EventID = eventId,
                EventTitle = eventDetails.BookTitle,
                Tickets = availableTickets.Select(t => new TicketViewModel
                {
                    TicketID = t.TicketID,
                    Category = t.Category,
                    Price = t.Price,
                    RemainingTickets = t.RemainingTickets
                }).ToList()
            };

            return View(viewModel);
        }

        

        public ActionResult ConfirmPurchase(int ticketId)
        {
            // Include the Event when loading the Ticket
            var ticket = db.Tickets.Include("Event").FirstOrDefault(t => t.TicketID == ticketId);
            if (ticket == null || ticket.RemainingTickets < 1)
            {
                TempData["ErrorMessage"] = "Sorry, this ticket is no longer available.";
                return RedirectToAction("BuyTickets", new { eventId = ticket.EventID });
            }

            // Get the current user
            var userId = User.Identity.GetUserId();
            var user = db.Users.Find(userId);
            if (user == null)
            {
                return HttpNotFound("User not found.");
            }

            // Calculate total price
            var totalPrice = ticket.Price;

            // Save a pending ticket sale (without QR code) for payment verification later
            var ticketSale = new TicketSale
            {
                TicketID = ticketId,
                UserID = userId,
                Quantity = 1,
                SaleDate = DateTime.Now,
                TotalPrice = totalPrice,
                QRCode = null  // QR code will be generated after successful payment
            };

            ticket.RemainingTickets -= 1;

            db.TicketSales.Add(ticketSale);
            db.SaveChanges();

            // Redirect to payment processing (e.g., PayFast)
            return RedirectToAction("ProcessPayment", new { ticketSaleId = ticketSale.TicketSaleID });
        }

        public ActionResult ProcessPayment(int ticketSaleId)
        {
            var ticketSale = db.TicketSales.Include("Ticket.Event").FirstOrDefault(ts => ts.TicketSaleID == ticketSaleId);
            if (ticketSale == null)
            {
                return HttpNotFound();
            }

            string site = "https://sandbox.payfast.co.za/eng/process?";
            string merchant_id = "";  // merchant ID
            string merchant_key = "";  // merchant key

            var paymentUrl = $"{site}merchant_id={merchant_id}&merchant_key={merchant_key}&amount={ticketSale.TotalPrice}&item_name=Tickets for {ticketSale.Ticket.Event.BookTitle}&return_url={Url.Action("PaymentSuccess", "Tickets", new { ticketSaleId = ticketSale.TicketSaleID }, Request.Url.Scheme)}&cancel_url={Url.Action("PaymentCancelled", "Tickets", new { ticketSaleId = ticketSale.TicketSaleID }, Request.Url.Scheme)}";

            return Redirect(paymentUrl);
        }



        
        public ActionResult PaymentSuccess(int ticketSaleId)
        {
            var ticketSale = db.TicketSales
                                .Include("Ticket.Event")
                                .Include("User")
                                .FirstOrDefault(ts => ts.TicketSaleID == ticketSaleId);

            if (ticketSale == null || ticketSale.Ticket == null || ticketSale.User == null)
            {
                return HttpNotFound();
            }

            // Generate QR Code content
            string qrContent = $"TicketID:{ticketSale.TicketSaleID};UserID:{ticketSale.UserID};Quantity:{ticketSale.Quantity};TotalPrice:{ticketSale.TotalPrice};Date:{DateTime.Now}";
            string qrCodeBase64 = GenerateQRCode(qrContent);

            // Update the ticket sale with the QR code
            ticketSale.QRCode = qrCodeBase64;

            
            

            // Generate and save the ticket image
            string ticketImagePath = GenerateTicketImage(
                ticketSale.Ticket.Event.BookTitle,
                ticketSale.Ticket.Event.LaunchDate.ToString("f"),
                ticketSale.User.Name,
                ticketSale.TotalPrice,
                ticketSale.Ticket.Category,
                qrContent,
                ticketSale.Quantity
            );

            // Update the ticket sale with the path to the ticket image
            ticketSale.TicketImagePath = ticketImagePath;

            // Save changes to the database
            db.SaveChanges();

            // Send the ticket via email
            SendTicketEmail(ticketSale.User.Email, ticketSale.User.Name, ticketSale.Ticket.Event.BookTitle, ticketImagePath);

            // Save the path of the generated ticket image in TempData to display it on the Thank You page
            TempData["TicketImagePath"] = ticketImagePath;

            // Redirect to Thank You page
            TempData["SuccessMessage"] = "Thank you for your purchase! Your ticket and QR code have been generated.";
            return RedirectToAction("ThankYou");
        }


        private void SendTicketEmail(string email, string name, string eventTitle, string ticketImagePath)
        {
            var message = new MailMessage
            {
                From = new MailAddress("senderemail.com", "The Book Buffet"),
                Subject = $"Your Ticket for {eventTitle}",
                Body = $"Dear {name},<br/><br/>Thank you for your purchase! Attached is your ticket for {eventTitle}.<br/><br/>Best Regards,<br/>The Book Buffet Team",
                IsBodyHtml = true
            };

            message.To.Add(new MailAddress(email));

            // Attach the ticket image
            if (!string.IsNullOrEmpty(ticketImagePath))
            {
                var webClient = new WebClient();
                byte[] ticketData = webClient.DownloadData(ticketImagePath);
                message.Attachments.Add(new Attachment(new MemoryStream(ticketData), "Ticket.jpeg", "image/jpeg"));
            }

            using (var smtpClient = new SmtpClient())
            {
                smtpClient.Send(message);
            }
        }



        public ActionResult PaymentCancelled(int ticketSaleId)
        {
            // You can log the failed payment or notify the user as needed.
            var ticketSale = db.TicketSales.Find(ticketSaleId);
            if (ticketSale == null)
            {
                return HttpNotFound();
            }

            // Redirect to a Payment Failed page
            return RedirectToAction("PaymentFailed");
        }


        public ActionResult PaymentFailed()
        {           
            return View();
        }
        public ActionResult ThankYou()
        {
            // Retrieve the ticket image path from TempData
            string ticketImagePath = TempData["TicketImagePath"] as string;

            // Pass the ticket image path to the view
            ViewBag.TicketImagePath = ticketImagePath;

            return View();
        }


        // QR Code generation method
        private string GenerateQRCode(string content)
        {
            using (var qrGenerator = new QRCodeGenerator())
            using (var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q))
            using (var qrCode = new QRCode(qrCodeData))
            using (var qrCodeImage = qrCode.GetGraphic(20))
            {
                using (var memoryStream = new System.IO.MemoryStream())
                {
                    qrCodeImage.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                    var qrCodeBytes = memoryStream.ToArray();
                    return Convert.ToBase64String(qrCodeBytes); // Return QR code as base64 string
                }
            }
        }


        
        private string GenerateTicketImage(string eventName, string eventDate, string customerName, decimal totalPrice, string category, string qrContent, int quantity)
        {
            // Generate the QR code image
            string qrCodeBase64;
            using (var qrGenerator = new QRCodeGenerator())
            using (var qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q))
            using (var qrCode = new QRCode(qrCodeData))
            using (var qrCodeImage = qrCode.GetGraphic(7))
            {
                using (var memoryStream = new MemoryStream())
                {
                    qrCodeImage.Save(memoryStream, ImageFormat.Png);
                    qrCodeBase64 = Convert.ToBase64String(memoryStream.ToArray()); // Get the QR code as Base-64
                }
            }

            // Convert the Base-64 QR code back to an image for overlaying on the ticket
            var qrCodeBytes = Convert.FromBase64String(qrCodeBase64);
            using (var ms = new MemoryStream(qrCodeBytes))
            using (var qrCodeFinalImage = Image.FromStream(ms))
            {
                // Load the ticket template image from Azure Blob Storage
                string templateBlobUrl = "https://appdev3blob.blob.core.windows.net/appdevproject/ticket.jpeg";  // replace with your actual blob URL for the template image
                var webClient = new System.Net.WebClient();
                byte[] templateBytes = webClient.DownloadData(templateBlobUrl);
                using (var templateStream = new MemoryStream(templateBytes))
                using (var ticketTemplate = Image.FromStream(templateStream))
                using (var graphics = Graphics.FromImage(ticketTemplate))
                {
                    // Define positions for text and QR code on the ticket
                    var font = new Font("Arial", 14, FontStyle.Bold);
                    var brush = Brushes.Black;

                    // Define positions for each text element and QR code
                    var qrPosition = new PointF(80, 100);  // QR Code - Positioned near the top center

                    // Define positions for each text element at the bottom of the image
                    var eventNamePosition = new PointF(60, 550);  // Event Name - aligned left at the bottom
                    var eventDatePosition = new PointF(60, 605);  // Event Date - aligned left and below event name
                    var customerNamePosition = new PointF(60, 690);  // Customer Name - aligned left and below event date
                    var categoryPosition = new PointF(350, 710);  // Category - aligned right of customer name, at the same height
                    var totalPricePosition = new PointF(60, 740);  // Total Price - aligned left and below customer name

                    // New: Add quantity of tickets
                    var quantityPosition = new PointF(300, 790);  
                    var quantityText = $"Admission for: {quantity}";

                    // Draw the text and QR code onto the ticket template
                    graphics.DrawString(eventName, font, brush, eventNamePosition);
                    graphics.DrawString(eventDate, font, brush, eventDatePosition);
                    graphics.DrawString(customerName, font, brush, customerNamePosition);
                    graphics.DrawString($"Category: {category}", font, brush, categoryPosition);
                    graphics.DrawString($"Total Price: {totalPrice:C}", font, brush, totalPricePosition);
                    graphics.DrawString(quantityText, font, brush, quantityPosition); // Draw quantity of tickets
                    graphics.DrawImage(qrCodeFinalImage, qrPosition);

                    // Convert the ticket image to a stream
                    using (var ticketStream = new MemoryStream())
                    {
                        ticketTemplate.Save(ticketStream, ImageFormat.Jpeg);
                        ticketStream.Position = 0;

                        // Save to Azure Blob Storage
                        string connectionString = "";
                        string containerName = "ticket-images";
                        string blobName = $"{Guid.NewGuid()}.jpeg";

                        BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
                        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                        containerClient.CreateIfNotExists(PublicAccessType.Blob);
                        BlobClient blobClient = containerClient.GetBlobClient(blobName);

                        // Upload the image stream to Azure Blob Storage
                        blobClient.Upload(ticketStream, new BlobHttpHeaders { ContentType = "image/jpeg" });

                        // Return the URL to the stored image
                        return blobClient.Uri.AbsoluteUri;
                    }
                }
            }
        }

    }
}  


