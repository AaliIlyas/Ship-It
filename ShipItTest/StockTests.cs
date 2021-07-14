using NUnit.Framework;
using ShipIt.Models.ApiModels;
using ShipIt.Models.DataModels;
using ShipIt.Repositories;
using ShipItTest.Builders;
using System.Collections.Generic;

namespace ShipItTest
{
    public class StockControllerTests : AbstractBaseTest
    {
        private readonly StockRepository stockRepository = new StockRepository();
        private readonly CompanyRepository companyRepository = new CompanyRepository();
        private readonly ProductRepository productRepository = new ProductRepository();

        private const string GTIN = "0000";

        public new void onSetUp()
        {
            base.onSetUp();
            companyRepository.AddCompanies(new List<Company>() { new CompanyBuilder().CreateCompany() });
            productRepository.AddProducts(new List<ProductDataModel>() { new ProductBuilder().setGtin(GTIN).CreateProductDatabaseModel() });
        }

        [Test]
        public void TestAddNewStock()
        {
            onSetUp();
            int productId = productRepository.GetProductByGtin(GTIN).Id;

            stockRepository.AddStock(1, new List<StockAlteration>() { new StockAlteration(productId, 1) });

            Dictionary<int, StockDataModel> databaseStock = stockRepository.GetStockByWarehouseAndProductIds(1, new List<int>() { productId });
            Assert.AreEqual(databaseStock[productId].held, 1);
        }

        [Test]
        public void TestUpdateExistingStock()
        {
            onSetUp();
            int productId = productRepository.GetProductByGtin(GTIN).Id;
            stockRepository.AddStock(1, new List<StockAlteration>() { new StockAlteration(productId, 2) });

            stockRepository.AddStock(1, new List<StockAlteration>() { new StockAlteration(productId, 5) });

            Dictionary<int, StockDataModel> databaseStock = stockRepository.GetStockByWarehouseAndProductIds(1, new List<int>() { productId });
            Assert.AreEqual(databaseStock[productId].held, 7);
        }
    }
}
