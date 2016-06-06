using Folke.Elm;

namespace Folke.Forum.Data.Comments
{
    public interface ICommentMap<T, TUser> : IFolkeTable where T:ICommentable
    {
        T Commentable { get; set; }
        Comment<TUser> Comment { get; set; }
    }
}
