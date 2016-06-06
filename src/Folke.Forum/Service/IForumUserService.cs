using System.Collections.Generic;
using System.Threading.Tasks;

namespace Folke.Forum.Service
{
    public interface IForumUserService<TUser, TUserView>
    {
        TUserView MapToUserView(TUser user);

        Task<TUser> GetCurrentUserAsync();

        Task<bool> IsUser(TUser loggedUser, TUser accessedUser);

        Task<bool> HasRole(TUser user, string role);

        Task<IEnumerable<TUser>> GetUsersAsync(IEnumerable<TUserView> userViews);
    }
}
