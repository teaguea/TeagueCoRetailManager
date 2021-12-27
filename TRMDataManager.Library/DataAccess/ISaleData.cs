using System.Collections.Generic;
using TRMDataManager.Library.Models;

namespace TRMDataManager.Library.DataAccess
{
    public interface ISaleData
    {
        List<SaleReportModel> GetSalesReport();
        decimal GetTaxRate();
        void SaveSale(SaleModel saleInfo, string casherId);
    }
}