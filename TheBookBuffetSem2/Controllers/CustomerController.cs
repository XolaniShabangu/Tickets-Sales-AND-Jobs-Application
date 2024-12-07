using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TheBookBuffetSem2.Models;
using System.Data.Entity;
using System.Web.Security;
using Microsoft.AspNet.Identity;

namespace TheBookBuffetSem2.Controllers
{
    public class CustomerController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [Authorize]
        
        public ActionResult Dashboard()
        {
            var upcomingEvents = db.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .Where(b => b.Event.LaunchDate >= DateTime.Now && b.Event.LaunchDate <= DbFunctions.AddHours(DateTime.Now, 24))
                .Select(b => new EventVenueViewModel
                {
                    Event = b.Event,
                    Venue = b.Venue,
                    Tickets = b.Event.Tickets.ToList()
                })
                .ToList();

            ViewBag.UpcomingEvents = upcomingEvents;

            return View();
        }


        public ActionResult FAQ()
        {
            var faqs = db.FAQs.Where(f => f.IsVisible).ToList();
            return View(faqs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitQuestion(string question)
        {
            if (!string.IsNullOrEmpty(question))
            {
                var userQuestion = new UserQuestion
                {
                    Question = question,
                    SubmittedOn = DateTime.Now
                };

                db.UserQuestions.Add(userQuestion);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Your question has been submitted. We will get back to you soon!";
            }
            else
            {
                TempData["ErrorMessage"] = "Please enter a question.";
            }

            return RedirectToAction("FAQ");
        }

        [Authorize]
        public ActionResult ManageQuestions()
        {
            var questions = db.UserQuestions.ToList();
            return View(questions);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AnswerQuestion(int id, string answer)
        {
            var question = db.UserQuestions.Find(id);

            if (question != null)
            {
                question.Answer = answer;
                question.IsAnswered = true;

                db.SaveChanges();

                TempData["SuccessMessage"] = "The question has been answered successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Question not found.";
            }

            return RedirectToAction("ManageQuestions");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddToFAQ(int id)
        {
            var question = db.UserQuestions.Find(id);

            if (question != null && question.IsAnswered)
            {
                var faq = new FAQ
                {
                    Question = question.Question,
                    Answer = question.Answer,
                    IsVisible = true
                };

                db.FAQs.Add(faq);
                db.SaveChanges();

                TempData["SuccessMessage"] = "The question has been added to the FAQs.";
            }
            else
            {
                TempData["ErrorMessage"] = "Question must be answered before it can be added to the FAQs.";
            }

            return RedirectToAction("ManageQuestions");
        }


        [HttpPost]
        public ActionResult BulkAddToFAQ(int[] selectedQuestions)
        {
            if (selectedQuestions != null && selectedQuestions.Any())
            {
                foreach (var id in selectedQuestions)
                {
                    var question = db.UserQuestions.Find(id);
                    if (question != null && question.IsAnswered && !question.IsAddedToFAQ)
                    {
                        // Add the question to the FAQ
                        var faq = new FAQ
                        {
                            Question = question.Question,
                            Answer = question.Answer
                        };
                        db.FAQs.Add(faq);

                        // Mark the question as added to the FAQ
                        question.IsAddedToFAQ = true;
                    }
                }
                db.SaveChanges();
                TempData["SuccessMessage"] = "Selected questions have been added to the FAQ.";
            }
            else
            {
                TempData["ErrorMessage"] = "No questions were selected.";
            }

            return RedirectToAction("ManageQuestions");
        }



        //surveys

        public ActionResult SurveyRequest()
        {
            var userId = User.Identity.GetUserId();
            var recentEvents = db.TicketSales
                                 .Where(ts => ts.UserID == userId && ts.CheckedIn)
                                 .OrderByDescending(ts => ts.CheckedInTime)
                                 .Select(ts => ts.Ticket.Event)
                                 .Take(2) // Take the two most recent events
                                 .ToList();

            var pendingSurveys = recentEvents.Where(e => !db.Surveys.Any(s => s.EventID == e.EventId && s.UserID == userId))
                                             .ToList();

            if (pendingSurveys.Any())
            {
                // Return the first pending survey request
                return PartialView("_SurveyRequest", pendingSurveys);
            }

            return new EmptyResult(); // No surveys to show
        }

        // In CustomerController

        public ActionResult TakeSurvey(int eventId, int? invitationId)
        {
            var userId = User.Identity.GetUserId();

            // Check if the survey for this event has already been answered by the user or by the invitation ID
            var existingSurvey = db.Surveys.FirstOrDefault(s =>
                s.EventID == eventId &&
                (s.UserID == userId || s.InvitationID == invitationId));

            if (existingSurvey != null)
            {
                // If survey already exists, redirect to a "Survey Already Taken" page or handle accordingly
                return RedirectToAction("SurveyAlreadyTaken");
            }

            // Create a view model for the survey
            var surveyViewModel = new SurveyViewModel
            {
                EventId = eventId,
                EventTitle = db.Events.Find(eventId).BookTitle,
                Questions = new List<SurveyQuestionViewModel>
        {
            new SurveyQuestionViewModel { QuestionText = "How would you rate the event organization?" },
            new SurveyQuestionViewModel { QuestionText = "How satisfied were you with the Book launched?" },
            new SurveyQuestionViewModel { QuestionText = "How would you rate the speaker's performance?" },
            new SurveyQuestionViewModel { QuestionText = "How would you rate the overall experience?" },
            new SurveyQuestionViewModel { QuestionText = "How likely are you to recommend our events to others?" }
        }
            };

            return View(surveyViewModel);
        }

        public ActionResult SurveyAlreadyTaken() { return View(); }

       

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitSurvey(SurveyViewModel model)
        {
            // Check if it's a registered user or an invited guest
            string userId = User.Identity.GetUserId();
            int? invitationId = model.InvitationId;

            // Calculate the sum of the selected ratings
            int sumOfRatings = model.Questions.Sum(q => q.SelectedRating);

            // Calculate the average rating
            int numberOfQuestions = model.Questions.Count;
            double averageRating = (double)sumOfRatings / numberOfQuestions;

            // Create a new Survey object and map the question ratings from the model
            var survey = new Survey
            {
                EventID = model.EventId,
                UserID = !string.IsNullOrEmpty(userId) ? userId : null, // Use UserID if available
                InvitationID = invitationId, // Use InvitationID if available
                Question1Rating = model.Questions[0].SelectedRating, // First question's rating
                Question2Rating = model.Questions[1].SelectedRating, // Second question's rating
                Question3Rating = model.Questions[2].SelectedRating, // Third question's rating
                Question4Rating = model.Questions[3].SelectedRating, // Fourth question's rating
                Question5Rating = model.Questions[4].SelectedRating, // Fifth question's rating
                SubmissionDate = DateTime.Now,
                TotalRating = (int)Math.Round(averageRating) // Rounded average rating out of 5
            };

            // Save the survey to the database
            db.Surveys.Add(survey);
            db.SaveChanges();

            // Set a success message using TempData
            TempData["SuccessMessage"] = "Thank you! Your survey has been submitted successfully.";

            // Redirect back to the customer dashboard
            return RedirectToAction("Dashboard", "Customer");
        }


    }
}