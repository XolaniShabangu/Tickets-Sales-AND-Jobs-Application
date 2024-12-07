using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TheBookBuffetSem2.Models
{
    public class CreateTicketViewModel
    {
        public int EventId { get; set; }
        public string EventTitle { get; set; }

        [Required]
        public string Category { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        public int TotalTickets { get; set; }

        public int VenueId { get; set; }
    }
}