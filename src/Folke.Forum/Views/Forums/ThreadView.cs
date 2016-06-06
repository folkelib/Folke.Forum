using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Folke.Forum.Views.Poll;

namespace Folke.Forum.Views.Forums
{
    public class ThreadView<TUserView>
    {
        public int Id { get; set; }
        public DateTime? CreationDate { get; set; }
        public TUserView Author { get; set; }
        [Required]
        [MinLength(3)]
        public string Title { get; set; }
        [Required]
        [MinLength(3)]
        public string Text { get; set; }
        public bool Sticky { get; set; }
        public int NumberOfComments { get; set; }
        public int NumberOfViewedComments { get; set;}
        public int LastViewedId { get; set; }
    }
}