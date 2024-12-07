using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TheBookBuffetSem2.Models.Careers
{
    public class OCRResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public double? Accuracy { get; set; }
    }

}