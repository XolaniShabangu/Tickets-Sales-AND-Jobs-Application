using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TheBookBuffetSem2.Models
{
    public class SurveyViewModel
    {
        public int EventId { get; set; }
        public string EventTitle { get; set; }
        public int? InvitationId { get; set; }
        public List<SurveyQuestionViewModel> Questions { get; set; }
    }

    public class SurveyQuestionViewModel
    {
        public string QuestionText { get; set; }
        public int SelectedRating { get; set; } // The selected rating from 1 to 5
    }

}