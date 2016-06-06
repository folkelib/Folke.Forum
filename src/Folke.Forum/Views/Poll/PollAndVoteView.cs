namespace Folke.Forum.Views.Poll
{
    public class PollAndVoteView<TUserView>
    {
        public PollChosenAnswerView<TUserView> Answer { get; set; }
        public PollView<TUserView> Poll { get; set; }
    }
}
