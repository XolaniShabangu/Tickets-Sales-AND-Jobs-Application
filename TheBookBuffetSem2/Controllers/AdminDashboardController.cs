using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using TheBookBuffetSem2.Models;
using System.Data.Entity; // For Entity Framework 6


namespace TheBookBuffetSem2.Controllers
{
    public class AdminDashboardController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin
       
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            var model = new AdminDashboardViewModel
            {
                TotalVenuesCount = db.Venues.Count(),
                UpcomingEventsCount = db.Events.Count(e => e.LaunchDate >= DateTime.Now),
                TicketSalesChartLabels = db.Events.Select(e => e.BookTitle).ToList(),
                TicketSalesChartData = db.Events.Select(e => e.Tickets.Sum(t => (int?)t.TotalTickets - t.RemainingTickets) ?? 0).ToList()
            };


            return View(model);
        }

        [Authorize(Roles = "Staff")]
        public ActionResult StaffDashboard()
        {
            var model = new StaffDashboardViewModel
            {
                UpcomingEventsCount = db.Events.Count(e => e.LaunchDate >= DateTime.Now)
                // You can add more properties if needed
            };

            return View(model);
        }



    }
}