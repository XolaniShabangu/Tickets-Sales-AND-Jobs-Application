using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TheBookBuffetSem2.Models
{
    public class EventReminderViewModel
    {
        public int EventId { get; set; }
        public string BookTitle { get; set; }
        public DateTime LaunchDate { get; set; }
        public bool RemindersSent { get; set; }
        public bool ReminderScheduled { get; set; } // Add this to track if a reminder has been scheduled.
    }

}