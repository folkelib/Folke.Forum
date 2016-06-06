namespace Folke.Forum.DataMapping
{
    public interface ICommentMapping<TComment, TCommentView>
    {
        TCommentView ToCommentView(TComment comment);
    }
}
