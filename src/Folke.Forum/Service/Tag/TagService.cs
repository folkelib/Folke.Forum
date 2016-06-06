using System.Collections.Generic;
using Folke.Elm;
using Folke.Elm.Fluent;
using Folke.Forum.Data.Forums;

namespace Folke.Forum.Service.Tag
{
    public class TagService<TUser> : ITagService<TUser>
    {
        private readonly IFolkeConnection session;

        public TagService(IFolkeConnection session)
        {
            this.session = session;
        }

        private class ManyToManyHelperConfig : IManyToManyHelperConfiguration<Data.Tags.Tag, string>
        {
            public bool AreEqual(Data.Tags.Tag child, string dto)
            {
                return child.Text == dto;
            }

            public Data.Tags.Tag Map(string dto)
            {
                return new Data.Tags.Tag { Text = dto };
            }

            public IList<Data.Tags.Tag> QueryExisting(IFolkeConnection connection, IList<string> dto)
            {
                return connection.SelectAllFrom<Data.Tags.Tag>().Where(t => t.Text.In(dto)).ToList();
            }

            public void UpdateDto(string dto, Data.Tags.Tag child)
            {
            }
        }

        public void BindTagListToThread(Thread<TUser> thread, IList<string> newTags)
        {
            session.UpdateManyToMany(thread, thread.Tags, newTags, new ManyToManyHelperConfig());
        }
    }
}
