using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TheBookBuffetSem2.Models
{
    public class RSVPViewModel
    {
        public string Token { get; set; }
        public string EventTitle { get; set; }
        public bool? RSVPStatus { get; set; }
        public bool? WillingToSpeak { get; set; }
        public string InviteeName { get; set; }
        public string InviteeType { get; set; }
        public int EventID { get; set; }
    }

}