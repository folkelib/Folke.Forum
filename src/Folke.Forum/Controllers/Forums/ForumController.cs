using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Folke.Elm;
using Folke.Elm.Fluent;
using Folke.Forum.Data.Forums;
using Folke.Forum.DataMapping;
using Folke.Forum.Service;
using Folke.Forum.Views.Forums;
using Folke.Mvc.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Folke.Forum.Controllers.Forums
{
    [Route("api/forum")]
    public class ForumController<TUser, TUserView> : TypedControllerBase
    {
        private readonly IFolkeConnection session;
        private readonly IForumUserService<TUser, TUserView> userService;
        private readonly ForumsDataMapping<TUser, TUserView> forumDataMapping;

        public ForumController(IFolkeConnection session, IForumUserService<TUser, TUserView> userService, ForumsDataMapping<TUser,TUserView> forumDataMapping)
        {
            this.session = session;
            this.userService = userService;
            this.forumDataMapping = forumDataMapping;
        }

        [HttpGet]
        public async Task<IEnumerable<ForumView>> GetAll()
        {
            var account = await userService.GetCurrentUserAsync();
            var query = await session.SelectAllFrom<Data.Forums.Forum>().OrderBy(f => f.Name).Asc().ToListAsync();
            var forums = new List<Data.Forums.Forum>();
            foreach (var q in query)
            {
                if (q.ReadRole == null || await userService.HasRole(account, q.ReadRole))
                    forums.Add(q);
            }

            if (account != null && forums.Any())
            {
                var lastViewed = await session.SelectAllFrom<LastForumView<TUser>>().Where(l => l.Account.Equals(account)).Where(l => l.Forum.In(forums.ToArray())).ToListAsync();
                var dto = new List<ForumView>();
                foreach (var forum in forums)
                {
                    var numberOfNewMessages = await GetForumNewMessages(forum, lastViewed);
                    dto.Add(forumDataMapping.ToForumView(forum, numberOfNewMessages));
                }
                return dto;                
            }
            return forums.Select(f => forumDataMapping.ToForumView(f, 0));
        }

        [NonAction]
        private async Task<int> GetForumNewMessages(Data.Forums.Forum forum, IEnumerable<LastForumView<TUser>> lastViewed)
        {
            var last = lastViewed.Where(l => l.Forum == forum).Select(f => f.LastView).SingleOrDefault();
            var numberOfNewMessages = await session.Select<Thread<TUser>>().CountAll().From().Where(t => t.Forum == forum && t.CreationDate > last).ScalarAsync<int>();
            numberOfNewMessages += await session.Select<CommentInThread<TUser>>().CountAll().From().LeftJoinOnId(x => x.Commentable).LeftJoinOnId(x => x.Comment).Where(x => x.Comment.CreationDate > last && x.Commentable.Forum == forum).ScalarAsync<int>();
            return numberOfNewMessages;
        }

        [HttpGet("new")]
        public async Task<int> GetNewMessages()
        {
            var account = await userService.GetCurrentUserAsync();
            if (account == null)
                return 0;
            var query = await session.SelectAllFrom<Data.Forums.Forum>().ToListAsync();
            var forums = new List<Data.Forums.Forum>();
            foreach (var q in query)
            {
                if (q.ReadRole == null || await userService.HasRole(account, q.ReadRole))
                    forums.Add(q);
            }
            var numberOfNewMessages = 0;
            if (forums.Any())
            {
                var lastViewed = await session.SelectAllFrom<LastForumView<TUser>>().Where(l => l.Account.Equals(account)).Where(l => l.Forum.Id.In(forums.Select(x => x.Id))).ToListAsync();
                foreach (var forum in forums)
                {
                    numberOfNewMessages += await GetForumNewMessages(forum, lastViewed);
                }
            }
            return numberOfNewMessages;
        }

        [HttpGet("{id:int}", Name = "GetForum")]
        public async Task<ForumView> Get(int id)
        {
            return forumDataMapping.ToForumView(await session.LoadAsync<Data.Forums.Forum>(id), 0);
        }

        [HttpGet("{name}")]
        public async Task<ForumView> GetByName(string name)
        {
            return
                forumDataMapping.ToForumView(
                    await session.SelectAllFrom<Data.Forums.Forum>().FirstOrDefaultAsync(x => x.Name == name), 0);
        }

        [Authorize("ForumAdministrator")]
        [HttpPost]
        public async Task<IHttpActionResult<ForumView>> Post([FromBody]ForumView value)
        {
            using (var transaction = session.BeginTransaction())
            {
                var forum = new Data.Forums.Forum { Name = value.Name, ReadRole = value.ReadRole, WriteRole = value.WriteRole };
                await session.SaveAsync(forum);
                transaction.Commit();
                value = forumDataMapping.ToForumView(forum, 0);
            }
            return Created("GetForum", value.Id, value);
        }

        [Authorize("ForumAdministrator")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, [FromBody]ForumView value)
        {
            using (var transaction = session.BeginTransaction())
            {
                var forum = await session.LoadAsync<Data.Forums.Forum>(id);
                forum.ReadRole = value.ReadRole;
                forum.WriteRole = value.WriteRole;
                forum.Name = value.Name;
                await session.UpdateAsync(forum);
                transaction.Commit();
            }
            return Ok();
        }

        [Authorize("ForumAdministrator")]
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            using (var transaction = session.BeginTransaction())
            {
                var forum = await session.LoadAsync<Data.Forums.Forum>(id);
                await session.DeleteAsync(forum);
                transaction.Commit();
            }
            return Ok();
        }
    }
}
