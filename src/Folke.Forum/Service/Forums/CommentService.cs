using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Folke.Elm;
using Folke.Forum.DataMapping;
using Folke.Elm.Fluent;
using Folke.Forum.Data.Comments;
using Folke.Forum.Views.Forums;

namespace Folke.Forum.Service.Forums
{
    public class CommentService<TThread, TCommentInThread, TUser, TUserView>
            where TThread : class, IFolkeTable, ICommentable, new()
            where TCommentInThread : class, ICommentMap<TThread, TUser>, new()
    {
        private readonly ForumsDataMapping<TUser, TUserView> commentMapping;
        private readonly IFolkeConnection session;
        private readonly IForumUserService<TUser, TUserView> forumUserService;

        public CommentService(ForumsDataMapping<TUser, TUserView> commentMapping, IFolkeConnection session, IForumUserService<TUser, TUserView> forumUserService)
        {
            this.commentMapping = commentMapping;
            this.session = session;
            this.forumUserService = forumUserService;
        }

        public async Task<IEnumerable<CommentView<TUserView>>> GetComments(TUser account, int newsId, bool descending = false)
        {
            var news = await session.LoadAsync<TThread>(newsId);
            var comments = (await session.SelectAllFrom<TCommentInThread>(x => x.Comment, x => x.Comment.Author).Where(c => c.Commentable == news).ToListAsync()).Select(x => x.Comment);

            if (descending)
            {
                return comments.OrderByDescending(c => c.CreationDate).Select(c => commentMapping.ToCommentView(c));
            }

            return comments.OrderBy(c => c.CreationDate).Select(c =>  commentMapping.ToCommentView(c));
        }

        public async Task<CommentView<TUserView>> PostComment(TUser account, int newsId, CommentView<TUserView> value)
        {
            if (account == null)
                return null;
            using (var transaction = session.BeginTransaction())
            {
                var news = await session.LoadAsync<TThread>(newsId);
                var comment = new Comment<TUser> { Author = account, Text = value.Text, CreationDate = DateTime.UtcNow };
                await session.SaveAsync(comment);
                news.NumberOfComments++;
                await session.UpdateAsync(news);
                var commentInNews = new TCommentInThread { Comment = comment, Commentable = news };
                await session.SaveAsync(commentInNews);
                transaction.Commit();
                return commentMapping.ToCommentView(comment);
            }
        }

        public async Task<bool> DeleteComment(TUser account, int newsId, int id)
        {
            using (var transaction = session.BeginTransaction())
            {
                var commentInThread = await session.LoadAsync<TCommentInThread>(id, x => x.Comment, x => x.Commentable);
                if (!await forumUserService.IsUser(account, commentInThread.Comment.Author))
                    return false;

                await session.DeleteAsync(commentInThread.Comment);
                await session.DeleteAsync(commentInThread);

                transaction.Commit();
                return true;
            }
        }
    }
}
