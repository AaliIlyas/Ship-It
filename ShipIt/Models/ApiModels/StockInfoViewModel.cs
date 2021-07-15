using ShipIt.Models.DataModels;
using ShipIt.Repositories;

namespace ShipIt.Models.ApiModels
{
    public class StockInfoViewModel
    {
        public int WarehouseId { get; set; }
        public int Held { get; set; }
        public ProductDataModel Product { get; set; }
        public CompanyDataModel Company { get; set; }

        public StockInfoViewModel(ProductDataModel product, CompanyDataModel company, StockDataModel stock, int warehouseId)
        {
            WarehouseId = warehouseId;
            Held = stock.held;
            Product = product;
            Company = company;
        }
    }
}
