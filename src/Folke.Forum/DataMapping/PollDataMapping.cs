using System.Linq;
using Folke.Forum.Data.Polls;
using Folke.Forum.Service;
using Folke.Forum.Views.Poll;

namespace Folke.Forum.DataMapping
{
    public class PollDataMapping<TUser, TUserView>
    {
        private readonly IForumUserService<TUser, TUserView> forumUserService;

        public PollDataMapping(IForumUserService<TUser, TUserView> forumUserService)
        {
            this.forumUserService = forumUserService;
        }

        public PollAndVoteView<TUserView> ToPollAndVoteDto(Poll<TUser> poll, PollChosenAnswer<TUser> chosenAnswer)
        {
            return new PollAndVoteView<TUserView>
            {
                Poll = ToPollDto(poll),
                Answer = chosenAnswer != null ? ToPollChosenAnswerDto(chosenAnswer) : null
            };
        }
        
        public PollView<TUserView> ToPollDto(Poll<TUser> poll)
        {
            return new PollView<TUserView>
            {
                Id = poll.Id,
                Author = forumUserService.MapToUserView(poll.Author),
                Question = poll.Question,
                PossibleAnswers = poll.PossibleAnswers.Select(ToPollPossibleAnswerDto).ToList(),
                OpenDate = poll.OpenDate,
                CloseDate = poll.CloseDate
            };
        }

        public PollPossibleAnswer<TUser> FromPollDto(PollPossibleAnswerView pollPossibleAnswerView, Poll<TUser> poll)
        {
            return new PollPossibleAnswer<TUser>
            {
                Id = pollPossibleAnswerView.Id,
                Text = pollPossibleAnswerView.Text,
                Poll = poll
            };
        }

        public PollPossibleAnswerView ToPollPossibleAnswerDto(PollPossibleAnswer<TUser> possibleAnswer)
        {
            return new PollPossibleAnswerView
            {
                Id = possibleAnswer.Id,
                Text = possibleAnswer.Text,
                Count = possibleAnswer.Count
            };
        }

        public PollChosenAnswerView<TUserView> ToPollChosenAnswerDto(PollChosenAnswer<TUser> chosenAnswer)
        {
            return new PollChosenAnswerView<TUserView>
            {
                Id = chosenAnswer.Id,
                Answer = ToPollPossibleAnswerDto(chosenAnswer.Answer),
                Account = forumUserService.MapToUserView(chosenAnswer.Account)
            };
        }
    }
}
