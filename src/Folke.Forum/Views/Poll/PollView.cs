using System;
using System.Collections.Generic;

namespace Folke.Forum.Views.Poll
{
    public class PollView<TUserView>
    {
        public int Id { get; set; }
        public TUserView Author { get; set; }
        public string Question {get;set;}
        public ICollection<PollPossibleAnswerView> PossibleAnswers { get; set;}
        public DateTime? OpenDate { get; set; }
        public DateTime? CloseDate { get; set; }
    }
}
