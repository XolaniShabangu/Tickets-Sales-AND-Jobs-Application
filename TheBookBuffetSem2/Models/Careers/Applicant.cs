using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TheBookBuffetSem2.Models.Careers
{
    public class Applicant
    {
        [Key]
        public int ApplicantId { get; set; } // Primary key

        // Personal Information
        [Required]
        public string FullName { get; set; }

        [Required(ErrorMessage = "ID Number is required.")]
        [StringLength(13, MinimumLength = 13, ErrorMessage = "ID Number must be 13 digits long.")]
        public string IDNumber { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string MobileNumber { get; set; }

        [Required]
        public string Gender { get; set; }

        

        // File Uploads (to be stored in blob storage)
        
        public string CVBlobUrl { get; set; }  // URL to the CV in blob storage

        
        public string IDBlobUrl { get; set; }  // URL to the ID document in blob storage

        // Optional Extra Document
        public string ExtraDocumentBlobUrl { get; set; }  // URL to the Extra Document in blob storage
        public string ExtraDocumentDescription { get; set; }  // Description of what the extra document is

        // Foreign Key to Job
        public int JobId { get; set; }  // Foreign key
        public virtual Job Job { get; set; }  // Navigation property

        // Foreign Key to User
        public string UserId { get; set; }  // User ID from Identity
        public virtual ApplicationUser User { get; set; }  // Navigation property for User

        // Optional: Track Application Date
        public DateTime ApplicationDate { get; set; } = DateTime.Now;

        // New OCR-related properties
        public string OCRStatus { get; set; } // Passed/Failed based on OCR
        public DateTime? OCRPassedDate { get; set; } // Nullable in case OCR is not yet passed

        public double? OCRAccuracy { get; set; } // CV matching accuracy percentage



        // New properties for Interview and Audio Sample scores
        public double? AudioSampleScore { get; set; } // Audio Sample Score
        public int? AudioRetryCount { get; set; } // Audio Sample Score
        public double? AudioPenalty { get; set; }  // Penalty applied due to retries
        public string AudioBlobUrl { get; set; }  // URL to saved audio in Azure Blob

        public double? InterviewScore { get; set; } // Interview Score

        // Calculated Overall Score
        public double? OverallScore
        {
            get
            {
                // We assume each score is weighted equally for simplicity
                // If some scores are missing, adjust accordingly
                int count = 0;
                double total = 0;

                if (OCRAccuracy.HasValue)
                {
                    total += OCRAccuracy.Value;
                    count++;
                }
                if (AudioSampleScore.HasValue)
                {
                    total += AudioSampleScore.Value;
                    count++;
                }
                

                return count > 0 ? (double?)(total / count) : null;
            }
        }

        public bool IsCanceled { get; set; } = false;  // Default is false for active applications
        // New Property for Job Offer Status
        public string JobStatus { get; set; }  // "Offered", "Accepted", "Rejected", etc.
        public string ContractText { get; set; }
    }


}