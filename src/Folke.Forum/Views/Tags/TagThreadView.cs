using Folke.Forum.Views.Forums;

namespace Folke.Forum.Views.Tags
{
    public class TagThreadView<TUserView>
    {
        public int Id { get; set; }
        public TagView Tag { get; set; }
        public ThreadView<TUserView> Thread { get; set; }
    }
}
