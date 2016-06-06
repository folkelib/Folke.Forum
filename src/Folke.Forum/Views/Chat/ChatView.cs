using System;
using System.ComponentModel.DataAnnotations;

namespace Folke.Forum.Views.Chat
{
    public class ChatView<TUserView>
    {
        public int Id { get; set; }
        public TUserView Author { get; set; }
        [Required]
        public string Text { get; set; }
        public DateTime? CreationDate { get; set; }

        public ChatView()
        {
            CreationDate = DateTime.UtcNow;
        }
    }
}