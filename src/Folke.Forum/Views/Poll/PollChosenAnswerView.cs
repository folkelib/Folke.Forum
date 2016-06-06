namespace Folke.Forum.Views.Poll
{
    public class PollChosenAnswerView<TUserView>
    {
        public int Id { get; set; }
        public PollPossibleAnswerView Answer { get; set; }
        public TUserView Account { get; set; }
    }
}
