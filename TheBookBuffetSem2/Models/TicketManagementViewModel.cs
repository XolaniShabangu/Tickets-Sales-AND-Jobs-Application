using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TheBookBuffetSem2.Models
{
    public class TicketManagementViewModel
    {
        public int EventID { get; set; }

        public string EventName { get; set; }

        public int VenueCapacity { get; set; } // Total capacity of the venue

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Please enter a valid price.")]
        public decimal GeneralPrice { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Please enter a valid price.")]
        public decimal VIPPrice { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Please enter a valid price.")]
        public decimal VVIPPrice { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Please enter a valid number of tickets.")]
        public int GeneralTickets { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Please enter a valid number of tickets.")]
        public int VIPTickets { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Please enter a valid number of tickets.")]
        public int VVIPTickets { get; set; }

        public int TotalTickets => GeneralTickets + VIPTickets + VVIPTickets;

        public int RemainingCapacity => VenueCapacity - TotalTickets;
    }
}