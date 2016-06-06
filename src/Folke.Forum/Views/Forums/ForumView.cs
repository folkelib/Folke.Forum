namespace Folke.Forum.Views.Forums
{
    public class ForumView
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string WriteRole { get; set; }
        public string ReadRole { get; set; }
        public int NumberOfNewMessages { get; set; }
    }
}