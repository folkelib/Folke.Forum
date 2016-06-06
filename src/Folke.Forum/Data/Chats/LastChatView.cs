using Folke.Elm;
using Folke.Elm.Mapping;

namespace Folke.Forum.Data.Chats
{
    public class LastChatView<TUser> : IFolkeTable
    {
        public int Id { get; set; }
        [Index(Name= "LastChatViewAccount")]
        public TUser Account { get; set; }
        public Chat<TUser> Chat { get; set; }
    }
}
