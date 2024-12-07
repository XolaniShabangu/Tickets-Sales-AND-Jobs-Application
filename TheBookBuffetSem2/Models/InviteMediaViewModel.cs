using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TheBookBuffetSem2.Models
{
    public class InviteMediaViewModel
    {
        public int EventId { get; set; }
        public string EventTitle { get; set; }
        public List<MediaSelectionViewModel> MediaContacts { get; set; }
        public string InvitationMessage { get; set; }
    }

    public class MediaSelectionViewModel
    {
        public int MediaID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool IsSelected { get; set; }
    }

}