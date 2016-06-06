using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Folke.Elm;
using Folke.Elm.Mapping;

namespace Folke.Forum.Data.Forums
{
    public class PrivateMessage<TUser> : IFolkeTable
    {
        public int Id { get; set; }
        public TUser Author { get; set; }
        public string Title { get; set; }
        [MaxLength(65536)]
        public string Text { get; set; }
        [Select(IncludeReference = nameof(PrivateMessageRecipient<TUser>.Recipient))]
        public IReadOnlyList<PrivateMessageRecipient<TUser>> Recipients { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
