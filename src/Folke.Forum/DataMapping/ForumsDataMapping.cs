using System.Linq;
using Folke.Forum.Data.Comments;
using Folke.Forum.Data.Forums;
using Folke.Forum.Data.Polls;
using Folke.Forum.Service;
using Folke.Forum.Views.Forums;

namespace Folke.Forum.DataMapping
{
    public class ForumsDataMapping<TUser, TUserView> : ICommentMapping<Comment<TUser>, CommentView<TUserView>>
    {
        private readonly IForumUserService<TUser, TUserView> forumUserService;
        private readonly PollDataMapping<TUser, TUserView> pollDataMapping;

        public ForumsDataMapping(IForumUserService<TUser, TUserView> forumUserService, PollDataMapping<TUser, TUserView> pollDataMapping)
        {
            this.forumUserService = forumUserService;
            this.pollDataMapping = pollDataMapping;
        }

        public ForumView ToForumView(Data.Forums.Forum forum, int numberOfNewComments)
        {
            if (forum == null) return null;
            return new ForumView
            {
                Id = forum.Id,
                Name = forum.Name,
                NumberOfNewMessages = numberOfNewComments,
                ReadRole = forum.ReadRole,
                WriteRole = forum.WriteRole
            };
        }

        public CommentView<TUserView> ToCommentView(Comment<TUser> comment)
        {
            return new CommentView<TUserView>
            {
                Id = comment.Id,
                Author = forumUserService.MapToUserView(comment.Author),
                Text = comment.Text,
                CreationDate = comment.CreationDate
            };
        }

        public ThreadView<TUserView> ToThreadView(Thread<TUser> thread, ThreadLastViewed<TUser> lastViewed)
        {
            return new ThreadView<TUserView>
            {
                Id = thread.Id,
                CreationDate = thread.CreationDate,
                Author = forumUserService.MapToUserView(thread.Author),
                Title = thread.Title,
                Text = thread.Text,
                Sticky = thread.Sticky,
                NumberOfComments = thread.NumberOfComments,
                NumberOfViewedComments = lastViewed?.NumberOfComments ?? 0,
                LastViewedId = lastViewed?.Id ?? 0
            };
        }

        public ThreadFullView<TUserView> ToThreadFullView(Thread<TUser> thread, 
            ThreadLastViewed<TUser> lastViewed)
        {
            return new ThreadFullView<TUserView>
            {
                Id = thread.Id,
                CreationDate = thread.CreationDate,
                Author = forumUserService.MapToUserView(thread.Author),
                Title = thread.Title,
                Text = thread.Text,
                Sticky = thread.Sticky,
                NumberOfComments = thread.NumberOfComments,
                NumberOfViewedComments = lastViewed?.NumberOfComments ?? 0,
                LastViewedId = lastViewed?.Id ?? 0,
                Photos = thread.Photos.Select(x => ToPhotoView(x.Photo)).ToList()
            };
        }

        public PrivateMessageView<TUserView> ToPrivateMessageView(PrivateMessage<TUser> privateMessage)
        {
            return new PrivateMessageView<TUserView>
            {
                Author = forumUserService.MapToUserView(privateMessage.Author),
                Text = privateMessage.Text,
                Title = privateMessage.Title,
                AccountRecipients = privateMessage.Recipients.Select(x => forumUserService.MapToUserView(x.Recipient)).ToList(),
                CreationDate = privateMessage.CreationDate,
                Id = privateMessage.Id
            };
        }

        public PhotoView ToPhotoView(Photo<TUser> photo)
        {
            if (photo == null)
                return null;

            return new PhotoView
            {
                Id = photo.Id,
                FileUrl = "/Content/upload/" + photo.FileName,
                Width = photo.Width,
                Height = photo.Height
            };
        }
    }
}
