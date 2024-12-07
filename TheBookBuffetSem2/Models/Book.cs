using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TheBookBuffetSem2.Models
{
    public class Book
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BookID { get; set; } // Primary Key
        public string Title { get; set; }

        public decimal Price { get; set; }

        // Foreign key to Author
        [Required(ErrorMessage = "Author is required")]
        public int AuthorID { get; set; }
        [ForeignKey("AuthorID")]
        public Author Author { get; set; }

        
    }
}