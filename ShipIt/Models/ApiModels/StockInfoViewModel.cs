using ShipIt.Models.DataModels;
using ShipIt.Repositories;

namespace ShipIt.Models.ApiModels
{
    public class StockInfoViewModel
    {
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public int Held { get; set; }
        private readonly ICompanyRepository _companyRepository;
        private readonly IProductRepository _productRepository;
        public ProductDataModel Product { get; set; }
        public CompanyDataModel Company { get; set; }

        public StockInfoViewModel(IProductRepository productRepository, ICompanyRepository companyRepository, StockDataModel stock, int warehouseId)
        {
            _companyRepository = companyRepository;
            _productRepository = productRepository;

            ProductId = stock.ProductId;
            WarehouseId = warehouseId;
            Held = stock.held;

            Product = _productRepository.GetProductById(ProductId);
            Company = _companyRepository.GetCompany(Product.Gcp);
        }
    }
}
