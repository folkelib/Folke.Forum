using Folke.Forum.Data.Tags;
using Folke.Forum.Views.Tags;

namespace Folke.Forum.DataMapping
{
    public class TagDataMapping<TUser, TUserView>
    {
        private readonly ForumsDataMapping<TUser, TUserView> forumsDataMapping;

        public TagDataMapping(ForumsDataMapping<TUser, TUserView> forumsDataMapping)
        {
            this.forumsDataMapping = forumsDataMapping;
        }

        public static TagView ToTagDto(Tag tag)
        {
            return new TagView
            {
                Id = tag.Id,
                Text = tag.Text
            };
        }

        public TagThreadView<TUserView> ToTagThreadDto(TagThread<TUser> tagThread)
        {
            return new TagThreadView<TUserView>
            {
                Id = tagThread.Id,
                Tag = ToTagDto(tagThread.Tag),
                Thread = forumsDataMapping.ToThreadView(tagThread.Thread, null)
            };
        }
    }
}
