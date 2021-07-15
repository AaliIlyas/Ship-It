using NUnit.Framework;
using ShipIt.Controllers;
using ShipIt.Exceptions;
using ShipIt.Models.ApiModels;
using ShipIt.Models.DataModels;
using ShipIt.Repositories;
using ShipItTest.Builders;
using System;
using System.Collections.Generic;

namespace ShipItTest
{
    public class OutboundOrderControllerTests : AbstractBaseTest
    {
        private readonly OutboundOrderController outboundOrderController = new OutboundOrderController(
            new StockRepository(),
            new ProductRepository()
        );
        private readonly StockRepository stockRepository = new StockRepository();
        private readonly CompanyRepository companyRepository = new CompanyRepository();
        private readonly ProductRepository productRepository = new ProductRepository();
        private readonly EmployeeRepository employeeRepository = new EmployeeRepository();

        private static readonly Employee EMPLOYEE = new EmployeeBuilder().CreateEmployee();
        private static readonly Company COMPANY = new CompanyBuilder().CreateCompany();
        private static readonly int WAREHOUSE_ID = EMPLOYEE.WarehouseId;

        private Product product;
        private int productId;
        private const string GTIN = "0000";

        public new void onSetUp()
        {
            base.onSetUp();
            employeeRepository.AddEmployees(new List<Employee>() { EMPLOYEE });
            companyRepository.AddCompanies(new List<Company>() { COMPANY });
            ProductDataModel productDataModel = new ProductBuilder().setGtin(GTIN).CreateProductDatabaseModel();
            productRepository.AddProducts(new List<ProductDataModel>() { productDataModel });
            product = new Product(productRepository.GetProductByGtin(GTIN));
            productId = product.Id;
        }

        [Test]
        public void TestOutboundOrder()
        {
            onSetUp();
            stockRepository.AddStock(WAREHOUSE_ID, new List<StockAlteration>() { new StockAlteration(productId, 10) });
            OutboundOrderRequestModel outboundOrder = new OutboundOrderRequestModel()
            {
                WarehouseId = WAREHOUSE_ID,
                OrderLines = new List<OrderLine>()
                {
                    new OrderLine()
                    {
                        Gtin = GTIN,
                        Quantity = 3
                    }
                }
            };

            var output = outboundOrderController.Post(outboundOrder);
            //Console.WriteLine(output);

            StockDataModel stock = stockRepository.GetStockByWarehouseAndProductIds(WAREHOUSE_ID, new List<int>() { productId })[productId];
            Assert.AreEqual(stock.held, 7);
            Assert.AreEqual(1, output.Trucks);
        }

        [Test]
        public void TestOutboundOrderInsufficientStock()
        {
            onSetUp();
            stockRepository.AddStock(WAREHOUSE_ID, new List<StockAlteration>() { new StockAlteration(productId, 10) });
            OutboundOrderRequestModel outboundOrder = new OutboundOrderRequestModel()
            {
                WarehouseId = WAREHOUSE_ID,
                OrderLines = new List<OrderLine>()
                {
                    new OrderLine()
                    {
                        Gtin = GTIN,
                        Quantity = 11
                    }
                }
            };

            try
            {
                outboundOrderController.Post(outboundOrder);
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (InsufficientStockException e)
            {
                Assert.IsTrue(e.Message.Contains(GTIN));
            }
        }

        [Test]
        public void TestOutboundOrderStockNotHeld()
        {
            onSetUp();
            string noStockGtin = GTIN + "XYZ";
            productRepository.AddProducts(new List<ProductDataModel>() { new ProductBuilder().setGtin(noStockGtin).CreateProductDatabaseModel() });
            stockRepository.AddStock(WAREHOUSE_ID, new List<StockAlteration>() { new StockAlteration(productId, 10) });

            OutboundOrderRequestModel outboundOrder = new OutboundOrderRequestModel()
            {
                WarehouseId = WAREHOUSE_ID,
                OrderLines = new List<OrderLine>()
                {
                    new OrderLine()
                    {
                        Gtin = GTIN,
                        Quantity = 8
                    },
                    new OrderLine()
                    {
                        Gtin = noStockGtin,
                        Quantity = 1000
                    }
                }
            };

            try
            {
                outboundOrderController.Post(outboundOrder);
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (InsufficientStockException e)
            {
                Assert.IsTrue(e.Message.Contains(noStockGtin));
                Assert.IsTrue(e.Message.Contains("no stock held"));
            }
        }

        [Test]
        public void TestOutboundOrderBadGtin()
        {
            onSetUp();
            string badGtin = GTIN + "XYZ";

            OutboundOrderRequestModel outboundOrder = new OutboundOrderRequestModel()
            {
                WarehouseId = WAREHOUSE_ID,
                OrderLines = new List<OrderLine>()
                {
                    new OrderLine()
                    {
                        Gtin = GTIN,
                        Quantity = 1
                    },
                    new OrderLine()
                    {
                        Gtin = badGtin,
                        Quantity = 1
                    }
                }
            };

            try
            {
                outboundOrderController.Post(outboundOrder);
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (NoSuchEntityException e)
            {
                Assert.IsTrue(e.Message.Contains(badGtin));
            }
        }

        [Test]
        public void TestOutboundOrderDuplicateGtins()
        {
            onSetUp();
            stockRepository.AddStock(WAREHOUSE_ID, new List<StockAlteration>() { new StockAlteration(productId, 10) });
            OutboundOrderRequestModel outboundOrder = new OutboundOrderRequestModel()
            {
                WarehouseId = WAREHOUSE_ID,
                OrderLines = new List<OrderLine>()
                {
                    new OrderLine()
                    {
                        Gtin = GTIN,
                        Quantity = 1
                    },
                    new OrderLine()
                    {
                        Gtin = GTIN,
                        Quantity = 1
                    }
                }
            };

            try
            {
                outboundOrderController.Post(outboundOrder);
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (ValidationException e)
            {
                Assert.IsTrue(e.Message.Contains(GTIN));
            }
        }
    }
}
