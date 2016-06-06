using Folke.Elm;
using Folke.Forum.Data.Chats;
using Folke.Forum.Data.Comments;
using Folke.Forum.Data.Forums;
using Folke.Forum.Data.Misc;
using Folke.Forum.Data.Polls;
using Folke.Forum.Data.Tags;

namespace Folke.Forum
{
    public static class FolkeConnectionExtensions
    {
        public static void UpdateForumSchema<TUser>(this IFolkeConnection folkeConnection)
        {
            folkeConnection.CreateOrUpdateTable<Chat<TUser>>();
            folkeConnection.CreateOrUpdateTable<LastChatView<TUser>>();

            folkeConnection.CreateOrUpdateTable<Comment<TUser>>();
            
            folkeConnection.CreateOrUpdateTable<Data.Forums.Forum>();
            folkeConnection.CreateOrUpdateTable<LastForumView<TUser>>();
            folkeConnection.CreateOrUpdateTable<Photo<TUser>>();
            folkeConnection.CreateOrUpdateTable<PrivateMessage<TUser>>();
            folkeConnection.CreateOrUpdateTable<PrivateMessageRecipient<TUser>>();
            folkeConnection.CreateOrUpdateTable<PrivateMessageViewed<TUser>>();
            folkeConnection.CreateOrUpdateTable<Thread<TUser>>();
            folkeConnection.CreateOrUpdateTable<ThreadLastViewed<TUser>>();
            folkeConnection.CreateOrUpdateTable<PhotoInThread<TUser>>();
            folkeConnection.CreateOrUpdateTable<CommentInThread<TUser>>();

            folkeConnection.CreateOrUpdateTable<ExternalLink>();

            folkeConnection.CreateOrUpdateTable<Poll<TUser>>();
            folkeConnection.CreateOrUpdateTable<PollPossibleAnswer<TUser>>();
            folkeConnection.CreateOrUpdateTable<PollChosenAnswer<TUser>>();
            folkeConnection.CreateOrUpdateTable<Tag>();
            folkeConnection.CreateOrUpdateTable<TagThread<TUser>>();
        }
    }
}
