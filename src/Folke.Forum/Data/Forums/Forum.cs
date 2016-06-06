using Folke.Elm;

namespace Folke.Forum.Data.Forums
{
    public class Forum : IFolkeTable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ReadRole { get; set; }
        public string WriteRole { get; set; }
    }
}
