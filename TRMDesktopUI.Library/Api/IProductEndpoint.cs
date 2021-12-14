using System.Collections.Generic;
using System.Threading.Tasks;
using TRMDesktopUI.Library.Model;

namespace TRMDesktopUI.Library.Api
{
    public interface IProductEndpoint
    {
        Task<List<ProductModel>> GetAll();
    }
}