using Folke.Elm;

namespace Folke.Forum.Data.Forums
{
    public class PrivateMessageRecipient<TUser> : IFolkeTable
    {
        public int Id { get; set; }
        public PrivateMessage<TUser> PrivateMessage { get; set; }
        public TUser Recipient { get; set; }
    }
}
