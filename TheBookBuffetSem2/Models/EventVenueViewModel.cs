using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TheBookBuffetSem2.Models
{
    public class EventVenueViewModel
    {
        public Event Event { get; set; }
        public Venue Venue { get; set; }
        public List<Ticket> Tickets { get; internal set; }

        public bool GuestsInvited { get; set; } // Indicator for invited guests
        public bool TicketsCreated { get; set; } // Indicator for created tickets

        // When populating the model in your controller:

    }
}