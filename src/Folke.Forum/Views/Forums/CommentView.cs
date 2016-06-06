using System;

namespace Folke.Forum.Views.Forums
{
    public class CommentView<TUserView>
    {
        public int Id { get; set; }
        public TUserView Author { get; set; }
        public string Text { get; set; }
        public DateTime? CreationDate { get; set; }
    }
}