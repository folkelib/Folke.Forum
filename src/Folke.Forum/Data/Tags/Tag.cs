using Folke.Elm;

namespace Folke.Forum.Data.Tags
{
    public class Tag : IFolkeTable
    {
        public int Id { get; set; }
        public string Text { get; set; }
    }
}
