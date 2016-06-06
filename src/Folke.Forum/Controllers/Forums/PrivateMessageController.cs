using System.Collections.Generic;
using System.Threading.Tasks;
using Folke.Elm;
using Folke.Forum.Data.Forums;
using Folke.Forum.DataMapping;
using Folke.Forum.Service;
using Folke.Forum.Service.Forums;
using Folke.Forum.Views.Forums;
using Folke.Mvc.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Folke.Forum.Controllers.Forums
{
    [Route("api/private-message")]
    public class PrivateMessageController<TUser, TUserView> : TypedControllerBase
    {
        private readonly IFolkeConnection session;
        private readonly IForumUserService<TUser, TUserView> accountService;
        private readonly ForumsDataMapping<TUser, TUserView> forumsDataMapping;
        private readonly HtmlSanitizerService<TUser> htmlSanitizerService;

        public PrivateMessageController(IFolkeConnection session,
            IForumUserService<TUser, TUserView> accountService, 
            ForumsDataMapping<TUser, TUserView> forumsDataMapping,
            HtmlSanitizerService<TUser> htmlSanitizerService)
        {
            this.session = session;
            this.accountService = accountService;
            this.forumsDataMapping = forumsDataMapping;
            this.htmlSanitizerService = htmlSanitizerService;
        }

        [HttpPost]
        public async Task<IHttpActionResult<PrivateMessageView<TUserView>>> Post([FromBody] PrivateMessageView<TUserView> value)
        {
            var account = await accountService.GetCurrentUserAsync();
            if (account == null)
                return Unauthorized<PrivateMessageView<TUserView>>();
            if (value.AccountRecipients.Count == 0)
                return BadRequest<PrivateMessageView<TUserView>>("Aucun destinataire");
            using (var transaction = session.BeginTransaction())
            {
                var recipientAccounts = await accountService.GetUsersAsync(value.AccountRecipients);

                var html = await htmlSanitizerService.Sanitize(value.Text, account);

                var privateMessage = new PrivateMessage<TUser>
                {
                    Author = account,
                    Text = html,
                    Title = value.Title
                };

                session.Save(privateMessage);
                
                var recipients = new List<PrivateMessageRecipient<TUser>>();

                foreach (var recipientAccount in recipientAccounts)
                {
                    var recipient = new PrivateMessageRecipient<TUser>
                    {
                        PrivateMessage = privateMessage,
                        Recipient = recipientAccount
                    };
                    recipients.Add(recipient);
                    await session.SaveAsync(recipient);
                }
                privateMessage.Recipients = recipients;
                transaction.Commit();
                return Created("GetPrivateMessage", value.Id, forumsDataMapping.ToPrivateMessageView(privateMessage));
            }
        }

        [HttpGet]
        [Route("{id:int}", Name = "GetPrivateMessage")]
        public async Task<IHttpActionResult<PrivateMessageView<TUserView>>> Get(int id)
        {
            var user = await accountService.GetCurrentUserAsync();
            var message = await session.LoadAsync<PrivateMessage<TUser>>(id);
            if (!await accountService.IsUser(user, message.Author))
            {
                return Unauthorized<PrivateMessageView<TUserView>>();
            }
            return Ok(forumsDataMapping.ToPrivateMessageView(message));
        }
    }
}
