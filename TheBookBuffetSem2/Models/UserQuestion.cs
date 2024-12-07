using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TheBookBuffetSem2.Models
{
    public class UserQuestion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int QuestionId { get; set; }

        [Required]
        [StringLength(200)]
        public string Question { get; set; }

        public string Answer { get; set; } // Admin's answer to the question

        public DateTime SubmittedOn { get; set; } = DateTime.Now;

        public bool IsAnswered { get; set; } = false; // Indicates if the question has been answered by the admin
        public bool IsAddedToFAQ { get; set; } = false;
    }
}