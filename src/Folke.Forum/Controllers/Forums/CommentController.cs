using System.Threading.Tasks;
using Folke.Elm;
using Folke.Forum.Data.Comments;
using Folke.Forum.DataMapping;
using Folke.Forum.Service;
using Folke.Forum.Views.Forums;
using Folke.Mvc.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Folke.Forum.Controllers.Forums
{
    [Route("api/comment")]
    public class CommentController<TUser, TUserView> : TypedControllerBase
    {
        private readonly IFolkeConnection session;
        private readonly ForumsDataMapping<TUser, TUserView> forumsDataMapping;
        private readonly IForumUserService<TUser, TUserView> userService;

        public CommentController(IFolkeConnection session, ForumsDataMapping<TUser, TUserView> forumsDataMapping, IForumUserService<TUser, TUserView> userService)
        {
            this.session = session;
            this.forumsDataMapping = forumsDataMapping;
            this.userService = userService;
        }

        // GET api/commentapi/5
        [HttpGet(Name="GetComment")]
        public async Task<CommentView<TUserView>> Get(int id)
        {
            return forumsDataMapping.ToCommentView(await session.LoadAsync<Comment<TUser>>(id));
        }


        // PUT api/commentapi/5
        [HttpPut]
        [Route("{id:int}")]
        public async Task Put(int id, [FromBody]CommentView<TUserView> value)
        {
            var account = await userService.GetCurrentUserAsync();
            using (var transaction = session.BeginTransaction())
            {
                var comment = await session.LoadAsync<Comment<TUser>>(id);
                if (await userService.IsUser(account, comment.Author))
                {
                    comment.Text = value.Text;
                    await session.UpdateAsync(comment);
                    transaction.Commit();
                }
            }
        }
    }
}
