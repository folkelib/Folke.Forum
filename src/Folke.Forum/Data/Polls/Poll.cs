using System;
using System.Collections.Generic;
using Folke.Elm;

namespace Folke.Forum.Data.Polls
{
    public class Poll<TUser> : IFolkeTable
    {
        public int Id { get; set; }
        public TUser Author { get; set; }
        /// <summary>
        /// The question for that poll. Multi-question polls are not supported in this module.
        /// </summary>
        public string Question { get; set; }
        /// <summary>
        /// The possible answers for that poll.
        /// </summary>
        public IReadOnlyList<PollPossibleAnswer<TUser>> PossibleAnswers { get; set; }
        /// <summary>
        /// The answers picked by the users.
        /// </summary>
        public IReadOnlyList<PollChosenAnswer<TUser>> ChosenAnswers { get; set; }
        /// <summary>
        /// Allows for delaying opening of poll (should default to ICommentable CreationDate otherwise)
        /// </summary>
        public DateTime OpenDate { get; set; }
        /// <summary>
        /// Allows for closing the poll at a given date.
        /// </summary>
        public DateTime CloseDate { get; set; }
        /// <summary>
        /// Toogles wether or not this poll is visible.
        /// When a poll is deleted, this is toogled to true.
        /// </summary>
        public bool Deleted { get; set; }
    }
}
