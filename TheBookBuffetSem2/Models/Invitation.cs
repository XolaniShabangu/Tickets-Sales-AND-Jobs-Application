using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TheBookBuffetSem2.Models
{
    public class Invitation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InvitationID { get; set; } // Primary Key

        public string Token { get; set; } // Unique token for tracking

        public string InviteeType { get; set; } // "Author" or "Media"

        public int InviteeID { get; set; } // AuthorID or MediaID

        public bool? RSVPStatus { get; set; } // E.g., "Accepted"

        public bool? WillingToSpeak { get; set; } // Nullable, true if willing to speak

        public int? EventID { get; set; } // Reference to the Event
        public Event Event { get; set; } // Navigation property
    }


}