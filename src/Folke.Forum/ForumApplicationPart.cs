using System.Collections.Generic;
using System.Reflection;
using Folke.Forum.Controllers.Chat;
using Folke.Forum.Controllers.Forums;
using Folke.Forum.Controllers.Misc;
using Folke.Forum.Controllers.Polls;
using Folke.Forum.Controllers.Tags;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace Folke.Forum
{
    public class ForumApplicationPart<TUser, TUserView> : ApplicationPart, IApplicationPartTypeProvider
    {
        public override string Name => "Folke.Forum";

        private readonly List<TypeInfo> types = new List<TypeInfo>
        {
            typeof (ChatController<TUser, TUserView>).GetTypeInfo(),
            typeof (CommentController<TUser, TUserView>).GetTypeInfo(),
            typeof (ForumController<TUser, TUserView>).GetTypeInfo(),
            typeof (ImageController<TUser, TUserView>).GetTypeInfo(),
            typeof (PrivateMessageController<TUser, TUserView>).GetTypeInfo(),
            typeof (ThreadController<TUser, TUserView>).GetTypeInfo(),
            typeof (PollChosenAnswerController<TUser, TUserView>).GetTypeInfo(),
            typeof (PollController<TUser, TUserView>).GetTypeInfo(),
            typeof (ExternalLinkController).GetTypeInfo(),
            typeof (TagController).GetTypeInfo()
        };

        public IEnumerable<TypeInfo> Types => types;
    }
}