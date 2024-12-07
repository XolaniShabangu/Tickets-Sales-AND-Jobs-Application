using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TheBookBuffetSem2.Models
{
    public class TicketScanViewModel
    {
        public string QRCodeData { get; set; } // This will store the decoded QR code data
        public string ScanResultMessage { get; set; } // This will store any result messages like "Check-in successful"
    }
}