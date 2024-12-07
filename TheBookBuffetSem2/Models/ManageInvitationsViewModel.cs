using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TheBookBuffetSem2.Models
{
    public class ManageInvitationsViewModel
    {
        public string EventTitle { get; set; }
        public DateTime EventDate { get; set; }
        public List<InviteeDetailsViewModel> Invitations { get; set; }
    }

    public class InviteeDetailsViewModel
    {
        public int EventID { get; set; }
        public string InviteeName { get; set; }
        public string Email { get; set; }
        public string InviteeType { get; set; } // "Author" or "Media"
        public bool? RSVPStatus { get; set; }
        public bool? WillingToSpeak { get; set; }
    }




}