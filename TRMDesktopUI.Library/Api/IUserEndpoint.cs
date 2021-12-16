using System.Collections.Generic;
using System.Threading.Tasks;
using TRMDesktopUI.Library.Model;

namespace TRMDesktopUI.Library.Api
{
    public interface IUserEndpoint
    {
        Task<List<UserModel>> GetAll();
    }
}