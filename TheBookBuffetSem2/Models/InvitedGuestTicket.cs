using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TheBookBuffetSem2.Models
{
    public class InvitedGuestTicket
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InvitedGuestTicketID { get; set; } // Primary Key

        [Required]
        public int InvitationID { get; set; } // Foreign Key referencing Invitation model
        [ForeignKey("InvitationID")]
        public Invitation Invitation { get; set; }

        [Required]
        public string InviteeName { get; set; } // Name of the invited guest

        [Required]
        public string InviteeType { get; set; } // Type of invitee ("Author" or "Media")

        [Required]
        public DateTime IssuedDate { get; set; } // Date the ticket was issued

        public string QRCode { get; set; } // Unique QR code for the guest's ticket

        public string TicketImagePath { get; set; } // Path to the ticket image

        public bool CheckedIn { get; set; } = false; // Track whether the ticket has been checked in

        public DateTime? CheckedInTime { get; set; } // Track when the ticket was checked in
    }
}