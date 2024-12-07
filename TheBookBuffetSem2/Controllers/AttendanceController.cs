using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TheBookBuffetSem2.Models;
using System.Threading.Tasks;

namespace TheBookBuffetSem2.Controllers
{
    public class AttendanceController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult ScanTickets(int eventId)
        {
            // Retrieve the selected event to display some details, if needed
            var selectedEvent = db.Events.Find(eventId);

            if (selectedEvent == null)
            {
                return HttpNotFound("Event not found");
            }

            // Pass the event ID and details to the view
            ViewBag.EventId = eventId;
            ViewBag.EventTitle = selectedEvent.BookTitle;

            return View();
        }


        [HttpPost]
        public JsonResult ProcessQRCode(string qrCodeData, int eventId)
        {
            // Parse the QR code content
            var parsedData = ParseQRCodeData(qrCodeData);

            if (parsedData == null)
            {
                return Json(new { success = false, message = "Invalid QR Code." });
            }

            if (parsedData.ContainsKey("TicketID"))
            {
                return ProcessTicketSaleQRCode(parsedData, eventId);
            }
            else if (parsedData.ContainsKey("InvitationID"))
            {
                return ProcessInvitedGuestQRCode(parsedData, eventId);
            }

            return Json(new { success = false, message = "Unknown QR Code format." });
        }

        private JsonResult ProcessTicketSaleQRCode(Dictionary<string, string> data, int eventId)
        {
            if (!int.TryParse(data["TicketID"], out int ticketId))
            {
                return Json(new { success = false, message = "Invalid Ticket ID." });
            }

            var ticketSale = db.TicketSales
                                .Include("Ticket")
                                .Include("User")
                                .FirstOrDefault(ts => ts.TicketSaleID == ticketId && ts.Ticket.EventID == eventId);

            if (ticketSale == null || ticketSale.CheckedIn)
            {
                return Json(new { success = false, message = "Ticket not found for this event or already checked in." });
            }

            ticketSale.CheckedIn = true;
            ticketSale.CheckedInTime = DateTime.Now;
            db.SaveChanges();

            return Json(new
            {
                success = true,
                message = "Ticket check-in successful.",
                details = new
                {
                    InviteeName = ticketSale.User.Name,
                    Category = ticketSale.Ticket.Category,
                    Quantity = ticketSale.Quantity,
                    CheckedInTime = ticketSale.CheckedInTime?.ToString("f")
                }
            });
        }


        private JsonResult ProcessInvitedGuestQRCode(Dictionary<string, string> data, int eventId)
        {
            if (!int.TryParse(data["InvitationID"], out int invitationId))
            {
                return Json(new { success = false, message = "Invalid Invitation ID." });
            }

            var invitedGuestTicket = db.InvitedGuestTickets
                                       .Include("Invitation")
                                       .FirstOrDefault(igt => igt.InvitationID == invitationId && igt.Invitation.EventID == eventId);

            if (invitedGuestTicket == null || invitedGuestTicket.CheckedIn)
            {
                return Json(new { success = false, message = "Invitation not found for this event or already checked in." });
            }

            invitedGuestTicket.CheckedIn = true;
            invitedGuestTicket.CheckedInTime = DateTime.Now;
            db.SaveChanges();


            // Trigger the survey email sending process
            SendSurveyEmail(invitedGuestTicket.Invitation);


            return Json(new
            {
                success = true,
                message = "Guest check-in successful.",
                details = new
                {
                    InviteeName = invitedGuestTicket.InviteeName,
                    Category = invitedGuestTicket.InviteeType,
                    Quantity = 1,
                    CheckedInTime = invitedGuestTicket.CheckedInTime?.ToString("f")
                }
            });
        }


        private Dictionary<string, string> ParseQRCodeData(string qrCodeData)
        {
            // Split the QR code content into key-value pairs
            var data = new Dictionary<string, string>();

            try
            {
                var parts = qrCodeData.Split(';');
                foreach (var part in parts)
                {
                    var keyValue = part.Split(':');
                    if (keyValue.Length == 2)
                    {
                        data[keyValue[0]] = keyValue[1];
                    }
                }
            }
            catch
            {
                // Log the error or handle it as needed
                return null;
            }

            return data;
        }


        public ActionResult ScanResult(string message)
        {
            // Pass the message to the view
            ViewBag.Message = message;
            return View();
        }


        // Action method to show events happening today
        public ActionResult SelectEventForScanning()
        {
            // Get today's date (without time part)
            var today = DateTime.Today;

            // Query to get events happening today
            var eventsToday = db.Events
                                .Where(e => DbFunctions.TruncateTime(e.LaunchDate) == today)
                                .ToList();

            return View(eventsToday);
        }

        //email for survey of invited
        private async Task SendSurveyEmailAsync(Invitation invitation)
        {
            // Delay the sending for 2 minutes
            await Task.Delay(TimeSpan.FromMinutes(2));

            // Now send the email (same logic as above)
            SendSurveyEmail(invitation);
        }


        private void SendSurveyEmail(Invitation invitation)
        {
            // Ensure the invitation object and its properties are not null
            if (invitation == null)
            {
                throw new ArgumentNullException(nameof(invitation), "Invitation object cannot be null.");
            }

            if (invitation.EventID == null)
            {
                throw new InvalidOperationException("Event ID associated with the invitation cannot be null.");
            }

            // Ensure that the Event object is loaded
            var eventDetails = db.Events.Find(invitation.EventID);
            if (eventDetails == null)
            {
                throw new InvalidOperationException("Event associated with the invitation could not be found.");
            }

            // Determine the email based on the invitee type
            string recipientEmail = string.Empty;

            if (invitation.InviteeType == "Author")
            {
                // Fetch the author using the InviteeID
                var author = db.Authors.Find(invitation.InviteeID);
                if (author != null)
                {
                    recipientEmail = author.Email;
                }
            }
            else if (invitation.InviteeType == "Media")
            {
                // Fetch the media contact using the InviteeID
                var media = db.Media.Find(invitation.InviteeID);
                if (media != null)
                {
                    recipientEmail = media.Email;
                }
            }

            if (!string.IsNullOrEmpty(recipientEmail))
            {
                // Generate a survey link (e.g., using the InvitationID)
                //var surveyUrl = Url.Action("TakeSurvey", "Customer", new { invitationId = invitation.InvitationID }, protocol: Request.Url.Scheme);
                var surveyUrl = Url.Action("TakeSurvey", "Customer", new { eventId = invitation.EventID, invitationId = invitation.InvitationID }, protocol: Request.Url.Scheme);


                // Prepare and send the email
                var mailMessage = new MailMessage
                {
                    From = new MailAddress("email.com"), //email
                    Subject = $"Survey for {eventDetails.BookTitle}",
                    Body = $"Dear {invitation.InviteeType},<br/><br/>" +
                           $"Thank you for attending the event '{eventDetails.BookTitle}'. We would appreciate your feedback. " +
                           $"Please click the link below to complete the survey:<br/><br/>" +
                           $"<a href='{surveyUrl}'>Complete the Survey</a><br/><br/>" +
                           "Thank you,<br/>The Book Buffet Team",
                    IsBodyHtml = true
                };

                // Add recipient email
                mailMessage.To.Add(new MailAddress(recipientEmail));

                // Send email using the SMTP settings from web.config
                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.Send(mailMessage);
                }
            }
            else
            {
                // Handle the case where the recipient email could not be determined
                // Log the issue or take appropriate action
                throw new InvalidOperationException("Could not determine recipient email.");
            }
        }





    }
}