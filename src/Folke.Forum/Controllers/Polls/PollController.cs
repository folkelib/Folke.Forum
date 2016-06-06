using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Folke.Elm;
using Folke.Elm.Fluent;
using Folke.Forum.Data.Polls;
using Folke.Forum.DataMapping;
using Folke.Forum.Service;
using Folke.Forum.Views.Poll;
using Folke.Mvc.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Folke.Forum.Controllers.Polls
{
    [Route("api/poll")]
    public class PollController<TUser, TUserView> : TypedControllerBase
    {
        private readonly IFolkeConnection session;
        private readonly IForumUserService<TUser, TUserView> accountService;
        private readonly PollDataMapping<TUser, TUserView> pollDataMapping;

        public PollController(IFolkeConnection session, IForumUserService<TUser, TUserView> accountService, PollDataMapping<TUser, TUserView> pollDataMapping)
        {
            this.session = session;
            this.accountService = accountService;
            this.pollDataMapping = pollDataMapping;
        }

        public class PollAndVote
        {
            public Poll<TUser> Poll { get; set; }
            public PollChosenAnswer<TUser> Answer { get; set; }
        }

        // TODO un peu de sécurité
        // Genre si le thread est soumis à authentification et que le poll est lié au thread, on ne devrait pas pouvoir query le poll
        // TODO Voir "comme dans comments"
        /// <summary>
        /// Returns the poll with the provided Id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:int}", Name="GetPoll")]
        public async Task<IHttpActionResult<PollView<TUserView>>> GetPoll(int id)
        {
            var poll = await session.LoadAsync<Poll<TUser>>(id, x => x.Author);
            if (poll.Deleted)
                return NotFound<PollView<TUserView>>();

            return Ok(pollDataMapping.ToPollDto(poll));
        }

        [HttpGet("{id:int}/vote")]
        public async Task<IHttpActionResult<PollAndVoteView<TUserView>>> GetPollAndVote(int id)
        {
            var account = await accountService.GetCurrentUserAsync();
            var poll = await session.Select<PollAndVote>().All(x => x.Poll).All(x => x.Answer).From(x => x.Poll).LeftJoin(x => x.Answer).On(x => x.Poll == x.Answer.Poll).AndOn(x => x.Answer.Account.Equals(account)).Where(x => x.Poll.Id == id).SingleOrDefaultAsync();
            if (poll == null || poll.Poll.Deleted)
                return NotFound<PollAndVoteView<TUserView>>();
            return Ok(pollDataMapping.ToPollAndVoteDto(poll.Poll, poll.Answer));
        }

        /// <summary>Returns the list of all the open polls.</summary>
        /// <returns>The list of polls</returns>
        [HttpGet]
        [Authorize("ForumAdministrator")]
        public async Task<IHttpActionResult<IEnumerable<PollView<TUserView>>>> Get()
        {
            
            var polls = await session.SelectAllFrom<Poll<TUser>>()
                                            .Where(x => !x.Deleted)
                                            .OrderBy(x => x.OpenDate).Desc()
                                            .Limit(0, 20)
                                            .ToListAsync();
                                            
            return Ok(polls.Select(x => pollDataMapping.ToPollDto(x)));
        }

        // TODO ne poster un poll que si on en a le droit ; pour le moment seuls les admins peuvent
        /// <summary>Creates a new poll.</summary>
        [Authorize("ForumAdministrator")]
        [HttpPost("poll")]
        public async Task<IHttpActionResult<PollView<TUserView>>> Post([FromBody] PollView<TUserView> pollView)
        {
            if (!ModelState.IsValid)
                return BadRequest<PollView<TUserView>>(ModelState);
            
            using (var transaction = session.BeginTransaction())
            {
                var account = await accountService.GetCurrentUserAsync();
                // First we create and save the poll
                // then we create the possible answers
                // attached to that poll.
                var poll = new Poll<TUser>()
                {
                    Author = account,
                    Question = pollView.Question,
                    // For now open and close date are hard coded
                    // TODO: Add date picker
                    OpenDate = DateTime.Now,
                    CloseDate = DateTime.Now.AddDays(7)
                };
                await session.SaveAsync(poll);

                var newAnswers = pollView.PossibleAnswers.Where(a => a.Text != null).ToList();

                foreach (var answerDto in newAnswers)
                {
                    answerDto.Text = answerDto.Text.Trim();
                    if (answerDto.Text.Length >= 1) 
                    { 
                        var possibleAnwer = pollDataMapping.FromPollDto(answerDto, poll);
                        await session.SaveAsync(possibleAnwer);
                    }
                }

                transaction.Commit();
                return Created("GetPoll", poll.Id, pollDataMapping.ToPollDto(poll));
            }
        }
        /// <summary>
        ///  Updates the poll with the provided Id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pollView"></param>
        /// <returns></returns>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, [FromBody] PollView<TUserView> pollView)
        {
            using (var transaction = session.BeginTransaction())
            {
                // Update poll
                var poll = await session.LoadAsync<Poll<TUser>>(id);
                poll.Question = pollView.Question;

                var newAnswers = pollView.PossibleAnswers.Where(a => a.Text != null).ToList();
                                
                // Update or add modified possible answers
                foreach (var possibleAnswerDto in newAnswers)
                {
                    var possibleAnswer = poll.PossibleAnswers.SingleOrDefault(p => p.Id == possibleAnswerDto.Id);

                    if (possibleAnswer != null)
                    {
                        possibleAnswer.Text = possibleAnswerDto.Text.Trim();

                        if (possibleAnswer.Text.Length >= 1)
                        {
                            possibleAnswer.Text = possibleAnswerDto.Text;
                            await session.UpdateAsync(possibleAnswer);
                        }
                    }
                    else
                    {
                        await session.SaveAsync(new PollPossibleAnswer<TUser>
                        {
                            Text = possibleAnswerDto.Text,
                            Poll = poll
                        });
                    }
                }

                // Delete from db possible answers no longer in the Dto (removed by user)
                foreach (var answer in poll.PossibleAnswers.Where(p => newAnswers.All(u => u.Id != p.Id)))
                {
                    await session.DeleteAsync(answer);
                }
                await session.UpdateAsync(poll);

                transaction.Commit();
            }
            return Ok();
        }
        /// <summary>
        /// Deletes the poll with the provided Id.
        /// Note that the poll isn't actually deleted, instead it's Visible property is set to 1.
        /// </summary>
        /// <param name="id">The poll id</param>
        /// <returns></returns>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {            
            var poll = session.Load<Poll<TUser>>(id);
            var user = await accountService.GetCurrentUserAsync();
            if (!await accountService.IsUser(user, poll.Author))
            {
                return Unauthorized();
            }

            using (var transaction = session.BeginTransaction())
            {
                poll.Deleted = true;
                await session.UpdateAsync(poll);
                transaction.Commit();
            }
            return Ok();            
        }
   }
}
