using System.Collections.Generic;
using Folke.Forum.Data.Forums;

namespace Folke.Forum.Service.Tag
{
    public interface ITagService<TUser>
    {
        void BindTagListToThread(Thread<TUser> thread, IList<string> newTags);
    }
}
