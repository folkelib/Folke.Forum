using Microsoft.Extensions.DependencyInjection;

namespace Folke.Forum
{
    public static class MvcBuilderExtensions
    {
        public static IMvcBuilder AddForum<TUser, TUserView>(this IMvcBuilder builder)
        {
            var part = new ForumApplicationPart<TUser, TUserView>();
            builder.ConfigureApplicationPartManager(manager => manager.ApplicationParts.Add(part));
            return builder;
        }
    }
}
