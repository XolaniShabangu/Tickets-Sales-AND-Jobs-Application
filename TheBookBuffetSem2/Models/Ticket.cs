using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TheBookBuffetSem2.Models
{
    public class Ticket
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TicketID { get; set; }

        public int EventID { get; set; }
        [ForeignKey("EventID")]
        public Event Event { get; set; }

        [Required]
        public string Category { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        public int TotalTickets { get; set; }  // Total number of tickets available for this category

        public int RemainingTickets { get; set; }  // Tracks how many tickets are left
    }
}