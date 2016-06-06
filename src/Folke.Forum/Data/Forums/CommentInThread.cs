using Folke.Forum.Data.Comments;

namespace Folke.Forum.Data.Forums
{
    public class CommentInThread<TUser>: ICommentMap<Thread<TUser>, TUser>
    {
        public virtual int Id { get; set; }
        public virtual Comment<TUser> Comment { get; set; }
        public virtual Thread<TUser> Commentable { get; set; }
    }
}
