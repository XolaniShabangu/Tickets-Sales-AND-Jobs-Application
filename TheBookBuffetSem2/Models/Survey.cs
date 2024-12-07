using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TheBookBuffetSem2.Models
{
    public class Survey
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SurveyID { get; set; } // Primary Key

        [Required]
        public int EventID { get; set; } // Foreign Key referencing Event model
        [ForeignKey("EventID")]
        public Event Event { get; set; }

        
        public string UserID { get; set; } // Foreign Key referencing User model
        [ForeignKey("UserID")]
        public ApplicationUser User { get; set; }

        public int? InvitationID { get; set; } // Foreign Key referencing Invitation model (optional for buyers)
        [ForeignKey("InvitationID")]
        public Invitation Invitation { get; set; }
        [Required]
        public int Question1Rating { get; set; } // Likert scale rating for Question 1

        [Required]
        public int Question2Rating { get; set; } // Likert scale rating for Question 2

        [Required]
        public int Question3Rating { get; set; } // Likert scale rating for Question 3

        [Required]
        public int Question4Rating { get; set; } // Likert scale rating for Question 3

        [Required]
        public int Question5Rating { get; set; } // Likert scale rating for Question 3

        [Required]
        public DateTime SubmissionDate  { get; set; }

        // Add more questions as needed

        [Required]
        public int TotalRating { get; set; } // Calculated total rating
    }
}