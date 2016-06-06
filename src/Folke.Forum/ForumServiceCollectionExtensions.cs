using Folke.Forum.Data.Comments;
using Folke.Forum.Data.Forums;
using Folke.Forum.DataMapping;
using Folke.Forum.Service;
using Folke.Forum.Service.Forums;
using Folke.Forum.Service.Tag;
using Folke.Forum.Views.Forums;
using Microsoft.Extensions.DependencyInjection;

namespace Folke.Forum
{
    public static class ForumServiceCollectionExtensions
    {
        public static IServiceCollection AddForum<TUser, TUserView, TForumUserService, TImageStore>(
            this IServiceCollection services)
            where TForumUserService : class, IForumUserService<TUser, TUserView>
            where TImageStore : class, IImageStore
        {
            services.AddScoped<IForumUserService<TUser, TUserView>, TForumUserService>();
            services.AddScoped<IImageStore, TImageStore>();

            services.AddScoped<CommentService<Thread<TUser>, CommentInThread<TUser>, TUser, TUserView>>();
            services.AddScoped<HtmlSanitizerService<TUser>>();
            services.AddScoped<ITagService<TUser>, TagService<TUser>>();

            services.AddScoped<ChatDataMapping<TUser, TUserView>>();
            services.AddScoped<ForumsDataMapping<TUser, TUserView>>();
            services
                .AddScoped<ICommentMapping<Comment<TUser>, CommentView<TUserView>>, ForumsDataMapping<TUser, TUserView>>
                ();
            services.AddScoped<MiscDataMapping>();
            services.AddScoped<PollDataMapping<TUser, TUserView>>();
            services.AddScoped<TagDataMapping<TUser, TUserView>>();
            return services;
        }
    }
}
