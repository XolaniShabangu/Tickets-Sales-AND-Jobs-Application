using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TheBookBuffetSem2.Models
{
    public class InviteGuestsViewModel
    {
        public int EventId { get; set; }
        public string EventTitle { get; set; }
        public List<AuthorSelectionViewModel> Authors { get; set; }
        public string InvitationMessage { get; set; }

        public Author NewAuthor { get; set; }
    }

    public class AuthorSelectionViewModel
    {
        public int AuthorID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool IsSelected { get; set; }
    }

}