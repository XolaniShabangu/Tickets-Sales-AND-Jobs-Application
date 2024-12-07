using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TheBookBuffetSem2.Models.Careers
{
    public class Job
    {
        [Key]
        
        public int JobId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        [StringLength(500)]
        public string Qualifications { get; set; }

        // Comma-separated OCR Keywords for searching languages or skills
        [Required]
        [StringLength(500)]
        
        public string OcrKeywords { get; set; }  // Example: "English,Zulu,JavaScript"

        [Required]
        public bool Published { get; set; } = false;

        [Required]
        public DateTime ClosingDate { get; set; }  // Closing date for job applications

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        public int NumberOfPositions { get; set; }  // Number of positions for the job
    }
}