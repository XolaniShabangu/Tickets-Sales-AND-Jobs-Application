using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TheBookBuffetSem2.Models;
using TheBookBuffetSem2.Models.Careers;
using Newtonsoft.Json;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;
using System.IO;
using Tweetinvi.Exceptions;
using Microsoft.AspNet.Identity;
using Azure.Storage.Blobs;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System.Threading;
using System.Text.RegularExpressions;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System.Net.Mail;
using Google.Apis.Calendar.v3.Data;
using Azure.Storage.Blobs.Models;
using NAudio.Wave;





namespace TheBookBuffetSem2.Controllers
{
    public class JobsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [Authorize(Roles = "Recruiter")]
        public ActionResult Dashboard()
        {
            // Get all published jobs
            var jobs = db.Jobs.Where(j => j.Published).ToList();

            // Prepare data for the graph
            var jobTitles = jobs.Select(j => j.Title).ToList();
            var totalApplicants = jobs.Select(j => db.Applicants.Count(a => a.JobId == j.JobId)).ToList();
            var hiredApplicants = jobs.Select(j => db.Applicants.Count(a => a.JobId == j.JobId && a.JobStatus == "Accepted")).ToList();

            // Pass data to the view
            ViewBag.JobTitles = jobTitles;
            ViewBag.TotalApplicants = totalApplicants;
            ViewBag.HiredApplicants = hiredApplicants;

            return View();
        }







        // GET: Jobs
        public ActionResult Index(string sortOrder)
        {
            ViewBag.TitleSortParam = sortOrder == "title_asc" ? "title_desc" : "title_asc";
            ViewBag.DateSortParam = sortOrder == "date_asc" ? "date_desc" : "date_asc";
            ViewBag.CreatedSortParam = sortOrder == "created_asc" ? "created_desc" : "created_asc";

            var jobs = db.Jobs.AsQueryable();

            switch (sortOrder)
            {
                case "title_desc":
                    jobs = jobs.OrderByDescending(j => j.Title);
                    break;
                case "title_asc":
                    jobs = jobs.OrderBy(j => j.Title);
                    break;
                case "date_desc":
                    jobs = jobs.OrderByDescending(j => j.ClosingDate);
                    break;
                case "date_asc":
                    jobs = jobs.OrderBy(j => j.ClosingDate);
                    break;
                case "created_desc":
                    jobs = jobs.OrderByDescending(j => j.CreatedDate);
                    break;
                case "created_asc":
                    jobs = jobs.OrderBy(j => j.CreatedDate);
                    break;
                default:
                    jobs = jobs.OrderByDescending(j => j.CreatedDate); // Default sort by CreatedDate descending
                    break;
            }

            return View(jobs.ToList());
        }


        // GET: Jobs/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Job job = db.Jobs.Find(id);
            if (job == null)
            {
                return HttpNotFound();
            }
            return View(job);
        }

        // GET: Jobs/Create
        public ActionResult Create()
        {
            return View();
        }



        // POST: Jobs/Create
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "JobId,Title,Description,Qualifications,OcrKeywords,Published,ClosingDate")] Job job)
        {
            if (ModelState.IsValid)
            {
                // Set the CreatedDate here
                job.CreatedDate = DateTime.Now;

                // Ensure Published is false on creation
                job.Published = false;
                job.NumberOfPositions = 50;

                // Add the job to the database
                db.Jobs.Add(job);
                db.SaveChanges();

                // Set success message in TempData
                TempData["SuccessMessage"] = "Job draft created successfully!";

                // Redirect to the Dashboard
                return RedirectToAction("Dashboard");
            }

            // If the model is invalid, return to the form
            return View(job);
        }



        // GET: Jobs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Job job = db.Jobs.Find(id);
            if (job == null)
            {
                return HttpNotFound();
            }
            return View(job);
        }

        // POST: Jobs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "JobId,Title,Description,Qualifications,OcrKeywords,Published,ClosingDate,CreatedDate")] Job job)
        {
            if (ModelState.IsValid)
            {
                db.Entry(job).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(job);
        }

        // GET: Jobs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Job job = db.Jobs.Find(id);
            if (job == null)
            {
                return HttpNotFound();
            }
            return View(job);
        }

        // POST: Jobs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Job job = db.Jobs.Find(id);
            db.Jobs.Remove(job);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }



        //publish
        //[HttpGet]
        //public ActionResult Publish(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }

        //    var job = db.Jobs.Find(id);
        //    if (job == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    // Load the image and overlay the job title at the desired position
        //    string imagePath = Server.MapPath("~/img/Job-Poster.png");

        //    // Generate a unique filename for the new image
        //    string outputPath = Server.MapPath($"~/img/Job-Poster-{job.JobId}.jpg");

        //    try
        //    {
        //        using (Image img = Image.FromFile(imagePath))
        //        using (Graphics g = Graphics.FromImage(img))
        //        {
        //            // Set font, brush, and position for the job title
        //            Font font = new Font("Arial", 40, FontStyle.Bold);
        //            Brush brush = new SolidBrush(Color.White);
        //            PointF position = new PointF(100, 100); // Adjust this based on where you want the title to be

        //            // Draw the job title onto the image
        //            g.DrawString(job.Title, font, brush, position);

        //            // Save the new image (ensure it's saved as a different file, not overwriting the template)
        //            img.Save(outputPath);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle any errors that occur while saving the image
        //        ViewBag.ErrorMessage = "An error occurred while generating the image: " + ex.Message;
        //        return View("Error");
        //    }

        //    // Pass the new image path to the view
        //    ViewBag.ImagePath = $"/img/we-are-hiring-{job.JobId}.jpg";

        //    // Redirect to the publish preview page
        //    return View("PublishPreview", job);
        //}

        //    [HttpGet]
        //    public ActionResult Publish(int? id)
        //    {
        //        if (id == null)
        //        {
        //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //        }

        //        var job = db.Jobs.Find(id);
        //        if (job == null)
        //        {
        //            return HttpNotFound();
        //        }

        //        // Load the image and overlay the job title at the desired position
        //        string imagePath = Server.MapPath("~/img/Job-Poster.png");

        //        // Generate a unique filename for the new image
        //        string outputPath = Server.MapPath($"~/img/Job-Poster-{job.JobId}.jpg");

        //        try
        //        {
        //            using (Image img = Image.FromFile(imagePath))
        //            using (Graphics g = Graphics.FromImage(img))
        //            {
        //                // Set font, brush, and position for the job title
        //                Font font = new Font("Arial", 50, FontStyle.Bold);
        //                // Use ColorTranslator to convert hex code to a Color object
        //                Color customColor = ColorTranslator.FromHtml("#fad02c");
        //                Brush brush = new SolidBrush(customColor);

        //                // Center the text horizontally and vertically
        //                SizeF textSize = g.MeasureString(job.Title.ToUpper(), font);
        //                float xPosition = (img.Width - textSize.Width) / 2;
        //                float yPosition = (img.Height - textSize.Height) / 2;

        //                // Draw the job title in yellow and all caps
        //                g.DrawString(job.Title.ToUpper(), font, brush, new PointF(xPosition, yPosition));

        //                // Save the new image (ensure it's saved as a different file, not overwriting the template)
        //                img.Save(outputPath);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            // Handle any errors that occur while saving the image
        //            ViewBag.ErrorMessage = "An error occurred while generating the image: " + ex.Message;
        //            return View("Error");
        //        }

        //        // Pass the new image path to the view
        //        ViewBag.ImagePath = $"/img/Job-Poster-{job.JobId}.jpg";

        //        // Redirect to the publish preview page
        //        return View("PublishPreview", job);
        //    }





        //    //post to facebook


        //    public async Task<ActionResult> PostToFacebook(int id, string message)
        //    {
        //        // Get the job by id
        //        var job = db.Jobs.Find(id);
        //        if (job == null)
        //        {
        //            return HttpNotFound();
        //        }

        //        // Replace with your temporary access token from Graph API Explorer
        //        string accessToken = "EAAdOPkflxdYBO8viN4kdq1Jin6hGK66BP8eXvVh3z50yPQZC8AM3y5tff7Wv7w4PJ5rPLZCvNhvIuJaukK78U860W2xxc23tFDTarJ2aO8ZAZBu32ZANZBru1CgYYXVfVHQ9KZBWSbuHOMU5Q2HWz7OV3kzuJIog9KiMY90urwgUhYXFBn76ZB3odt5B9SyG43qjiSBxyK6KmvApEzZCJgyQuldJXweZCiU2Iu";
        //        string pageId = "402673519596997"; // Replace with your Facebook Page ID

        //        // Define the message to post
        //        var postMessage = $"🚨We Are Hiring🚨\n\n{job.Title}\n\n{job.Description}\n\n✅ {job.Qualifications}\n\nApply before: {job.ClosingDate.ToString("d MMM yyyy")}";

        //        // Path to the image generated in the Publish method
        //        string imagePath = Server.MapPath($"~/img/Job-Poster-{job.JobId}.jpg");

        //        // Create the request URL for photo upload
        //        string url = $"https://graph.facebook.com/v21.0/{pageId}/photos";

        //        // Create the multipart form data request
        //        var parameters = new MultipartFormDataContent
        //{
        //    { new StringContent(postMessage), "message" },
        //    { new ByteArrayContent(System.IO.File.ReadAllBytes(imagePath)), "source", "Job-Poster.jpg" }, // Read the image as byte array and send it
        //    { new StringContent(accessToken), "access_token" }
        //};

        //        try
        //        {
        //            // Use HttpClient to post to Facebook
        //            using (var client = new HttpClient())
        //            {
        //                var response = await client.PostAsync(url, parameters);

        //                // Check if the request was successful
        //                if (response.IsSuccessStatusCode)
        //                {
        //                    // Mark the job as published in the database
        //                    job.Published = true;
        //                    db.Entry(job).State = EntityState.Modified;
        //                    db.SaveChanges(); // Save the changes to the database

        //                    // Set a flag to open the Facebook page in the view
        //                    TempData["FacebookSuccess"] = true;

        //                    TempData["SuccessMessage"] = "Job successfully posted to Facebook!";
        //                    ViewBag.SuccessMessage = "Job successfully posted to Facebook with image!";
        //                }
        //                else
        //                {
        //                    TempData["ErrorMessage"] = "Failed to post to Facebook. " + response.ReasonPhrase;
        //                    ViewBag.ErrorMessage = "Failed to post to Facebook. " + response.ReasonPhrase;
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            // Handle any errors that occur during the request
        //            ViewBag.ErrorMessage = "An error occurred while posting to Facebook: " + ex.Message;
        //        }

        //        // Use TempData to pass information to the view for this one request
        //        bool facebookSuccess = TempData.ContainsKey("FacebookSuccess") && (bool)TempData["FacebookSuccess"];

        //        // Clear TempData before rendering the view to prevent future triggers
        //        TempData.Remove("FacebookSuccess");

        //        // Redirect to the dashboard with the success or error message
        //        return RedirectToAction("Dashboard", new { id = job.JobId, facebookSuccess = facebookSuccess });

        //    }


        [HttpGet]
        public async Task<ActionResult> Publish(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var job = db.Jobs.Find(id);
            if (job == null)
            {
                return HttpNotFound();
            }

            // Azure Blob Storage settings
            string blobConnectionString = ""; 
            string containerName = "poster"; // The container where images are stored
            string inputBlobName = "Job-Poster.png"; // The original image in blob storage
            string outputBlobName = $"Job-Poster-{job.JobId}.jpg"; // The new image to be saved to blob

            BlobServiceClient blobServiceClient = new BlobServiceClient(blobConnectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Get the original image from Blob Storage
            BlobClient inputBlobClient = containerClient.GetBlobClient(inputBlobName);
            BlobClient outputBlobClient = containerClient.GetBlobClient(outputBlobName);

            using (MemoryStream imageStream = new MemoryStream())
            {
                await inputBlobClient.DownloadToAsync(imageStream);
                imageStream.Position = 0;

                try
                {
                    using (Image img = Image.FromStream(imageStream))
                    using (Graphics g = Graphics.FromImage(img))
                    {
                        // Set font, brush, and position for the job title
                        Font font = new Font("Arial", 50, FontStyle.Bold);
                        Color customColor = ColorTranslator.FromHtml("#fad02c");
                        Brush brush = new SolidBrush(customColor);

                        // Center the text horizontally and vertically
                        SizeF textSize = g.MeasureString(job.Title.ToUpper(), font);
                        float xPosition = (img.Width - textSize.Width) / 2;
                        float yPosition = (img.Height - textSize.Height) / 2;

                        // Draw the job title in yellow and all caps
                        g.DrawString(job.Title.ToUpper(), font, brush, new PointF(xPosition, yPosition));

                        // Save the modified image to a memory stream
                        using (MemoryStream outputStream = new MemoryStream())
                        {
                            img.Save(outputStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                            outputStream.Position = 0;

                            // Upload the modified image back to Azure Blob Storage
                            await outputBlobClient.UploadAsync(outputStream, overwrite: true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = "An error occurred while generating the image: " + ex.Message;
                    return View("Error");
                }
            }

            // Pass the new image URL to the view
            ViewBag.ImagePath = outputBlobClient.Uri.ToString();

            // Redirect to the publish preview page
            return View("PublishPreview", job);
        }


        public async Task<ActionResult> PostToFacebook(int id, string message)
        {
            var job = db.Jobs.Find(id);
            if (job == null)
            {
                return HttpNotFound();
            }

            string accessToken = "";
            string pageId = ""; //fb page id

            // Define the message to post
            var postMessage = $"🚨We Are Hiring🚨\n\n{job.Title}\n\n{job.Description}\n\n✅ {job.Qualifications}\n\nApply before: {job.ClosingDate.ToString("d MMM yyyy")}";

            // Path to the image in Blob Storage (from the Publish method)
            string imageUrl = $"https://appdev3project.blob.core.windows.net/poster/Job-Poster-{job.JobId}.jpg";

            // Create the request URL for photo upload
            string url = $"https://graph.facebook.com/v21.0/{pageId}/photos";

            // Create the multipart form data request
            var parameters = new MultipartFormDataContent
    {
        { new StringContent(postMessage), "message" },
        { new StringContent(imageUrl), "url" }, // Send the image URL instead of uploading the file
        { new StringContent(accessToken), "access_token" }
    };

            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.PostAsync(url, parameters);

                    if (response.IsSuccessStatusCode)
                    {
                        job.Published = true;
                        db.Entry(job).State = EntityState.Modified;
                        db.SaveChanges();

                        TempData["FacebookSuccess"] = true;
                        TempData["SuccessMessage"] = "Job successfully posted to Facebook!";
                        ViewBag.SuccessMessage = "Job successfully posted to Facebook with image!";
                    }
                    else
                    {
                        // Capture error response from Facebook
                        string errorMessage = await response.Content.ReadAsStringAsync();
                        TempData["ErrorMessage"] = $"Failed to post to Facebook: {response.ReasonPhrase}. Details: {errorMessage}";
                        ViewBag.ErrorMessage = $"Failed to post to Facebook: {response.ReasonPhrase}. Details: {errorMessage}";
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception and set the error message
                TempData["ErrorMessage"] = $"An error occurred while posting to Facebook: {ex.Message}";
                ViewBag.ErrorMessage = $"An error occurred while posting to Facebook: {ex.Message}";
            }
            finally
            {
                // Ensure the job is marked as published even if Facebook post fails
                job.Published = true;
                db.Entry(job).State = EntityState.Modified;
                db.SaveChanges();
            }

            bool facebookSuccess = TempData.ContainsKey("FacebookSuccess") && (bool)TempData["FacebookSuccess"];
            TempData.Remove("FacebookSuccess");

            return RedirectToAction("Dashboard", new { id = job.JobId, facebookSuccess = facebookSuccess });
        }






        // Action to display the AccessJobs view
        public ActionResult AccessJobs()
        {
            // Get only published jobs where the closing date is in the future
            var publishedJobs = db.Jobs
                .Where(j => j.Published && j.ClosingDate >= DateTime.Now)
                .ToList();

            // Get the current logged-in user's information
            var userId = User.Identity.GetUserId();

            // Check which jobs the user has already applied for
            var appliedJobIds = db.Applicants
                .Where(a => a.UserId == userId)
                .Select(a => a.JobId)
                .ToList();

            // Pass both jobs and applied job ids to the view
            ViewBag.AppliedJobIds = appliedJobIds;

            return View(publishedJobs);
        }




        //apply
        // GET: Apply for a Job
        [HttpGet]
        [Authorize] // Ensure the user is logged in
        public ActionResult Apply(int jobId)
        {
            // Find the job by ID
            var job = db.Jobs.Find(jobId);
            if (job == null)
            {
                return HttpNotFound("Job not found.");
            }

            // Get the current logged-in user's information
            var userId = User.Identity.GetUserId(); // Get the logged-in user's ID
            var user = db.Users.Find(userId); // Assuming you have access to the user details

            if (user == null || !User.IsInRole("Customer"))
            {
                return RedirectToAction("Login", "Account");
            }

            // Prefill the applicant model with user data (Name + Surname = FullName)
            var model = new Applicant
            {
                FullName = $"{user.Name} {user.Surname}",
                Email = user.Email,
                MobileNumber = user.MobileNumber,
                JobId = jobId, // Set the JobId for the application
                Job = job // Pass the job details for display
            };
            ModelState.Clear();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitApplication(Applicant model, HttpPostedFileBase CV, HttpPostedFileBase IDDocument, HttpPostedFileBase ExtraDocument)
        {
            if (!ModelState.IsValid)
            {
                foreach (var modelStateKey in ModelState.Keys)
                {
                    var value = ModelState[modelStateKey];
                    foreach (var error in value.Errors)
                    {
                        // Log the errors or display them for debugging purposes
                        System.Diagnostics.Debug.WriteLine($"Key: {modelStateKey}, Error: {error.ErrorMessage}");
                    }
                }

                return View("Apply", model); // Return the invalid model to the view for display
            }

            // Azure Blob Storage configuration
            string connectionString = "";
            string containerName = "cvid"; // Replace with your actual container name

            // Set the UserId to the currently logged-in user's ID
            model.UserId = User.Identity.GetUserId();

            // Initialize the Blob service client
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Upload CV to Blob Storage
            if (CV != null && CV.ContentLength > 0)
            {
                string cvBlobName = Guid.NewGuid().ToString() + Path.GetExtension(CV.FileName);
                BlobClient cvBlobClient = containerClient.GetBlobClient(cvBlobName);

                using (var stream = CV.InputStream)
                {
                    await cvBlobClient.UploadAsync(stream, true);
                }

                model.CVBlobUrl = cvBlobClient.Uri.ToString(); // Save the CV URL to the model
            }

            // Upload ID Document to Blob Storage
            if (IDDocument != null && IDDocument.ContentLength > 0)
            {
                string idBlobName = Guid.NewGuid().ToString() + Path.GetExtension(IDDocument.FileName);
                BlobClient idBlobClient = containerClient.GetBlobClient(idBlobName);

                using (var stream = IDDocument.InputStream)
                {
                    await idBlobClient.UploadAsync(stream, true);
                }

                model.IDBlobUrl = idBlobClient.Uri.ToString(); // Save the ID Document URL to the model
            }

            // Upload Extra Document to Blob Storage (optional)
            if (ExtraDocument != null && ExtraDocument.ContentLength > 0)
            {
                string extraBlobName = Guid.NewGuid().ToString() + Path.GetExtension(ExtraDocument.FileName);
                BlobClient extraBlobClient = containerClient.GetBlobClient(extraBlobName);

                using (var stream = ExtraDocument.InputStream)
                {
                    await extraBlobClient.UploadAsync(stream, true);
                }

                model.ExtraDocumentBlobUrl = extraBlobClient.Uri.ToString(); // Save the Extra Document URL to the model
            }

            // Save applicant data temporarily
            db.Applicants.Add(model);
            await db.SaveChangesAsync();

            // Run the OCR process
            var ocrResult = await RunOCRProcess(model);

            // Check if OCR passed or failed
            if (!ocrResult.Success)
            {
                // Delete the applicant record if OCR fails
                db.Applicants.Remove(model);
                await db.SaveChangesAsync();
                // Redirect to the failure page with the message
                return RedirectToAction("ApplicationFailed", new { message = ocrResult.Message });
            }

            // If OCR succeeds, update the applicant status
            model.OCRStatus = "Passed";
            model.OCRAccuracy = ocrResult.Accuracy.HasValue ? Math.Round(ocrResult.Accuracy.Value, 2) : 0.0;
            model.OCRPassedDate = DateTime.Now;
            await db.SaveChangesAsync();

            // Redirect to success page
            return RedirectToAction("ApplicationSuccess", new { applicantId = model.ApplicantId, accuracy = ocrResult.Accuracy });
        }

        //OCR Scanning starts here

        

        private async Task<OCRResult> RunOCRProcess(Applicant applicant)
        {
            try
            {
                // Perform OCR on the CV and ID document
                var cvText = await RunAzureOCR(applicant.CVBlobUrl);
                var idText = await RunAzureOCR(applicant.IDBlobUrl);

                // Validate ID details (100% accuracy required)
                string idValidationMessage;
                bool isValidID = ValidateIDDetails(applicant.FullName, applicant.IDNumber, idText, out idValidationMessage);
                if (!isValidID)
                {
                    return new OCRResult
                    {
                        Success = false,
                        Message = idValidationMessage
                    };
                }

                // Match CV keywords (60% accuracy required)
                var job = db.Jobs.Find(applicant.JobId);
                double accuracy;
                bool matchedKeywords = MatchCVKeywords(cvText, job.OcrKeywords, out accuracy);
                if (!matchedKeywords)
                {
                    return new OCRResult
                    {
                        Success = false,
                        Message = $"You don't fit what we're looking for. CV Accuracy: {accuracy}%",
                        Accuracy = accuracy
                    };
                }

                // If both validations pass
                return new OCRResult { Success = true, Accuracy = accuracy };
            }
            catch (Exception ex)
            {
                return new OCRResult { Success = false, Message = "An internal error occurred: " + ex.Message };
            }
        }


        private async Task<string> RunAzureOCR(string blobUrl)
        {
            try
            {
                // Your Azure credentials
                string subscriptionKey = "";
                string endpoint = "https://bookshopocr.cognitiveservices.azure.com/";

                // Create the client
                var client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(subscriptionKey))
                {
                    Endpoint = endpoint
                };

                // Run OCR on the document
                var textHeaders = await client.ReadAsync(blobUrl);
                string operationLocation = textHeaders.OperationLocation;

                // Poll for results (adjust delay if needed)
                Thread.Sleep(2000);
                var readResults = await client.GetReadResultAsync(Guid.Parse(operationLocation.Split('/').Last()));

                // Extract text from the results
                var extractedText = string.Join(" ", readResults.AnalyzeResult.ReadResults.SelectMany(r => r.Lines.Select(l => l.Text)));
                return extractedText;
            }
            catch (Exception ex)
            {
                // Log the exception details
                System.Diagnostics.Debug.WriteLine("Error during OCR process: " + ex.Message);
                throw;
            }
        }



        

        private bool MatchCVKeywords(string cvText, string keywords, out double accuracy)
        {
            // Split the keywords by commas and trim spaces
            var keywordList = keywords.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(k => k.Trim())  // Remove any leading/trailing spaces
                                      .ToList();

            int totalKeywords = keywordList.Count;
            int matchedKeywords = 0;

            // Loop through each keyword
            foreach (var keyword in keywordList)
            {
                // Split each keyword into individual words
                var keywordWords = keyword.Split(' ').Select(w => w.Trim()).ToList();
                bool allWordsMatch = true;

                // Check if all words in the keyword exist in the CV text
                foreach (var word in keywordWords)
                {
                    if (cvText.IndexOf(word, StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        allWordsMatch = false;
                        break;
                    }
                }

                // If all words of the keyword were found in the CV, count it as a match
                if (allWordsMatch)
                {
                    matchedKeywords++;
                }
            }

            // Calculate accuracy percentage
            accuracy = ((double)matchedKeywords / totalKeywords) * 100;

            // Pass if 60% or more of the keywords are present in the CV text
            return accuracy >= 60;
        }





        private bool ValidateIDDetails(string fullName, string idNumber, string idText, out string idValidationMessage)
        {
            idValidationMessage = string.Empty;

            // Check if the document contains "REPUBLIC OF SOUTH AFRICA NATIONAL IDENTITY CARD"
            if (idText.IndexOf("REPUBLIC OF SOUTH AFRICA NATIONAL IDENTITY CARD", StringComparison.OrdinalIgnoreCase) < 0)
            {
                idValidationMessage = "The uploaded document does not appear to be a valid South African ID. Please upload a clearer scan if you believe this is an error.";
                return false;
            }

            // Extract name, surname, and ID number (based on known ID structure)
            string extractedSurname = ExtractValueFromID(idText, "Surname", "Names");
            string extractedNames = ExtractValueFromID(idText, "Names", "Sex");
            string extractedIDNumber = ExtractValueFromID(idText, "Identity Number", "Date of Birth");

            // Split fullName and extractedNames into individual words for flexible matching
            var fullNameParts = fullName.Split(' ').Select(p => p.Trim()).ToArray();
            var extractedNameParts = extractedNames.Split(' ').Select(p => p.Trim()).ToArray();

            // Compare surname
            bool surnameMatches = !string.IsNullOrEmpty(extractedSurname) && fullNameParts.Last().Equals(extractedSurname, StringComparison.OrdinalIgnoreCase);

            // Compare first name (check if any name in fullName matches any name in extractedNames)
            bool firstNameMatches = fullNameParts.Any(part => extractedNameParts.Contains(part, StringComparer.OrdinalIgnoreCase));

            // Compare ID number
            bool idNumberMatches = extractedIDNumber == idNumber;

            // If any of the fields don't match, return a detailed error message
            if (!surnameMatches || !firstNameMatches || !idNumberMatches)
            {
                idValidationMessage = $"ID validation failed. Expected: Name: {fullName}, ID Number: {idNumber}. Found: Surname: {extractedSurname}, Names: {extractedNames}, Identity Number: {extractedIDNumber}. Please upload a clearer scan if you believe this is incorrect.";
                return false;
            }

            return true;
        }



        private string ExtractValueFromID(string idText, string fieldName, string nextFieldName)
        {
            // Build a regex to find the value between the fieldName (with or without colon) and the nextFieldName
            string pattern = $"{fieldName}\\s*:?\\s*(.*?)\\s*{nextFieldName}";
            var match = Regex.Match(idText, pattern, RegexOptions.IgnoreCase);

            if (match.Success)
            {
                return match.Groups[1].Value.Trim(); // Extract the value between the two fields
            }

            return string.Empty;
        }


        public ActionResult ApplicationFailed(string message)
        {
            ViewBag.Message = message;
            return View();
        }

        public ActionResult ApplicationSuccess(int applicantId, double accuracy)
        {
            // Find the applicant by ID
            var applicant = db.Applicants.Find(applicantId);
            if (applicant == null)
            {
                return HttpNotFound("Applicant not found.");
            }

            // Pass the accuracy to the view using ViewBag
            ViewBag.Accuracy = accuracy;

            // Return the success view with the applicant data
            return View(applicant);
        }


       

        //Recruiter tracks applications
        public ActionResult Track(int id)
        {
            // Get the job by its ID
            var job = db.Jobs.Find(id);
            if (job == null)
            {
                return HttpNotFound("Job not found.");
            }

            // Get all applicants for this job
            var applicants = db.Applicants.Where(a => a.JobId == id).ToList();

            // Sort applicants only if they have both OCR and AudioSample scores, but still pass all applicants to the view
            var sortedApplicants = applicants
                .Where(a => a.OCRAccuracy.HasValue && a.AudioSampleScore.HasValue && a.OverallScore >= 70)
                .OrderByDescending(a => a.OverallScore)
                .Take(job.NumberOfPositions)  // Limit to the number of positions
                .ToList();

            // Pass both the job and applicants (sorted and unsorted) to the view
            var model = new JobTrackingViewModel
            {
                Job = job,
                Applicants = applicants,
                RecommendedApplicants = sortedApplicants // For sorting recommendations only
            };

            

            return View(model);
        }


        public async Task<ActionResult> ViewDocument(string documentUrl)
        {
            // Initialize the Blob service client
            string connectionString = "";
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            // Parse the URL to get the container name and blob name
            Uri blobUri = new Uri(documentUrl);
            string containerName = blobUri.Segments[1].TrimEnd('/');
            string blobName = string.Join("", blobUri.Segments.Skip(2));  // The blob name starts after the container in the URL

            // Get the container client
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Get the blob client using the blob name
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            // Download the file as a stream
            var response = await blobClient.DownloadAsync();
            Stream stream = response.Value.Content;

            // Return the file as a PDF (for inline viewing)
            return File(stream, "application/pdf");
        }



        //Applicant tracks applications

        [Authorize]  // Ensure the user is logged in
        public ActionResult TrackApplications()
        {
            // Get the current logged-in user's ID
            var userId = User.Identity.GetUserId();

            // Fetch the applications for the current user
            var applications = db.Applicants
                .Where(a => a.UserId == userId)
                .Include(a => a.Job)  // Include the job details
                .ToList();

            // Return the view with the list of applications
            return View(applications);
        }

        [HttpPost]
        [Authorize]
        public ActionResult CancelApplication(int applicantId)
        {
            var applicant = db.Applicants.Find(applicantId);
            if (applicant == null)
            {
                return HttpNotFound("Applicant not found.");
            }

            // Mark the application as canceled
            applicant.IsCanceled = true;  // Assuming `IsCanceled` is a boolean field

            // Save changes to the database
            db.SaveChanges();

            // Optionally, set a success message using TempData
            TempData["SuccessMessage"] = "Application canceled successfully.";

            return RedirectToAction("TrackApplications", "Jobs");
        }





        //Audio Sample

        

        private static readonly string StaticText = "The sun rises in the east and sets in the west. Every morning, people wake up and start their day with new energy. Walking in the park can help refresh your mind and body.";  // The paragraph to be read

        // Get audio sample page
        [HttpGet]
        [Authorize]
        public ActionResult DoAudioSample(int applicantId)
        {
            var applicant = db.Applicants.Find(applicantId);
            if (applicant == null)
            {
                return HttpNotFound("Applicant not found.");
            }

            ViewBag.ApplicantId = applicantId;
            ViewBag.TextToRead = StaticText;  // Display the static text to the applicant

            return View();
        }

        // Post the audio sample for processing
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> SubmitAudioSample(int applicantId, HttpPostedFileBase audioFile, int retryCount)
        {
            var applicant = db.Applicants.Find(applicantId);
            if (applicant == null)
            {
                return HttpNotFound("Applicant not found.");
            }

            if (audioFile == null || audioFile.ContentLength == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "No audio file received");
            }

            // Save the audio file to Azure Blob Storage and get the Blob URL
            string blobUrl = await SaveAudioToBlobStorage(audioFile);

            // Call Azure Speech Service to transcribe the audio
            string recognizedText = await RecognizeSpeechAsync(blobUrl);

            // Calculate the similarity score using Levenshtein distance
            double similarityScore = CompareTextsUsingLevenshtein(StaticText, recognizedText);

            // Update applicant's audio submission details
            applicant.AudioBlobUrl = blobUrl;  // Store the audio file URL in Azure Blob Storage
            applicant.AudioRetryCount = retryCount + 1;
            applicant.AudioSampleScore = similarityScore;  // Save the similarity score

            db.SaveChanges();

            // Pass the results to the result view
            TempData["RecognizedText"] = recognizedText;
            TempData["OriginalText"] = StaticText;
            TempData["LevenshteinDistance"] = ComputeLevenshteinDistance(StaticText, recognizedText);
            TempData["SimilarityScore"] = similarityScore;

            return RedirectToAction("AudioSampleResult", new { applicantId });
        }

        // Save audio file to Azure Blob Storage
        private async Task<string> SaveAudioToBlobStorage(HttpPostedFileBase audioFile)
        {
            string connectionString = "";
            string containerName = "audiofiles";
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Generate a unique blob name
            string blobName = Guid.NewGuid().ToString() + ".wav";
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            // Upload the audio file to Blob Storage
            using (var stream = audioFile.InputStream)  // Stream the file directly
            {
                await blobClient.UploadAsync(stream);
            }

            return blobClient.Uri.ToString();  // Return the Blob URL
        }


        // Use Azure Speech-to-Text to transcribe the audio file
        public ActionResult DownloadAudioFile(string filePath)
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            string fileName = "recognized-audio.wav";

            // Return the file to the browser
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        private async Task<string> RecognizeSpeechAsync(string audioFilePath)
        {
            var config = SpeechConfig.FromSubscription("", "southafricanorth"); //speech rec keys

            // Temporary path where the audio will be saved locally before processing
            string tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetFileName(audioFilePath));

            // Download the blob file if it's in Azure Blob Storage
            BlobClient blobClient = new BlobClient(new Uri(audioFilePath));
            BlobDownloadInfo download = await blobClient.DownloadAsync();

            // Save the blob content to a temporary local file
            using (var fileStream = System.IO.File.OpenWrite(tempFilePath))
            {
                await download.Content.CopyToAsync(fileStream);
            }

            // Check if the file is already in PCM WAV format
            if (!IsWavFile(tempFilePath))
            {
                // Convert the audio to the correct WAV format if necessary
                tempFilePath = ConvertToPcmWav(tempFilePath);
            }

            // Ensure the file is now at convertedFilePath
            using (var audioInput = AudioConfig.FromWavFileInput(tempFilePath))
            {
                using (var recognizer = new SpeechRecognizer(config, audioInput))
                {
                    var result = await recognizer.RecognizeOnceAsync();
                    if (result.Reason == ResultReason.RecognizedSpeech)
                    {
                        return result.Text;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }
        }

        // Method to check if the audio file is already a PCM WAV file
        private bool IsWavFile(string filePath)
        {
            byte[] buffer = new byte[4];
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                // Read the first 4 bytes of the file to check for "RIFF" header
                fileStream.Read(buffer, 0, 4);
            }

            // Convert byte array to string and check if it's "RIFF" (standard header for WAV files)
            string header = System.Text.Encoding.ASCII.GetString(buffer);
            return header == "RIFF";
        }

        // Method to convert audio to PCM WAV format using NAudio
        private string ConvertToPcmWav(string inputFilePath)
        {
            string outputFilePath = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(inputFilePath) + "_converted.wav");

            try
            {
                using (var reader = new AudioFileReader(inputFilePath))
                {
                    var newFormat = new WaveFormat(16000, 16, 1);  // Convert to 16kHz, 16-bit PCM mono
                    using (var conversionStream = new WaveFormatConversionStream(newFormat, reader))
                    {
                        WaveFileWriter.CreateWaveFile(outputFilePath, conversionStream);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log or handle the error appropriately
                throw new ApplicationException("Failed to convert audio to PCM WAV format.", ex);
            }

            return outputFilePath;
        }





        // Levenshtein distance computation
        private int ComputeLevenshteinDistance(string source, string target)
        {
            int sourceLength = source.Length;
            int targetLength = target.Length;
            var distance = new int[sourceLength + 1, targetLength + 1];

            for (int i = 0; i <= sourceLength; distance[i, 0] = i++) { }
            for (int j = 0; j <= targetLength; distance[0, j] = j++) { }

            for (int i = 1; i <= sourceLength; i++)
            {
                for (int j = 1; j <= targetLength; j++)
                {
                    int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;
                    distance[i, j] = Math.Min(Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1), distance[i - 1, j - 1] + cost);
                }
            }

            return distance[sourceLength, targetLength];
        }

        // Calculate similarity using Levenshtein distance
        //private double CompareTextsUsingLevenshtein(string originalText, string recognizedText)
        //{
        //    int levenshteinDistance = ComputeLevenshteinDistance(originalText, recognizedText);
        //    int maxLength = Math.Max(originalText.Length, recognizedText.Length);

        //    if (maxLength == 0) return 100.0;

        //    double similarityScore = (1.0 - (double)levenshteinDistance / maxLength) * 100;
        //    return similarityScore;
        //}
        private double CompareTextsUsingLevenshtein(string originalText, string recognizedText)
        {
            // Convert to lowercase and remove punctuation for a more lenient comparison
            var cleanedOriginalText = new string(originalText.ToLower().Where(c => !char.IsPunctuation(c)).ToArray());
            var cleanedRecognizedText = new string(recognizedText.ToLower().Where(c => !char.IsPunctuation(c)).ToArray());

            int levenshteinDistance = ComputeLevenshteinDistance(cleanedOriginalText, cleanedRecognizedText);
            int maxLength = Math.Max(cleanedOriginalText.Length, cleanedRecognizedText.Length);

            if (maxLength == 0) return 100.0;

            // Calculate similarity score as a percentage
            double similarityScore = (1.0 - (double)levenshteinDistance / maxLength) * 100;

            return similarityScore;
        }





        //[HttpGet]
        //[Authorize]
        //public ActionResult AudioSampleResult(int applicantId)
        //{
        //    var applicant = db.Applicants.Find(applicantId);
        //    if (applicant == null)
        //    {
        //        return HttpNotFound("Applicant not found.");
        //    }

        //    // Pass the TempData values to ViewBag for rendering in the view
        //    ViewBag.Message = TempData["Message"] != null ? TempData["Message"].ToString() : "Your audio sample has been submitted.";
        //    ViewBag.OriginalText = TempData["OriginalText"] != null ? TempData["OriginalText"].ToString() : string.Empty;
        //    ViewBag.RecognizedText = TempData["RecognizedText"] != null ? TempData["RecognizedText"].ToString() : string.Empty;
        //    ViewBag.LevenshteinDistance = TempData["LevenshteinDistance"] != null ? TempData["LevenshteinDistance"].ToString() : string.Empty;
        //    ViewBag.SimilarityScore = TempData["SimilarityScore"] != null ? TempData["SimilarityScore"].ToString() : string.Empty;

        //    // Pass applicant info to the view
        //    ViewBag.ApplicantName = applicant.FullName;

        //    return View(applicant);
        //}

        [HttpGet]
        [Authorize]
        public ActionResult AudioSampleResult(int applicantId)
        {
            var applicant = db.Applicants.Find(applicantId);
            if (applicant == null)
            {
                return HttpNotFound("Applicant not found.");
            }

            // Pass the TempData values to ViewBag for rendering in the view
            ViewBag.Message = TempData["Message"] != null ? TempData["Message"].ToString() : "Your audio sample has been submitted.";
            ViewBag.OriginalText = TempData["OriginalText"] != null ? TempData["OriginalText"].ToString() : string.Empty;
            ViewBag.RecognizedText = TempData["RecognizedText"] != null ? TempData["RecognizedText"].ToString() : string.Empty;
            ViewBag.LevenshteinDistance = TempData["LevenshteinDistance"] != null ? TempData["LevenshteinDistance"].ToString() : string.Empty;

            // Round the SimilarityScore to 2 decimal places in the controller
            if (TempData["SimilarityScore"] != null)
            {
                double similarityScore = Convert.ToDouble(TempData["SimilarityScore"]);
                ViewBag.SimilarityScore = Math.Round(similarityScore, 2);  // Round it off here
            }
            else
            {
                ViewBag.SimilarityScore = 0.0;
            }

            // Pass applicant info to the view
            ViewBag.ApplicantName = applicant.FullName;

            return View(applicant);
        }




        //save audioScore
        [HttpPost]
        [Authorize]
        public ActionResult UpdateAudioScore(int applicantId, double newScore)
        {
            var applicant = db.Applicants.Find(applicantId);
            if (applicant == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Applicant not found");
            }

            // Update the applicant's audio sample score
            applicant.AudioSampleScore = newScore;

            // Notify the applicant of the change via email
            SendAudioScoreEmail(applicant, newScore);

            // Save the changes to the database
            db.SaveChanges();

            // Return an explicit success status code (200 OK)
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        private void SendAudioScoreEmail(Applicant applicant, double newScore)
        {
            if (applicant == null || string.IsNullOrEmpty(applicant.Email))
            {
                throw new ArgumentNullException(nameof(applicant), "Applicant object or email cannot be null.");
            }

            // Prepare the email
            var mailMessage = new MailMessage
            {
                From = new MailAddress("senderemail.com"),
                Subject = "Your Audio Sample Score Update",
                Body = $"Dear {applicant.FullName},<br/><br/>" +
                       $"Your audio sample score has been updated to {newScore:F2}%. " +
                       "And you have been move to the next screening stage. You should receive word about an offer based on your overall score" +
                       "Thank you,<br/>The Book Buffet Team",
                IsBodyHtml = true
            };

            // Add recipient email
            mailMessage.To.Add(new MailAddress(applicant.Email));

            // Send email using the SMTP settings from web.config
            using (var smtpClient = new SmtpClient())
            {
                smtpClient.Send(mailMessage);
            }
        }



        //contract

        [HttpGet]
        [Authorize(Roles = "Recruiter")]
        public ActionResult ContractDraft(int jobId)
        {
            // Fetch all applicants for the given job from the database
            var applicants = db.Applicants
                .Where(a => a.JobId == jobId && a.JobStatus == null)
                .ToList();

            // Filter recommended applicants who have an OverallScore >= 70, a valid AudioSampleScore, and a non-empty AudioBlobUrl
            var recommendedApplicants = applicants
                .Where(a => a.OverallScore.HasValue && a.OverallScore.Value >= 70
                            && a.AudioSampleScore.HasValue    // AudioSampleScore must have a value
                            && !string.IsNullOrEmpty(a.AudioBlobUrl))  // AudioBlobUrl must not be empty or null
                .ToList();

            if (!recommendedApplicants.Any())
            {
                TempData["ErrorMessage"] = "No applicants with a recommended score and valid audio sample found.";
                return RedirectToAction("Track", new { id = jobId });
            }

            // Pass recommended applicants to the view
            ViewBag.RecommendedApplicants = recommendedApplicants;
            ViewBag.JobId = jobId;

            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Recruiter")]
        public async Task<ActionResult> SendContracts(int jobId, string contractText)
        {
            // Fetch all applicants for the given job from the database
            var applicants = db.Applicants
                .Where(a => a.JobId == jobId && a.JobStatus == null)
                .ToList();

            // Filter recommended applicants who have an OverallScore >= 70, a valid AudioSampleScore, and a non-empty AudioBlobUrl
            var recommendedApplicants = applicants
                .Where(a => a.OverallScore.HasValue && a.OverallScore.Value >= 70
                            && a.AudioSampleScore.HasValue    // AudioSampleScore must have a value
                            && !string.IsNullOrEmpty(a.AudioBlobUrl))  // AudioBlobUrl must not be empty or null
                .ToList();

            foreach (var applicant in recommendedApplicants)
            {
                // Update job status to "Offered"
                applicant.JobStatus = "Offered";

                // Replace placeholders with applicant-specific information
                var personalizedContract = contractText
                    .Replace("[Applicant Name]", applicant.FullName)
                    .Replace("[Job Title]", applicant.Job.Title);

                // Store the personalized contract in the applicant's record
                applicant.ContractText = personalizedContract;

                // Update applicant in database
                db.Entry(applicant).State = EntityState.Modified;

                // Send contract email
                await SendContractEmail(applicant, personalizedContract);
            }

            await db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Contracts have been sent to recommended applicants.";
            return RedirectToAction("Track", new { id = jobId });
        }


        private async Task SendContractEmail(Applicant applicant, string contractText)
        {
            // Prepare the email content
            var mailMessage = new MailMessage
            {
                From = new MailAddress("senderemail.com"),
                Subject = "Job Offer for " + applicant.Job.Title,
                Body = $"Dear {applicant.FullName},<br/><br/>" +
                       $"We are pleased to offer you a position at The Book Buffet for" + applicant.Job.Title +". " +
                       $"Please access your application tracking view to review and sign the contract.<br/><br/>" +
                       $"Best regards,<br/>The Book Buffet",
                IsBodyHtml = true
            };

            // Add recipient email
            mailMessage.To.Add(new MailAddress(applicant.Email));

            // Send the email
            using (var smtpClient = new SmtpClient())
            {
                await smtpClient.SendMailAsync(mailMessage);
            }
        }


        [HttpGet]
        [Authorize]
        public ActionResult ReviewContract(int applicantId)
        {
            var applicant = db.Applicants.Find(applicantId);
            if (applicant == null || applicant.JobStatus != "Offered")
            {
                return HttpNotFound("No active job offer found.");
            }

            ViewBag.ContractText = applicant.ContractText; // Pass the contract to the view
            return View(applicant);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult SignContract(int applicantId, bool accept)
        {
            var applicant = db.Applicants.Find(applicantId);
            if (applicant == null || applicant.JobStatus != "Offered")
            {
                return HttpNotFound("No active job offer found.");
            }

            // Update JobStatus based on acceptance or rejection
            applicant.JobStatus = accept ? "Accepted" : "Rejected";
            db.Entry(applicant).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("TrackApplications");
        }










    }
}
