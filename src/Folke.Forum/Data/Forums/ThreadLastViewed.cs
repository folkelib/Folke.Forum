using Folke.Elm;

namespace Folke.Forum.Data.Forums
{
    public class ThreadLastViewed<TUser> : IFolkeTable
    {
        public int Id { get; set; }
        public Thread<TUser> Thread { get; set; }
        public TUser Account { get; set; }
        public int NumberOfComments { get; set; }
    }
}
