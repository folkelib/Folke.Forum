using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Folke.Elm;
using Folke.Elm.Fluent;
using Folke.Forum.Data.Forums;
using Folke.Forum.DataMapping;
using Folke.Forum.Service;
using Folke.Forum.Service.Forums;
using Folke.Forum.Views.Forums;
using Folke.Mvc.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Folke.Forum.Controllers.Forums
{
    [Route("api/thread")]
    public class ThreadController<TUser, TUserView> : TypedControllerBase
    {
        private readonly IFolkeConnection session;
        private readonly IForumUserService<TUser, TUserView> accountService;
        private readonly ForumsDataMapping<TUser, TUserView> forumsDataMapping;
        private readonly HtmlSanitizerService<TUser> htmlSanitizerService;
        private readonly CommentService<Thread<TUser>, CommentInThread<TUser>, TUser, TUserView> commentService;

        public ThreadController(IFolkeConnection session, 
            IForumUserService<TUser, TUserView> accountService,
            ForumsDataMapping<TUser, TUserView> forumsDataMapping,
            HtmlSanitizerService<TUser> htmlSanitizerService,
            CommentService<Thread<TUser>, CommentInThread<TUser>, TUser, TUserView> commentService)
        {
            this.session = session;
            this.accountService = accountService;
            this.forumsDataMapping = forumsDataMapping;
            this.htmlSanitizerService = htmlSanitizerService;
            this.commentService = commentService;
        }

        public class ThreadBean
        {
            public Thread<TUser> Thread { get; set; }
            public ThreadLastViewed<TUser> LastViewed { get; set; }
        }
        
        [HttpGet("~/api/forum/{forumId:int}/thread")]
        public async Task<IHttpActionResult<IEnumerable<ThreadView<TUserView>>>> GetFromForum(int forumId, [FromQuery]int offset = 0, [FromQuery]int limit = 10)
        {
            var forum = await session.LoadAsync<Data.Forums.Forum>(forumId);
            var account = await accountService.GetCurrentUserAsync();
            if (forum.ReadRole != null && !await accountService.HasRole(account, forum.ReadRole))
                return Unauthorized<IEnumerable<ThreadView<TUserView>>>();

            if (account != null)
            {
                using (var transaction = session.BeginTransaction())
                {
                    var lastViewed = await session.SelectAllFrom<LastForumView<TUser>>().Where(l => l.Forum == forum && l.Account.Equals(account)).FirstOrDefaultAsync();
                    if (lastViewed == null)
                    {
                        lastViewed = new LastForumView<TUser> { Account = account, Forum = forum, LastView = DateTime.UtcNow };
                        await session.SaveAsync(lastViewed);
                    }
                    else
                    {
                        lastViewed.LastView = DateTime.UtcNow;
                        await session.UpdateAsync(lastViewed);
                    }
                    transaction.Commit();
                }

                var query = session.Select<ThreadBean>().All(x => x.Thread).All(x => x.LastViewed).All(x => x.Thread.Author).From(x => x.Thread).LeftJoin(x => x.LastViewed).On(x => x.Thread == x.LastViewed.Thread)
                    .AndOn(t => t.LastViewed.Account.Equals(account))
                    .LeftJoinOnId(x => x.Thread.Author)
                    .Where(t => t.Thread.Forum == forum).OrderBy(t => t.Thread.Sticky).Desc().OrderBy(t => t.Thread.CreationDate).Desc().Limit(offset, limit);

                var results = await query.ToListAsync();
                return Ok(results.Select(b => forumsDataMapping.ToThreadView(b.Thread, b.LastViewed)));
            }
            else
            {
                var query = session.SelectAllFrom<Thread<TUser>>(x => x.Author).Where(t => t.Forum == forum).OrderBy(t => t.Sticky).Desc().OrderBy(t => t.CreationDate).Desc().Limit(offset, limit);
                return Ok((await query.ToListAsync()).Select(t => forumsDataMapping.ToThreadView(t, null)));
            }
                
        }

        [HttpGet("{id:int}", Name = "GetThread")]
        public async Task<IHttpActionResult<ThreadFullView<TUserView>>> Get(int id)
        {
            var thread = await session.LoadAsync<Thread<TUser>>(id, x => x.Author, x => x.Forum);
            var account = await accountService.GetCurrentUserAsync();
            var forum = thread.Forum;
            if (forum.ReadRole != null && (account == null || !await accountService.HasRole(account, forum.ReadRole)))
                return Unauthorized<ThreadFullView<TUserView>>();
            var lastViewed = account == null ? null : await session.SelectAllFrom<ThreadLastViewed<TUser>>().Where(t => t.Account.Equals(account) && t.Thread == thread).SingleOrDefaultAsync();
            return Ok(forumsDataMapping.ToThreadFullView(thread, lastViewed));
        }

        [HttpPost("~/api/forum/{forumId:int}/thread")]
        public async Task<IHttpActionResult<ThreadView<TUserView>>> Post(int forumId, [FromBody]ThreadView<TUserView> value)
        {
            var forum = await session.LoadAsync<Data.Forums.Forum>(forumId);
            var account = await accountService.GetCurrentUserAsync();
            if (account == null || (forum.ReadRole != null && !await accountService.HasRole(account, forum.ReadRole)))
                return Unauthorized<ThreadView<TUserView>>();
            if (forum.WriteRole != null && !await accountService.HasRole(account, forum.WriteRole))
                return Unauthorized<ThreadView<TUserView>>();

            using (var transaction = session.BeginTransaction())
            {
                var html = await htmlSanitizerService.Sanitize(value.Text, account);
                var thread = new Thread<TUser>
                {
                    NumberOfComments = 0,
                    CreationDate = DateTime.UtcNow,
                    Sticky = value.Sticky,
                    Forum = forum,
                    Text = html,
                    Title = value.Title,
                    Author = account
                };
                await session.SaveAsync(thread);
                
                transaction.Commit();
                return Created("GetThread", thread.Id, forumsDataMapping.ToThreadView(thread, null));
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, [FromBody]ThreadView<TUserView> value)
        {
            using (var transaction = session.BeginTransaction())
            {
                var thread = await session.LoadAsync<Thread<TUser>>(id);
                var account = await accountService.GetCurrentUserAsync();
                if (!await accountService.IsUser(account, thread.Author))
                    return Unauthorized();

                thread.Title = value.Title;
                thread.Text = await htmlSanitizerService.Sanitize(value.Text, account);
                thread.Sticky = value.Sticky;
                await session.UpdateAsync(thread);
                transaction.Commit();
            }
            return Ok();
        }
        
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            using (var transaction = session.BeginTransaction())
            {
                var thread = await session.LoadAsync<Thread<TUser>>(id);
                var account = await accountService.GetCurrentUserAsync();
                if (!await accountService.IsUser(account, thread.Author))
                    return Unauthorized();
                await session.DeleteAsync(thread);
                transaction.Commit();
            }
            return Ok();
        }

        [NonAction]
        private async Task SetLastViewed(int threadId, int count)
        {
            var account = await accountService.GetCurrentUserAsync();
            using (var transaction = session.BeginTransaction())
            {
                var thread = await session.LoadAsync<Thread<TUser>>(threadId);
                var was = await session.SelectAllFrom<ThreadLastViewed<TUser>>().Where(t => t.Thread == thread && t.Account.Equals(account)).FirstOrDefaultAsync();
                if (was == null)
                {
                    was = new ThreadLastViewed<TUser> { Account = account, Thread = thread, NumberOfComments = count };
                    await session.SaveAsync(was);
                    transaction.Commit();
                }
                else if (was.NumberOfComments != count)
                {
                    was.NumberOfComments = count;
                    await session.UpdateAsync(was);
                    transaction.Commit();
                }
            }
        }

        [HttpGet("{threadId:int}/comment")]
        public async Task<IEnumerable<CommentView<TUserView>>> GetComments(int threadId)
        {
            var account = await accountService.GetCurrentUserAsync();
            var comments = (await commentService.GetComments(account, threadId)).ToList();
            if (account != null)
                await SetLastViewed(threadId, comments.Count);
            return comments;
        }

        [HttpPost("{threadId:int}/comment")]
        public async Task<IHttpActionResult<CommentView<TUserView>>> PostComment(int threadId, [FromBody]CommentView<TUserView> value)
        {
            var comment = await commentService.PostComment(await accountService.GetCurrentUserAsync(), threadId, value);
            if (comment == null)
                return BadRequest<CommentView<TUserView>>("Can't comment");
            await SetLastViewed(threadId, (await session.LoadAsync<Thread<TUser>>(threadId)).NumberOfComments);
            return Created("GetComment", comment.Id, comment);
        }

        [HttpDelete("{threadId:int}/comment/{id:int}")]
        public async Task<IActionResult> DeleteComment(int threadId, int id)
        {
            if (!await commentService.DeleteComment(await accountService.GetCurrentUserAsync(), threadId, id))
                return BadRequest();
            return Ok();
        }
    }
}
