using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TheBookBuffetSem2.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalVenuesCount { get; set; }
        public int UpcomingEventsCount { get; set; }
        public List<string> TicketSalesChartLabels { get; set; } // E.g., event names or dates
        public List<int> TicketSalesChartData { get; set; } // Corresponding ticket sales data
    }


}