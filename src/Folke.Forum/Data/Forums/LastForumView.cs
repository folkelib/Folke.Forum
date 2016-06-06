using System;
using Folke.Elm;

namespace Folke.Forum.Data.Forums
{
    public class LastForumView<TUser> : IFolkeTable
    {
        public int Id { get; set; }
        public TUser Account { get; set; }
        public Forum Forum { get; set; }
        public DateTime LastView { get; set; }
    }
}
