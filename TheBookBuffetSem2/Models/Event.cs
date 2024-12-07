using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TheBookBuffetSem2.Models
{
    public class Event
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EventId { get; set; }

        [Required]
        [StringLength(100)]
        public string Objectives { get; set; }

        [Required]
        [StringLength(100)]
        public string Author { get; set; }

        [Required]
        [StringLength(100)]
        public string BookTitle { get; set; }

        [Required]
        public DateTime LaunchDate { get; set; }

        [Required]
        public decimal Budget { get; set; }

        // Navigation property for Venue
        // Foreign Key for Venue
        public int? VenueID { get; set; }
        public virtual Venue Venue { get; set; }

        public virtual ICollection<Ticket> Tickets { get; set; } // Relationship with Tickets

        public Event()
        {
            Tickets = new HashSet<Ticket>();
        }
    }
}