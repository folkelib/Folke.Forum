using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Folke.Elm;
using Folke.Elm.Fluent;
using Folke.Forum.Data.Chats;
using Folke.Forum.DataMapping;
using Folke.Forum.Service;
using Folke.Forum.Views.Chat;
using Folke.Mvc.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Folke.Forum.Controllers.Chat
{
    [Route("api/chat")]
    public class ChatController<TUser, TUserView> : TypedControllerBase
    {
        private readonly IFolkeConnection session;
        private readonly IForumUserService<TUser, TUserView> userService;
        private readonly ChatDataMapping<TUser, TUserView> chatDataMapping;
        private static readonly PreparedQueryBuilder<LastChatView<TUser>, TUser> queryLastChatViewByAccount = new PreparedQueryBuilder<LastChatView<TUser>, TUser>(q => q.All().From().Where((c,p) => c.Account.Equals(p.Item0)));
        private static readonly PreparedQueryBuilder<Chat<TUser>> queryLastChats = new PreparedQueryBuilder<Chat<TUser>>(q => q.All().From().OrderBy(c => c.Id).Desc().Limit(0,1));
        private static readonly PreparedQueryBuilder<Chat<TUser>, int> queryChats = new PreparedQueryBuilder<Chat<TUser>, int>(q => q.All().All(x => x.Author).From().LeftJoinOnId(c => c.Author).OrderBy(c => c.CreationDate).Desc().Limit((x,p) => p.Item0, 25));

        public ChatController(IFolkeConnection session, IForumUserService<TUser, TUserView> userService, ChatDataMapping<TUser, TUserView> chatDataMapping)
        {
            this.session = session;
            this.userService = userService;
            this.chatDataMapping = chatDataMapping;
        }

        [HttpGet]
        public async Task<IEnumerable<ChatView<TUserView>>> GetAll([FromQuery] int offset = 0)
        {
            var account = await userService.GetCurrentUserAsync();
            if (account != null && offset == 0)
            {
                using (var transaction = session.BeginTransaction())
                {
                    var myLastChat = await queryLastChatViewByAccount.Build(session, account).FirstOrDefaultAsync();
                    var lastChat = await queryLastChats.Build(session).FirstOrDefaultAsync();
                    if (myLastChat == null)
                    {
                        myLastChat = new LastChatView<TUser> { Account = account, Chat = lastChat };
                        await session.SaveAsync(myLastChat);
                        transaction.Commit();
                    }
                    else if (lastChat != myLastChat.Chat)
                    {
                        myLastChat.Chat = lastChat;
                        await session.UpdateAsync(myLastChat);
                        transaction.Commit();
                    }
                }
            }
            return (await queryChats.Build(session, offset).ToListAsync()).Select(c => chatDataMapping.ToChatDto(c));
        }

        [HttpGet("new")]
        public async Task<bool> GetNewChats()
        {
            var account = await userService.GetCurrentUserAsync();
            if (account == null)
                return false;
            var myLastChat = session.Select<LastChatView<TUser>>().Values(x => x.Chat).From().Where(c => c.Account.Equals(account)).Scalar<int>();
            var lastChat = session.Select<Chat<TUser>>().Values(x => SqlFunctions.Max(x.Id)).From().Scalar<int>();
            return lastChat > myLastChat;
        }

        [Route("{id:int}", Name="GetChat")]
        public async Task<ChatView<TUserView>> Get(int id)
        {
            return chatDataMapping.ToChatDto(await session.LoadAsync<Chat<TUser>>(id));
        }
        
        [HttpPost]
        public async Task<IHttpActionResult<ChatView<TUserView>>> Post([FromBody]ChatView<TUserView> value)
        {
            var account = await userService.GetCurrentUserAsync();
            if (account == null)
                return Unauthorized<ChatView<TUserView>>();
            using (var transaction = session.BeginTransaction())
            {
                var chat = new Chat<TUser> { CreationDate = DateTime.UtcNow, Text = value.Text, Author = account };
                await session.SaveAsync(chat);
                value.CreationDate = chat.CreationDate;
                value.Id = chat.Id;
                value.Author = userService.MapToUserView(account);
                transaction.Commit();
            }
            return Created("GetChat", value.Id, value);
        }
    }
}
