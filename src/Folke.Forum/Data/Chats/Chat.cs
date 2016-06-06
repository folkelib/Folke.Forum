using System;
using Folke.Elm;
using Folke.Elm.Mapping;

namespace Folke.Forum.Data.Chats
{
    public class Chat<TUser> : IFolkeTable
    {
        public int Id { get; set; }
        public TUser Author { get; set; }
        [Index(Name = "ChatCreationDate")]
        public DateTime CreationDate { get; set; }
        public string Text { get; set; }
    }
}
