using NUnit.Framework;
using ShipIt.Controllers;
using ShipIt.Exceptions;
using ShipIt.Models.ApiModels;
using ShipIt.Models.DataModels;
using ShipIt.Repositories;
using ShipItTest.Builders;
using System.Collections.Generic;
using System.Linq;

namespace ShipItTest
{
    public class InboundOrderControllerTests : AbstractBaseTest
    {
        private readonly InboundOrderController inboundOrderController = new InboundOrderController(
            new EmployeeRepository(),
            new CompanyRepository(),
            new ProductRepository(),
            new StockRepository()
        );
        private readonly StockRepository stockRepository = new StockRepository();
        private readonly CompanyRepository companyRepository = new CompanyRepository();
        private readonly ProductRepository productRepository = new ProductRepository();
        private readonly EmployeeRepository employeeRepository = new EmployeeRepository();

        private static readonly Employee OPS_MANAGER = new EmployeeBuilder().CreateEmployee();
        private static readonly Company COMPANY = new CompanyBuilder().CreateCompany();
        private static readonly int WAREHOUSE_ID = OPS_MANAGER.WarehouseId;
        private static readonly string GCP = COMPANY.Gcp;

        private Product product;
        private int productId;
        private const string GTIN = "0000";

        public new void onSetUp()
        {
            base.onSetUp();
            employeeRepository.AddEmployees(new List<Employee>() { OPS_MANAGER });
            companyRepository.AddCompanies(new List<Company>() { COMPANY });
            ProductDataModel productDataModel = new ProductBuilder().setGtin(GTIN).CreateProductDatabaseModel();
            productRepository.AddProducts(new List<ProductDataModel>() { productDataModel });
            product = new Product(productRepository.GetProductByGtin(GTIN));
            productId = product.Id;
        }

        [Test]
        public void TestCreateOrderNoProductsHeld()
        {
            onSetUp();

            InboundOrderResponse inboundOrder = inboundOrderController.Get(WAREHOUSE_ID);

            Assert.AreEqual(inboundOrder.WarehouseId, WAREHOUSE_ID);
            Assert.IsTrue(EmployeesAreEqual(inboundOrder.OperationsManager, OPS_MANAGER));
            Assert.AreEqual(inboundOrder.OrderSegments.Count(), 0);
        }

        [Test]
        public void TestCreateOrderProductHoldingNoStock()
        {
            onSetUp();
            stockRepository.AddStock(WAREHOUSE_ID, new List<StockAlteration>() { new StockAlteration(productId, 0) });

            InboundOrderResponse inboundOrder = inboundOrderController.Get(WAREHOUSE_ID);

            Assert.AreEqual(inboundOrder.OrderSegments.Count(), 1);
            OrderSegment orderSegment = inboundOrder.OrderSegments.First();
            Assert.AreEqual(orderSegment.Company.Gcp, GCP);
        }

        [Test]
        public void TestCreateOrderProductHoldingSufficientStock()
        {
            onSetUp();
            stockRepository.AddStock(WAREHOUSE_ID, new List<StockAlteration>() { new StockAlteration(productId, product.LowerThreshold) });

            InboundOrderResponse inboundOrder = inboundOrderController.Get(WAREHOUSE_ID);

            Assert.AreEqual(inboundOrder.OrderSegments.Count(), 0);
        }

        [Test]
        public void TestCreateOrderDiscontinuedProduct()
        {
            onSetUp();
            stockRepository.AddStock(WAREHOUSE_ID, new List<StockAlteration>() { new StockAlteration(productId, product.LowerThreshold - 1) });
            productRepository.DiscontinueProductByGtin(GTIN);

            InboundOrderResponse inboundOrder = inboundOrderController.Get(WAREHOUSE_ID);

            Assert.AreEqual(inboundOrder.OrderSegments.Count(), 0);
        }

        [Test]
        public void TestProcessManifest()
        {
            onSetUp();
            int quantity = 12;
            InboundManifestRequestModel inboundManifest = new InboundManifestRequestModel()
            {
                WarehouseId = WAREHOUSE_ID,
                Gcp = GCP,
                OrderLines = new List<OrderLine>()
                {
                    new OrderLine()
                    {
                        Gtin = GTIN,
                        Quantity = quantity
                    }
                }
            };

            inboundOrderController.Post(inboundManifest);

            StockDataModel stock = stockRepository.GetStockByWarehouseAndProductIds(WAREHOUSE_ID, new List<int>() { productId })[productId];
            Assert.AreEqual(stock.held, quantity);
        }

        [Test]
        public void TestProcessManifestRejectsDodgyGcp()
        {
            onSetUp();
            int quantity = 12;
            string dodgyGcp = GCP + "XYZ";
            InboundManifestRequestModel inboundManifest = new InboundManifestRequestModel()
            {
                WarehouseId = WAREHOUSE_ID,
                Gcp = dodgyGcp,
                OrderLines = new List<OrderLine>()
                {
                    new OrderLine()
                    {
                        Gtin = GTIN,
                        Quantity = quantity
                    }
                }
            };

            try
            {
                inboundOrderController.Post(inboundManifest);
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (ValidationException e)
            {
                Assert.IsTrue(e.Message.Contains(dodgyGcp));
            }
        }

        [Test]
        public void TestProcessManifestRejectsUnknownProduct()
        {
            onSetUp();
            int quantity = 12;
            string unknownGtin = GTIN + "XYZ";
            InboundManifestRequestModel inboundManifest = new InboundManifestRequestModel()
            {
                WarehouseId = WAREHOUSE_ID,
                Gcp = GCP,
                OrderLines = new List<OrderLine>()
                {
                    new OrderLine()
                    {
                        Gtin = GTIN,
                        Quantity = quantity
                    },
                    new OrderLine()
                    {
                        Gtin = unknownGtin,
                        Quantity = quantity
                    }
                }
            };

            try
            {
                inboundOrderController.Post(inboundManifest);
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (ValidationException e)
            {
                Assert.IsTrue(e.Message.Contains(unknownGtin));
            }
        }

        [Test]
        public void TestProcessManifestRejectsDuplicateGtins()
        {
            onSetUp();
            int quantity = 12;
            InboundManifestRequestModel inboundManifest = new InboundManifestRequestModel()
            {
                WarehouseId = WAREHOUSE_ID,
                Gcp = GCP,
                OrderLines = new List<OrderLine>()
                {
                    new OrderLine()
                    {
                        Gtin = GTIN,
                        Quantity = quantity
                    },
                    new OrderLine()
                    {
                        Gtin = GTIN,
                        Quantity = quantity
                    }
                }
            };

            try
            {
                inboundOrderController.Post(inboundManifest);
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (ValidationException e)
            {
                Assert.IsTrue(e.Message.Contains(GTIN));
            }
        }

        private bool EmployeesAreEqual(Employee A, Employee B)
        {
            return A.WarehouseId == B.WarehouseId
                   && A.Name == B.Name
                   && A.role == B.role
                   && A.ext == B.ext;
        }
    }
}
