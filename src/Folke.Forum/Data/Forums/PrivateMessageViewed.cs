using System;
using Folke.Elm;

namespace Folke.Forum.Data.Forums
{
    public class PrivateMessageViewed<TUser> : IFolkeTable
    {
        public int Id { get; set; }
        public TUser Account { get; set; }
        public PrivateMessage<TUser> PrivateMessage { get; set; }
        public DateTime Date { get; set; }
    }
}
