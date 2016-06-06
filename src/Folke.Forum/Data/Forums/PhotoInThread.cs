using Folke.Elm;

namespace Folke.Forum.Data.Forums
{
    public class PhotoInThread<TUser> : IFolkeTable
    {
        public int Id { get; set; }
        public Thread<TUser> Thread { get; set; }
        public Photo<TUser> Photo { get; set; }
    }
}
