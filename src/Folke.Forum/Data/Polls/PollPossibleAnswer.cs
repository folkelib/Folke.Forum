using Folke.Elm;
using Folke.Elm.Mapping;

namespace Folke.Forum.Data.Polls
{
    public class PollPossibleAnswer<TUser> : IFolkeTable
    {
        public int Id { get; set; }
        public string Text { get; set; }
        [ColumnConstraint(OnDelete=ConstraintEventEnum.Cascade)]
        public Poll<TUser> Poll { get; set; }
        public int Count { get; set; }
    }
}
