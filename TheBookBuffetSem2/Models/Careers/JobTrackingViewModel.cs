using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TheBookBuffetSem2.Models.Careers
{
    public class JobTrackingViewModel
    {
        public Job Job { get; set; }
        public List<Applicant> Applicants { get; set; }

        // New property for recommended applicants
        public List<Applicant> RecommendedApplicants { get; set; }
    }

}