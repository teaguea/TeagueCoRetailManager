using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRMDataManager.Library.Internal.DataAccess;
using TRMDataManager.Library.Models;

namespace TRMDataManager.Library.DataAccess
{
    public class SaleData : ISaleData
    {
        private readonly IProductData _productData;
        private readonly ISqlDataAccess _sql;
        private readonly IConfiguration _config;

        public SaleData(IProductData productData, ISqlDataAccess sql, IConfiguration config)
        {
            _productData = productData;
            _sql = sql;
            _config = config;
        }

        public decimal GetTaxRate()
        {
            string rateText = _config.GetValue<string>("TaxRate");

            bool IsValidTaxRate = Decimal.TryParse(rateText, out decimal output);

            if (IsValidTaxRate == false)
            {
                throw new ConfigurationErrorsException("The tax rate is not SettingAttribute up properly");
            }

            output = output / 100;

            return output;
        }

        public void SaveSale(SaleModel saleInfo, string casherId)
        {
            List<SaleDetailDBModel> details = new List<SaleDetailDBModel>();
            var taxRate = GetTaxRate();


            foreach (var item in saleInfo.SaleDetails)
            {
                var detail = new SaleDetailDBModel
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                };

                var productInfo = _productData.GetProductById(detail.ProductId);

                if (productInfo == null)
                {
                    throw new Exception($"The product Id of { detail.ProductId} could not be found in the database.");
                }

                detail.PurchasePrice = (productInfo.RetailPrice * detail.Quantity);

                if (productInfo.IsTaxable)
                {
                    detail.Tax = (detail.PurchasePrice * taxRate);
                }

                details.Add(detail);
            }

            SaleDBModel sale = new SaleDBModel
            {
                SubTotal = details.Sum(x => x.PurchasePrice),
                Tax = details.Sum(x => x.Tax),
                CashierId = casherId
            };

            sale.Total = sale.SubTotal + sale.Tax;

            try
            {
                _sql.StartTransaction("TRMData");
                _sql.SaveDataInTransaction("dbo.spSale_Insert", sale);
                sale.Id = _sql.LoadDataInTransaction<int, dynamic>("spSale_Lookup", new { sale.CashierId, sale.SaleDate }).FirstOrDefault();

                foreach (var item in details)
                {
                    item.SaleId = sale.Id;
                    _sql.SaveDataInTransaction("dbo.spSaleDetail_Insert", item);
                }

                _sql.ComitTransaction();
            }
            catch (Exception ex)
            {
                _sql.RollbackTransaction();
                throw;
            }

        }

        public List<SaleReportModel> GetSalesReport()
        {
            var output = _sql.LoadData<SaleReportModel, dynamic>("dbo.spSale_SaleReport", new { }, "TRMData");

            return output;
        }
    }
}
