using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TheBookBuffetSem2.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        public int EventId { get; set; }
        public int VenueId { get; set; }

        public DateTime DateBooked { get; set; }

        public virtual Event Event { get; set; }
        public virtual Venue Venue { get; set; }
    }
}