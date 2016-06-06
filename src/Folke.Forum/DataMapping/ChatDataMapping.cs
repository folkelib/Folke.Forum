using Folke.Forum.Data.Chats;
using Folke.Forum.Service;
using Folke.Forum.Views.Chat;

namespace Folke.Forum.DataMapping
{
    public class ChatDataMapping<TUser, TUserView>
    {
        private readonly IForumUserService<TUser, TUserView> forumUserService;

        public ChatDataMapping(IForumUserService<TUser, TUserView> forumUserService)
        {
            this.forumUserService = forumUserService;
        }

        public ChatView<TUserView> ToChatDto(Chat<TUser> chat)
        {
            return new ChatView<TUserView>
            {
                Id = chat.Id,
                Author = forumUserService.MapToUserView(chat.Author),
                Text = chat.Text,
                CreationDate = chat.CreationDate
            };
        }
    }
}
