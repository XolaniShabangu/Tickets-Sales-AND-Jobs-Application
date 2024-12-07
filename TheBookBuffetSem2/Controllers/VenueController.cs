using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using TheBookBuffetSem2.Models;

namespace TheBookBuffetSem2.Controllers
{
    public class VenueController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            var venues = db.Venues.ToList();
            return View(venues);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            Venue venue = db.Venues.Find(id);
            if (venue == null)
            {
                return HttpNotFound();
            }

            return View(venue);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Venue venue, HttpPostedFileBase photo)
        {
            if (ModelState.IsValid)
            {
                if (photo != null && photo.ContentLength > 0)
                {
                    // Update the photo if a new one is uploaded
                    string photoUrl = await UploadPhotoToBlob(photo);
                    venue.Photos = photoUrl;
                }

                db.Entry(venue).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(venue);
        }



        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Venue venue, HttpPostedFileBase photo)
        {
            if (ModelState.IsValid)
            {
                // Upload photo to Azure Blob Storage
                if (photo != null && photo.ContentLength > 0)
                {
                    string photoUrl = await UploadPhotoToBlob(photo);
                    venue.Photos = photoUrl;
                }

                db.Venues.Add(venue);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(venue);
        }

        private async Task<string> UploadPhotoToBlob(HttpPostedFileBase photo)
        {
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(""); //connection strings

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container.
            CloudBlobContainer container = blobClient.GetContainerReference("appdevproject");

            // Create the container if it doesn't already exist.
            await container.CreateIfNotExistsAsync();
            await container.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

            // Retrieve reference to a blob named by the uploaded file's name.
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName));

            // Upload the file
            using (var fileStream = photo.InputStream)
            {
                await blockBlob.UploadFromStreamAsync(fileStream);
            }

            // Return the URL of the uploaded file
            return blockBlob.Uri.ToString();
        }






        //venue selection

        public ActionResult SelectVenue(int eventId, string searchString, string returnUrl)
        {
            // Retrieve the event details to get the event date
            var eventDetails = db.Events.Find(eventId);
            if (eventDetails == null)
            {
                return HttpNotFound("Event not found");
            }

            var eventDate = eventDetails.LaunchDate.Date;  // Extract only the date part

            // Start with the base query for venues
            var venues = from v in db.Venues
                         select v;

            // Apply search filtering if a search string is provided
            if (!String.IsNullOrEmpty(searchString))
            {
                venues = venues.Where(v => v.Name.Contains(searchString) || v.Location.Contains(searchString));
            }

            var venuesList = venues.ToList();

            // Get all bookings on the same date as the event
            var bookingsOnSameDate = db.Bookings
                .Where(b => DbFunctions.TruncateTime(b.Event.LaunchDate) == eventDate)
                .Select(b => b.VenueId)
                .ToList();

            // Check if each venue is booked on the event date (ignore time)
            var venueListWithAvailability = venuesList.Select(v => new VenueViewModel
            {
                Venue = v,
                IsBooked = bookingsOnSameDate.Contains(v.VenueID)
            }).ToList();

            ViewBag.ReturnId=returnUrl;
            ViewBag.EventId = eventId;
            return View(venueListWithAvailability);  // Pass the correct model to the view
        }


        // POST: Venue/BookVenue
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BookVenue(int eventId, int venueId, string returnUrl = null)
        {
            var booking = db.Bookings.FirstOrDefault(b => b.EventId == eventId);

            if (booking != null)
            {
                // Update the existing booking with the new venue
                booking.VenueId = venueId;
                booking.DateBooked = DateTime.Now;
            }
            else
            {
                // Create a new booking if it doesn't exist
                booking = new Booking
                {
                    EventId = eventId,
                    VenueId = venueId,
                    DateBooked = DateTime.Now
                };
                db.Bookings.Add(booking);
            }

            db.SaveChanges();

            // Redirect based on the returnUrl
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Default redirection if no returnUrl is provided
            return RedirectToAction("Details", "Event", new { id = eventId});
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
    }
}