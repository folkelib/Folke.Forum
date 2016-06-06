using Folke.Elm;

namespace Folke.Forum.Data.Misc
{
    public class ExternalLink : IFolkeTable
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
    }
}
