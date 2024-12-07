using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TheBookBuffetSem2.Models
{
    public class TicketSale
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TicketSaleID { get; set; } // Primary Key

        [Required]
        public int TicketID { get; set; } // Foreign Key referencing Ticket model
        [ForeignKey("TicketID")]
        public Ticket Ticket { get; set; }

        [Required]
        public string UserID { get; set; } // Foreign Key referencing User model
        [ForeignKey("UserID")]
        public ApplicationUser User { get; set; }

        [Required]
        public int Quantity { get; set; } // Number of tickets purchased

        [Required]
        public DateTime SaleDate { get; set; } // Date of the sale

        [Required]
        public decimal TotalPrice { get; set; } // Total price for the sale

        public string QRCode { get; set; } // Unique QR code
        public string TicketImagePath { get; set; } // Path to the ticket image

        public bool CheckedIn { get; set; } = false; // Track whether the ticket has been checked in

        public DateTime? CheckedInTime { get; set; } // Track when the ticket was checked in
    }

}