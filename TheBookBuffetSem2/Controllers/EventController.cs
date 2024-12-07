using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TheBookBuffetSem2.Models;
using System.Net.Mail;
using QRCoder;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Net;
using System.Data.Entity;
using Hangfire;
using Microsoft.AspNet.Identity;


namespace TheBookBuffetSem2.Controllers
{
    public class EventController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index(string searchString, DateTime? searchDate, string searchVenue)
        {
            // Fetch all events
            var events = db.Events.ToList();

            // Sort events: upcoming first, then past events
            var sortedEvents = events
                .OrderBy(e => e.LaunchDate >= DateTime.Now ? 0 : 1) // Upcoming events first
                .ThenBy(e => e.LaunchDate)
                .ToList();

            // Filter by search criteria
            if (!string.IsNullOrEmpty(searchString))
            {
                sortedEvents = sortedEvents.Where(e => e.Author.Contains(searchString)).ToList();
            }

            if (searchDate.HasValue)
            {
                sortedEvents = sortedEvents.Where(e => e.LaunchDate.Date == searchDate.Value.Date).ToList();
            }

            if (!string.IsNullOrEmpty(searchVenue))
            {
                sortedEvents = sortedEvents.Where(e => db.Bookings.Any(b => b.EventId == e.EventId && b.Venue.Name.Contains(searchVenue))).ToList();
            }

            // Prepare the view model
            var eventListWithVenues = sortedEvents.Select(e => new EventVenueViewModel
            {
                Event = e,
                Venue = db.Bookings.Where(b => b.EventId == e.EventId)
                                   .Select(b => b.Venue)
                                   .FirstOrDefault(),
                GuestsInvited = db.Invitations.Any(i => i.EventID == e.EventId), // Check if guests are invited
                TicketsCreated = db.Tickets.Any(t => t.EventID == e.EventId) // Check if tickets are created
            }).ToList();

            return View(eventListWithVenues);
        }






        // GET: Event/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Event/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Objectives,Author,BookTitle,LaunchDate,Budget")] Event @event)
        {
            if (ModelState.IsValid)
            {
                db.Events.Add(@event);
                db.SaveChanges();
                return RedirectToAction("SelectVenue", "Venue", new { eventId = @event.EventId });
            }

            return View(@event);
        }

        // Dispose of the db context
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        //showing full details of event to confirm or edit.

        // GET: Event/Details
       
        public ActionResult Details(int id, bool fromIndex = false)
        {
            var eventDetails = db.Events
                                 .Include("Venue")
                                 .FirstOrDefault(e => e.EventId == id);

            if (eventDetails == null)
            {
                return HttpNotFound();
            }

            var booking = db.Bookings.FirstOrDefault(b => b.EventId == id);

            if (booking != null)
            {
                eventDetails.Venue = db.Venues.FirstOrDefault(v => v.VenueID == booking.VenueId);
            }

            // Pass the fromIndex flag to the view
            ViewBag.FromIndex = fromIndex;

            return View(eventDetails);
        }

        // GET: Event/Edit
        public ActionResult Edit(int id)
        {
            var eventDetails = db.Events.FirstOrDefault(e => e.EventId == id);

            if (eventDetails == null)
            {
                return HttpNotFound();
            }

            // Fetch the associated booking and then retrieve the venue
            var booking = db.Bookings.FirstOrDefault(b => b.EventId == id);
            if (booking != null)
            {
                ViewBag.CurrentVenue = db.Venues.FirstOrDefault(v => v.VenueID == booking.VenueId);
            }
            else
            {
                ViewBag.CurrentVenue = null;
            }



            return View(eventDetails);
        }


        // POST: Event/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Event @event, int? newVenueId)
        {
            if (ModelState.IsValid)
            {
                // Retrieve the existing event
                var existingEvent = db.Events.FirstOrDefault(e => e.EventId == @event.EventId);

                if (existingEvent != null)
                {
                    // Update the event details
                    existingEvent.Objectives = @event.Objectives;
                    existingEvent.Author = @event.Author;
                    existingEvent.BookTitle = @event.BookTitle;
                    existingEvent.LaunchDate = @event.LaunchDate;
                    existingEvent.Budget = @event.Budget;

                    // Retrieve the booking and update the venue if necessary
                    var booking = db.Bookings.FirstOrDefault(b => b.EventId == existingEvent.EventId);
                    if (booking != null && newVenueId.HasValue && newVenueId.Value != booking.VenueId)
                    {
                        booking.VenueId = newVenueId.Value;
                    }

                    db.SaveChanges();

                    return RedirectToAction("Details", new { id = @event.EventId });
                }
            }

            // If something goes wrong, retrieve the current venue information
            var bookingInfo = db.Bookings.FirstOrDefault(b => b.EventId == @event.EventId);
            if (bookingInfo != null)
            {
                ViewBag.CurrentVenue = db.Venues.FirstOrDefault(v => v.VenueID == bookingInfo.VenueId);
            }

            return View(@event);
        }



        //add author same view

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateAuthor([Bind(Include = "Name,Email")] Author author, int eventId)
        {
            if (ModelState.IsValid)
            {
                db.Authors.Add(author);
                db.SaveChanges();

                // Redirect back to the same page, with the new author included in the list
                return RedirectToAction("InviteGuests", new { id = eventId });
            }

            // If the save operation failed, return to the view with the original model
            var eventDetails = db.Events.Find(eventId);
            var booking = db.Bookings.FirstOrDefault(b => b.EventId == eventId);
            var venueName = booking?.Venue?.Name;

            var viewModel = new InviteGuestsViewModel
            {
                EventId = eventId,
                EventTitle = eventDetails.BookTitle,
                Authors = db.Authors.ToList().Select(a => new AuthorSelectionViewModel
                {
                    AuthorID = a.AuthorID,
                    Name = a.Name,
                    Email = a.Email,
                    IsSelected = false
                }).ToList(),
                InvitationMessage = $"The Book Buffet would like to invite you to the book launch of '{eventDetails.BookTitle}' by {eventDetails.Author} on {eventDetails.LaunchDate.ToString("f")} at {venueName}."
            };

            return View("InviteGuests", viewModel);
        }


        //invite guests
        [Authorize(Roles = "Admin")]
        public ActionResult InviteGuests(int id)
        {
            var eventDetails = db.Events.Find(id);
            if (eventDetails == null)
            {
                return HttpNotFound();
            }

            // Fetch the booking related to this event to get the venue details
            var booking = db.Bookings.FirstOrDefault(b => b.EventId == id);
            var venueName = booking?.Venue?.Name; // Retrieve the venue name from the booking

            // Fetch all authors
            var authors = db.Authors.ToList();

            var viewModel = new InviteGuestsViewModel
            {
                EventId = id,
                EventTitle = eventDetails.BookTitle,
                Authors = authors.Select(a => new AuthorSelectionViewModel
                {
                    AuthorID = a.AuthorID,
                    Name = a.Name,
                    Email = a.Email,
                    IsSelected = false
                }).ToList(),
                InvitationMessage = $"The Book Buffet would like to invite you to the book launch of '{eventDetails.BookTitle}' by {eventDetails.Author} on {eventDetails.LaunchDate.ToString("f")} at {venueName}."
            };

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendInvitations(InviteGuestsViewModel model)
        {
            if (model.Authors == null || !model.Authors.Any())
            {
                ModelState.AddModelError("", "No authors selected.");
                return View(model); // Return the view with an error message
            }

            var selectedAuthors = model.Authors.Where(a => a.IsSelected).ToList();
            var eventDetails = db.Events.Find(model.EventId);

            if (selectedAuthors.Count > 0 && eventDetails != null)
            {
                foreach (var author in selectedAuthors)
                {

                    // Check if an invitation already exists for this event and author
                    var existingInvitation = db.Invitations.FirstOrDefault(i => i.EventID == model.EventId && i.InviteeID == author.AuthorID && i.InviteeType == "Author");
                    if (existingInvitation == null)
                    {
                        // Generate a unique token for RSVP
                        var token = Guid.NewGuid().ToString();

                        // Create and save the invitation
                        var invitation = new Invitation
                        {
                            Token = token,
                            InviteeType = "Author",
                            InviteeID = author.AuthorID,
                            EventID = model.EventId,
                            RSVPStatus = null, // Not yet responded
                            WillingToSpeak = null // No response yet
                        };
                        db.Invitations.Add(invitation);
                        db.SaveChanges();

                        // Send the email invitation
                        var invitationUrl = Url.Action("RSVP", "Event", new { token = token }, Request.Url.Scheme);
                        string message = model.InvitationMessage +
                                         "\n\nPlease RSVP by clicking the link below:\n" +
                                         invitationUrl +
                                         "\n\nIf you'd like to speak at the event, please indicate when accepting the invitation.";

                        // Create the MailMessage
                        var mailMessage = new MailMessage("thebookbuffet4@gmail.com", author.Email)
                        {
                            Subject = "Invitation to " + eventDetails.BookTitle + " Book Launch.",
                            Body = message,
                            IsBodyHtml = true // If your message contains HTML
                        };

                        using (var smtpClient = new SmtpClient())
                        {
                            await smtpClient.SendMailAsync(mailMessage);
                        }
                    }
                }

                // Set a confirmation message in TempData
                TempData["SuccessMessage"] = "Invitations to authors have been sent successfully! Duplicates have been ignored.";
            }

            // Redirect to the media invitation page
            return RedirectToAction("InviteMedia", new { id = model.EventId });
        }


        //add media in view

        ////media
        //public ActionResult InviteMedia(int id)
        //{
        //    var eventDetails = db.Events.Find(id);
        //    if (eventDetails == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    // Fetch the booking related to this event to get the venue details
        //    var booking = db.Bookings.FirstOrDefault(b => b.EventId == id);
        //    var venueName = booking?.Venue?.Name; // Retrieve the venue name from the booking

        //    // Fetch all media contacts
        //    var mediaContacts = db.Media.ToList();

        //    var viewModel = new InviteMediaViewModel
        //    {
        //        EventId = id,
        //        EventTitle = eventDetails.BookTitle,
        //        MediaContacts = mediaContacts.Select(m => new MediaSelectionViewModel
        //        {
        //            MediaID = m.MediaID,
        //            Name = m.Name,
        //            Email = m.Email,
        //            IsSelected = false
        //        }).ToList(),
        //        InvitationMessage = $"The Book Buffet would like to invite you to cover the book launch of '{eventDetails.BookTitle}' by {eventDetails.Author} on {eventDetails.LaunchDate.ToString("f")} at {venueName}."
        //    };

        //    return View(viewModel);
        //}


        // POST: Management/CreateMedia
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateMedia([Bind(Include = "Name,Email")] Media media, int eventId)
        {
            if (ModelState.IsValid)
            {
                db.Media.Add(media);
                db.SaveChanges();
                return RedirectToAction("InviteMedia", new { id = eventId });
            }

            return View(media);
        }

        // Original action
        public ActionResult InviteMedia(int id)
        {
            var eventDetails = db.Events.Find(id);
            if (eventDetails == null)
            {
                return HttpNotFound();
            }

            // Fetch the booking related to this event to get the venue details
            var booking = db.Bookings.FirstOrDefault(b => b.EventId == id);
            var venueName = booking?.Venue?.Name;

            // Fetch all media contacts
            var mediaContacts = db.Media.ToList();

            var viewModel = new InviteMediaViewModel
            {
                EventId = id,
                EventTitle = eventDetails.BookTitle,
                MediaContacts = mediaContacts.Select(m => new MediaSelectionViewModel
                {
                    MediaID = m.MediaID,
                    Name = m.Name,
                    Email = m.Email,
                    IsSelected = false
                }).ToList(),
                InvitationMessage = $"The Book Buffet would like to invite you to cover the book launch of '{eventDetails.BookTitle}' by {eventDetails.Author} on {eventDetails.LaunchDate.ToString("f")} at {venueName}."
            };

            return View(viewModel);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendMediaInvitations(InviteMediaViewModel model)
        {
            var selectedMediaContacts = model.MediaContacts.Where(m => m.IsSelected).ToList();
            var eventDetails = db.Events.Find(model.EventId);

            if (selectedMediaContacts.Count > 0 && eventDetails != null)
            {
                foreach (var media in selectedMediaContacts)
                {
                    // Check if an invitation already exists for this event and author
                    var existingInvitation = db.Invitations.FirstOrDefault(i => i.EventID == model.EventId && i.InviteeID == media.MediaID && i.InviteeType == "Media");
                    if (existingInvitation == null)
                    {

                        if (string.IsNullOrEmpty(media.Email))
                        {
                            ModelState.AddModelError("", $"Media contact {media.Name} has a null email address.");
                            return View(model); // Return the view with an error message
                        }

                        // Generate a unique token for RSVP (if needed)
                        var token = Guid.NewGuid().ToString();

                        // Create and save the invitation (optional, if you need to track invitations)
                        var invitation = new Invitation
                        {
                            Token = token,
                            InviteeType = "Media",
                            InviteeID = media.MediaID,
                            EventID = model.EventId,
                            RSVPStatus = null
                        };
                        db.Invitations.Add(invitation);
                        db.SaveChanges();

                        // Send the email invitation
                        var invitationUrl = Url.Action("RSVP", "Event", new { token = token }, Request.Url.Scheme);
                        string message = model.InvitationMessage +
                                         "\n\nPlease RSVP by clicking the link below:\n" +
                                         invitationUrl;

                        // Create the MailMessage
                        var mailMessage = new MailMessage("thebookbuffet4@gmail.com", media.Email)
                        {
                            Subject = "Media Invitation to " + eventDetails.BookTitle,
                            Body = message,
                            IsBodyHtml = true
                        };

                        using (var smtpClient = new SmtpClient())
                        {
                            await smtpClient.SendMailAsync(mailMessage);
                        }
                    }
                }

                // Set a confirmation message in TempData
                TempData["SuccessMessage"] = "Invitations to media contacts have been sent successfully!. Duplicates have been ignored.";
            }

            // Redirect to a confirmation or another page
            return RedirectToAction("Index", "Event");
        }

        //rsvp
        //GET
        public ActionResult RSVP(string token)
        {
            var invitation = db.Invitations.FirstOrDefault(i => i.Token == token);
            if (invitation == null || invitation.RSVPStatus != null)
            {
                // Redirect to an error page or show a message indicating the link has already been used
                TempData["ErrorMessage"] = "This invitation link has already been used or is invalid.";
                return RedirectToAction("RSVPError");
            }

            var eventDetails = db.Events.Find(invitation.EventID);
            if (eventDetails == null)
            {
                return HttpNotFound();
            }

            // Retrieve the invitee's name based on InviteeType
            string inviteeName;
            if (invitation.InviteeType == "Author")
            {
                var author = db.Authors.Find(invitation.InviteeID);
                inviteeName = author?.Name;
            }
            else
            {
                var mediaContact = db.Media.Find(invitation.InviteeID);
                inviteeName = mediaContact?.Name;
            }

            var viewModel = new RSVPViewModel
            {
                Token = token,
                EventTitle = eventDetails.BookTitle,
                InviteeName = inviteeName,
                InviteeType = invitation.InviteeType,
                EventID = invitation.EventID.Value
            };

            return View(viewModel);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RSVP(RSVPViewModel model)
        {
            var invitation = db.Invitations.FirstOrDefault(i => i.Token == model.Token);

            if (invitation == null)
            {
                return HttpNotFound();
            }

            invitation.RSVPStatus = model.RSVPStatus;

            if (model.InviteeType == "Author")
            {
                invitation.WillingToSpeak = model.WillingToSpeak;
            }

            db.SaveChanges();

            // If RSVP is accepted, generate and send the ticket
            if (model.RSVPStatus == true)
            {
                GenerateAndSendTicketForInvitee(invitation);
            }

            TempData["Message"] = "Thank you for your response!";
            return RedirectToAction("ThankYou");
        }

        //private void GenerateAndSendTicketForInvitee(Invitation invitation)
        //{
        //    string category = invitation.InviteeType == "Author" ? "VIP" : "MEDIA";

        //    // Retrieve event details
        //    var eventDetails = db.Events.Find(invitation.EventID);
        //    if (eventDetails == null)
        //    {
        //        return;
        //    }

        //    // Retrieve invitee details
        //    string inviteeName;
        //    string inviteeEmail;

        //    if (invitation.InviteeType == "Author")
        //    {
        //        var author = db.Authors.Find(invitation.InviteeID);
        //        inviteeName = author?.Name;
        //        inviteeEmail = author?.Email;
        //    }
        //    else
        //    {
        //        var mediaContact = db.Media.Find(invitation.InviteeID);
        //        inviteeName = mediaContact?.Name;
        //        inviteeEmail = mediaContact?.Email;
        //    }

        //    // Generate QR Code content
        //    string qrContent = $"InvitationID:{invitation.InvitationID};InviteeType:{invitation.InviteeType};EventID:{eventDetails.EventId};Date:{DateTime.Now}";
        //    string qrCodeBase64 = GenerateQRCode(qrContent);

        //    // Generate and save the ticket image
        //    string ticketImagePath = GenerateTicketImage(
        //        eventDetails.BookTitle,
        //        eventDetails.LaunchDate.ToString("f"),
        //        inviteeName,
        //        0, // Free ticket, so no price
        //        category,
        //        qrContent,
        //        1 // Quantity is 1 for invited guests
        //    );

        //    // Send the ticket via email
        //    SendTicketEmail(inviteeEmail, inviteeName, eventDetails.BookTitle, ticketImagePath);
        //}

        //sending ticket to invited
        private void GenerateAndSendTicketForInvitee(Invitation invitation)
        {
            string category = invitation.InviteeType == "Author" ? "VIP" : "MEDIA";

            // Retrieve event details
            var eventDetails = db.Events.Find(invitation.EventID);
            if (eventDetails == null)
            {
                return;
            }

            // Retrieve invitee details
            string inviteeName;
            string inviteeEmail;

            if (invitation.InviteeType == "Author")
            {
                var author = db.Authors.Find(invitation.InviteeID);
                inviteeName = author?.Name;
                inviteeEmail = author?.Email;
            }
            else
            {
                var mediaContact = db.Media.Find(invitation.InviteeID);
                inviteeName = mediaContact?.Name;
                inviteeEmail = mediaContact?.Email;
            }

            // Generate QR Code content
            string qrContent = $"InvitationID:{invitation.InvitationID};InviteeType:{invitation.InviteeType};EventID:{eventDetails.EventId};Date:{DateTime.Now}";
            string qrCodeBase64 = GenerateQRCode(qrContent);

            // Generate and save the ticket image
            string ticketImagePath = GenerateTicketImage(
                eventDetails.BookTitle,
                eventDetails.LaunchDate.ToString("f"),
                inviteeName,
                0, // Free ticket, so no price
                category,
                qrContent,
                1 // Quantity is 1 for invited guests
            );

            // Save the invited guest's ticket information to the InvitedGuestTicket table
            var invitedGuestTicket = new InvitedGuestTicket
            {
                InvitationID = invitation.InvitationID,
                InviteeName = inviteeName,
                InviteeType = invitation.InviteeType,
                IssuedDate = DateTime.Now,
                QRCode = qrCodeBase64,
                TicketImagePath = ticketImagePath
            };
            db.InvitedGuestTickets.Add(invitedGuestTicket);
            db.SaveChanges();

            // Send the ticket via email
            SendTicketEmail(inviteeEmail, inviteeName, eventDetails.BookTitle, ticketImagePath);
        }




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
            using (var qrCodeImage = qrCode.GetGraphic(8))
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
                        string connectionString = ""; //connection string to save ticket to blob
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

        private void SendTicketEmail(string email, string name, string eventTitle, string ticketImagePath)
        {
            var message = new MailMessage
            {
                From = new MailAddress("senderemail.com", "The Book Buffet"),
                Subject = $"Your Ticket for {eventTitle}",
                Body = $"Dear {name},<br/><br/>Thank you for your RSVP! Attached is your ticket for {eventTitle}.<br/><br/>Best Regards,<br/>The Book Buffet Team",
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



        public ActionResult RSVPError()
        {
            return View();
        }

        public ActionResult ThankYou()
        {
            return View();
        }


        //manage invitations

        public ActionResult ManageInvitations(string searchEventTitle)
        {
            var invitations = db.Invitations.Include("Event").ToList();

            if (!string.IsNullOrEmpty(searchEventTitle))
            {
                invitations = invitations.Where(i => i.Event.BookTitle.Contains(searchEventTitle)).ToList();
            }

            var groupedInvitations = invitations.GroupBy(i => i.EventID)
                                                .Select(g => new ManageInvitationsViewModel
                                                {
                                                    EventTitle = g.FirstOrDefault().Event.BookTitle,
                                                    EventDate = g.FirstOrDefault().Event.LaunchDate,  // Set the EventDate
                                                    Invitations = g.Select(inv => new InviteeDetailsViewModel
                                                    {
                                                        EventID = inv.EventID.Value,
                                                        InviteeName = inv.InviteeType == "Author"
                                                            ? db.Authors.Find(inv.InviteeID)?.Name
                                                            : db.Media.Find(inv.InviteeID)?.Name,
                                                        Email = inv.InviteeType == "Author"
                                                            ? db.Authors.Find(inv.InviteeID)?.Email
                                                            : db.Media.Find(inv.InviteeID)?.Email,
                                                        InviteeType = inv.InviteeType,
                                                        RSVPStatus = inv.RSVPStatus,
                                                        WillingToSpeak = inv.WillingToSpeak
                                                    }).ToList()
                                                }).ToList();

            return View(groupedInvitations);
        }

        //customer view of events
        // GET: Event/Index
        public ActionResult UpcomingEvents()
        {
            var events = db.Events.ToList();

            // Check if events is null or empty
            if (events == null || !events.Any())
            {
                // Handle the case where there are no events
                return View(new List<EventVenueViewModel>());
            }

            var eventVenueViewModels = events.Select(e => new EventVenueViewModel
            {
                Event = e,
                Venue = db.Bookings.Where(b => b.EventId == e.EventId).Select(b => b.Venue).FirstOrDefault(),
                Tickets = db.Tickets.Where(t => t.EventID == e.EventId).ToList()
            }).ToList();

            return View(eventVenueViewModels);
        }



        // GET: Event/EventDetails/{id}
    
        public ActionResult EventDetails(int id)
        {
            var eventDetails = db.Events.Find(id);
            if (eventDetails == null)
            {
                return HttpNotFound();
            }

            var venue = db.Bookings.Where(b => b.EventId == id).Select(b => b.Venue).FirstOrDefault();
            var tickets = db.Tickets.Where(t => t.EventID == id).ToList();

            var viewModel = new EventDetailsViewModel
            {
                Event = eventDetails,
                Venue = venue,
                Tickets = tickets.Select(t => new TicketViewModel
                {
                    Category = t.Category,
                    Price = t.Price,
                    RemainingTickets = t.RemainingTickets
                }).ToList()
            };

            return View(viewModel);
        }

        //reminders

       
        public ActionResult ManageReminders()
        {
            var model = db.Events
                .Where(e => e.LaunchDate > DateTime.Now)
                .Select(e => new EventReminderViewModel
                {
                    EventId = e.EventId,
                    BookTitle = e.BookTitle,
                    LaunchDate = e.LaunchDate,
                    RemindersSent = db.EmailReminders.Any(er => er.EventID == e.EventId && er.IsSent),
                    ReminderScheduled = db.EmailReminders.Any(er => er.EventID == e.EventId && !er.IsSent) // Check if there is an unsent scheduled reminder
                })
                .ToList();

            return View(model);
        }

       




        [HttpPost]
        public ActionResult SendReminderNow(int eventId)
        {
            var eventDetails = db.Events.Find(eventId);
            if (eventDetails == null) return Json(new { success = false, message = "Event not found." });

            // Get all ticket buyers
            var ticketSales = db.TicketSales
                                .Where(ts => ts.Ticket.EventID == eventId)
                                .Include(ts => ts.User)
                                .ToList();

            // Get all invited guests who have RSVPed
            var invitations = db.Invitations
                                .Where(i => i.EventID == eventId && i.RSVPStatus == true)
                                .ToList();

            // Send reminder to ticket buyers
            foreach (var sale in ticketSales)
            {
                SendReminderEmail(sale.User.Email, eventDetails.BookTitle, eventDetails.LaunchDate);
            }

            // Send reminder to invited guests
            foreach (var invite in invitations)
            {
                string email = invite.InviteeType == "Author"
                    ? db.Authors.Find(invite.InviteeID)?.Email
                    : db.Media.Find(invite.InviteeID)?.Email;

                if (!string.IsNullOrEmpty(email))
                {
                    SendReminderEmail(email, eventDetails.BookTitle, eventDetails.LaunchDate);
                }
            }

            // Check if a reminder record already exists
            var emailReminders = db.EmailReminders.Where(er => er.EventID == eventId && !er.IsSent).ToList();

            if (!emailReminders.Any())
            {
                // If no record exists, create a new one
                var newReminder = new EmailReminder
                {
                    EventID = eventId,
                    ScheduledTime = DateTime.Now, // Since it's an immediate send
                    IsSent = true
                };

                db.EmailReminders.Add(newReminder);
            }
            else
            {
                // Update the IsSent status in existing EmailReminders
                foreach (var reminder in emailReminders)
                {
                    reminder.IsSent = true;
                }
            }

            db.SaveChanges();

            return Json(new { success = true, message = "Reminders sent successfully!" });
        }




        [HttpPost]
        public ActionResult ScheduleReminder(int eventId, DateTime scheduledTime)
        {
            var reminder = new EmailReminder
            {
                EventID = eventId,
                ScheduledTime = scheduledTime,
                IsSent = false
            };

            db.EmailReminders.Add(reminder);
            db.SaveChanges();

            // Return the updated view model
            var eventReminderViewModel = db.Events
                .Where(e => e.EventId == eventId)
                .Select(e => new EventReminderViewModel
                {
                    EventId = e.EventId,
                    BookTitle = e.BookTitle,
                    LaunchDate = e.LaunchDate,
                    RemindersSent = db.EmailReminders.Any(er => er.EventID == e.EventId && er.IsSent),
                    ReminderScheduled = db.EmailReminders.Any(er => er.EventID == e.EventId && !er.IsSent) // Check if any unsent reminder is scheduled.
                })
                .FirstOrDefault();

            // Schedule the job using Hangfire or another background job system
            BackgroundJob.Schedule(() => SendReminderNowAndMarkSent(eventId), scheduledTime);

            return Json(new { success = true, message = "Reminder scheduled successfully!" });
        }

        public void SendReminderNowAndMarkSent(int eventId)
        {
            SendReminderNow(eventId);

            var emailReminders = db.EmailReminders.Where(er => er.EventID == eventId && er.ScheduledTime <= DateTime.Now).ToList();
            foreach (var reminder in emailReminders)
            {
                reminder.IsSent = true;
            }

            db.SaveChanges();
        }

        [HttpPost]
        public ActionResult CancelScheduledReminder(int eventId)
        {
            // Find the scheduled reminders for the event that haven't been sent yet
            var scheduledReminders = db.EmailReminders
                                        .Where(er => er.EventID == eventId && !er.IsSent)
                                        .ToList();

            if (!scheduledReminders.Any())
            {
                return Json(new { success = false, message = "No scheduled reminder found to cancel." });
            }

            // Remove these reminders from the database
            db.EmailReminders.RemoveRange(scheduledReminders);
            db.SaveChanges();

            return Json(new { success = true, message = "Scheduled reminder cancelled successfully!" });
        }



        private void SendReminderEmail(string email, string eventTitle, DateTime eventDate)
        {
            var message = new MailMessage
            {
                From = new MailAddress("senderemail.com", "The Book Buffet"),
                Subject = $"Reminder: {eventTitle} on {eventDate.ToString("f")}",
                Body = $"Dear Attendee,<br/><br/>This is a reminder for the upcoming event '{eventTitle}' happening on {eventDate.ToString("f")}.<br/><br/>Best Regards,<br/>The Book Buffet Team",
                IsBodyHtml = true
            };

            message.To.Add(new MailAddress(email));

            using (var smtpClient = new SmtpClient())
            {
                smtpClient.Send(message);
            }
        }

        public ActionResult EventSummary()
        {
            // Fetch all events first one first
            var events = db.Events.OrderByDescending(e => e.LaunchDate).ToList();

            // Prepare the view model with additional data
            var eventSummaryViewModelList = events.Select(e => new EventSummaryViewModel
            {
                EventId = e.EventId,
                EventName = e.BookTitle,
                LaunchDate = e.LaunchDate,
                Venue = db.Bookings.Where(b => b.EventId == e.EventId).Select(b => b.Venue).FirstOrDefault()?.Name ?? "No venue selected",
                AttendeesCount = db.TicketSales.Count(ts => ts.Ticket.EventID == e.EventId) + db.InvitedGuestTickets.Count(igt => igt.Invitation.EventID == e.EventId && igt.CheckedIn),
                GeneralTicketsSold = db.Tickets.Where(t => t.EventID == e.EventId && t.Category == "General").Select(t => (t.TotalTickets - t.RemainingTickets)).DefaultIfEmpty(0).Sum(),
                VIPTicketsSold = db.Tickets.Where(t => t.EventID == e.EventId && t.Category == "VIP").Select(t => (t.TotalTickets - t.RemainingTickets)).DefaultIfEmpty(0).Sum(),
                VVIPTicketsSold = db.Tickets.Where(t => t.EventID == e.EventId && t.Category == "VVIP").Select(t => (t.TotalTickets - t.RemainingTickets)).DefaultIfEmpty(0).Sum(),
                GeneralTicketPrice = db.Tickets.Where(t => t.EventID == e.EventId && t.Category == "General").Select(t => t.Price).FirstOrDefault(),
                VIPTicketPrice = db.Tickets.Where(t => t.EventID == e.EventId && t.Category == "VIP").Select(t => t.Price).FirstOrDefault(),
                VVIPTicketPrice = db.Tickets.Where(t => t.EventID == e.EventId && t.Category == "VVIP").Select(t => t.Price).FirstOrDefault(),
                AverageRating = db.Surveys.Where(s => s.EventID == e.EventId).Average(s => (double?)s.TotalRating) ?? 0.0,
                TotalRatings = db.Surveys.Count(s => s.EventID == e.EventId)
            }).ToList();

            return View(eventSummaryViewModelList);
        }















    }
}