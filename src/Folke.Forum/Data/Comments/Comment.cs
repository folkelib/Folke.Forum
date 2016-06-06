using System;
using System.ComponentModel.DataAnnotations;
using Folke.Elm;

namespace Folke.Forum.Data.Comments
{
    public class Comment<TUser> : IFolkeTable
    {
        public int Id { get; set; }
        public TUser Author { get; set; }
        [MaxLength(65536)]
        public string Text { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
