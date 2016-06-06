using Folke.Elm;

namespace Folke.Forum.Views.Tags
{
    public class TagView : IFolkeTable
    {
        public int Id { get; set; }
        public string Text { get; set; }
    }
}
