using System;
using System.Collections.Generic;

namespace Folke.Forum.Views.Forums
{
    public class PrivateMessageView<TUserView>
    {
        public int Id { get; set; }
        public DateTime? CreationDate { get; set; }
        public TUserView Author { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public IList<TUserView> AccountRecipients { get; set; }
    }
}
