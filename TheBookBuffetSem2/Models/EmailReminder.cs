using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TheBookBuffetSem2.Models
{
    public class EmailReminder
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReminderID { get; set; } // Primary Key

        public int EventID { get; set; } // Foreign Key referencing Event model
        [ForeignKey("EventID")]
        public Event Event { get; set; }

        public DateTime ScheduledTime { get; set; } // The time to send the reminder email
        public bool IsSent { get; set; } // Status to check if the reminder has been sent
    }

}