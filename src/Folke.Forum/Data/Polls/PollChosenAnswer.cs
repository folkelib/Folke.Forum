using Folke.Elm;
using Folke.Elm.Mapping;

namespace Folke.Forum.Data.Polls
{
    public class PollChosenAnswer<TUser> : IFolkeTable
    {
        public int Id { get; set; }
        [ColumnConstraint(OnDelete=ConstraintEventEnum.Cascade)]
        public Poll<TUser> Poll { get; set; }
        [ColumnConstraint(OnDelete = ConstraintEventEnum.Cascade)]
        public PollPossibleAnswer<TUser> Answer { get; set; }
        public TUser Account { get; set; }
    }
}