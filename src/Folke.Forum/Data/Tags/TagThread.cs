using Folke.Elm;
using Folke.Forum.Data.Forums;

namespace Folke.Forum.Data.Tags
{
    public class TagThread<TUser> : IManyToManyTable<Thread<TUser>, Tag>
    {
        public int Id { get; set; }
        public Tag Tag { get; set; }
        public Thread<TUser> Thread { get; set; }

        public Thread<TUser> Parent
        {
            get
            {
                return Thread;
            }
            set
            {
                Thread = value;
            }
        }

        public Tag Child
        {
            get
            {
                return Tag;
            }
            set
            {
                Tag = value;
            }
        }
    }
}
