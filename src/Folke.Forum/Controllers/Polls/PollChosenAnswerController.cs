using System.Threading.Tasks;
using Folke.Elm;
using Folke.Elm.Fluent;
using Folke.Forum.Data.Polls;
using Folke.Forum.DataMapping;
using Folke.Forum.Service;
using Folke.Forum.Views.Poll;
using Folke.Mvc.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Folke.Forum.Controllers.Polls
{
    [Route("api/poll-chosen-answer")]
    public class PollChosenAnswerController<TUser, TUserView> : TypedControllerBase
    {
        private readonly IFolkeConnection session;
        private readonly IForumUserService<TUser, TUserView> accountService;
        private readonly PollDataMapping<TUser, TUserView> pollDataMapping;

        public PollChosenAnswerController(IFolkeConnection session, IForumUserService<TUser, TUserView> accountService, PollDataMapping<TUser, TUserView> pollDataMapping)
        {
            this.session = session;
            this.accountService = accountService;
            this.pollDataMapping = pollDataMapping;
        }

        [HttpGet("{id:int}", Name = "GetPollChosenAnswer")]
        public async Task<PollChosenAnswerView<TUserView>> GetAnswer(int id)
        {
            return pollDataMapping.ToPollChosenAnswerDto(await session.LoadAsync<PollChosenAnswer<TUser>>(id));
        }

        [HttpPost("~/api/poll/{pollId:int}/answer")]
        public async Task<IHttpActionResult<PollChosenAnswerView<TUserView>>> AddAnswer(int pollId, [FromBody] PollPossibleAnswerView value)
        {
            // Checks that provided Poll is visible
            var poll = await session.LoadAsync<Poll<TUser>>(pollId);
            if (poll.Deleted)
                return BadRequest<PollChosenAnswerView<TUserView>>("Deleted poll");

            var account = await accountService.GetCurrentUserAsync();
            // Checks that account hasn't voted yet
            var answers = await session.SelectAllFrom<PollChosenAnswer<TUser>>()
                                .Where(x => x.Account.Equals(account))
                                .SingleOrDefaultAsync();

            if (answers != null)
                return BadRequest<PollChosenAnswerView<TUserView>>("Already answered");

            using (var transaction = session.BeginTransaction())
            {
                var answer = await session.LoadAsync<PollPossibleAnswer<TUser>>(value.Id);
                if (answer.Poll != poll)
                    return BadRequest<PollChosenAnswerView<TUserView>>("Not a possible answer");

                var chosenAnswer = new PollChosenAnswer<TUser>
                {
                    Poll = poll,
                    Answer = answer,
                    Account = account
                };

                await session.SaveAsync(chosenAnswer);
                answer.Count++;
                await session.UpdateAsync(answer);
                transaction.Commit();

                return Created("GetPollChosenAnswer", chosenAnswer.Id, pollDataMapping.ToPollChosenAnswerDto(chosenAnswer));
            }
        }
    }
}
