﻿using Microsoft.AspNetCore.Mvc;
using ShipIt.Exceptions;
using ShipIt.Models.ApiModels;
using ShipIt.Models.DataModels;
using ShipIt.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShipIt.Controllers
{
    [Route("orders/inbound")]
    public class InboundOrderController : ControllerBase
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        private readonly IEmployeeRepository _employeeRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IProductRepository _productRepository;
        private readonly IStockRepository _stockRepository;

        public InboundOrderController(IEmployeeRepository employeeRepository, ICompanyRepository companyRepository, IProductRepository productRepository, IStockRepository stockRepository)
        {
            _employeeRepository = employeeRepository;
            _stockRepository = stockRepository;
            _companyRepository = companyRepository;
            _productRepository = productRepository;
        }

        [HttpGet("{warehouseId}")]
        public InboundOrderResponse Get([FromRoute] int warehouseId)
        {
            Log.Info("orderIn for warehouseId: " + warehouseId);

            var operationsManager = _employeeRepository.GetOperationsManager(warehouseId);

            Log.Debug(string.Format("Found operations manager: {0}", operationsManager));

            var allStock = _stockRepository.GetStockByWarehouseId(warehouseId);

            var orderlinesByCompany = new Dictionary<CompanyDataModel, List<InboundOrderLine>>();
            foreach (StockDataModel stock in allStock)
            {
                var product = _productRepository.GetProductById(stock.ProductId);
                if (stock.held < product.LowerThreshold && product.Discontinued != 1)
                {
                    var company = _companyRepository.GetCompany(product.Gcp);

                    int orderQuantity = Math.Max(product.LowerThreshold * 3 - stock.held, product.MinimumOrderQuantity);

                    if (!orderlinesByCompany.ContainsKey(company))
                    {
                        orderlinesByCompany.Add(company, new List<InboundOrderLine>());
                    }

                    //Select(Products.where wid==warehouseId).Include(Employees.where wid==warehouseId)

                    orderlinesByCompany[company].Add(
                        new InboundOrderLine()
                        {
                            Gtin = product.Gtin,
                            Name = product.Name,
                            Quantity = orderQuantity
                        });
                }
            }

            Log.Debug(string.Format("Constructed order lines: {0}", orderlinesByCompany));

            var orderSegments = orderlinesByCompany.Select(ol => new OrderSegment()
            {
                OrderLines = ol.Value,
                Company = new Company(ol.Key)
            });

            Log.Info("Constructed inbound order");

            return new InboundOrderResponse()
            {
                OperationsManager = new Employee(operationsManager),
                WarehouseId = warehouseId,
                OrderSegments = orderSegments
            };
        }

        [HttpPost("")]
        public void Post([FromBody] InboundManifestRequestModel requestModel)
        {
            Log.Info("Processing manifest: " + requestModel);

            List<string> gtins = new List<string>();

            foreach (OrderLine orderLine in requestModel.OrderLines)
            {
                if (gtins.Contains(orderLine.Gtin))
                {
                    throw new ValidationException(string.Format("Manifest contains duplicate product gtin: {0}", orderLine.Gtin));
                }
                gtins.Add(orderLine.Gtin);
            }

            IEnumerable<ProductDataModel> productDataModels = _productRepository.GetProductsByGtin(gtins);
            Dictionary<string, Product> products = productDataModels.ToDictionary(p => p.Gtin, p => new Product(p));

            Log.Debug(string.Format("Retrieved products to verify manifest: {0}", products));

            List<StockAlteration> lineItems = new List<StockAlteration>();
            List<string> errors = new List<string>();

            foreach (OrderLine orderLine in requestModel.OrderLines)
            {
                if (!products.ContainsKey(orderLine.Gtin))
                {
                    errors.Add(string.Format("Unknown product gtin: {0}", orderLine.Gtin));
                    continue;
                }

                Product product = products[orderLine.Gtin];
                if (!product.Gcp.Equals(requestModel.Gcp))
                {
                    errors.Add(string.Format("Manifest GCP ({0}) doesn't match Product GCP ({1})",
                        requestModel.Gcp, product.Gcp));
                }
                else
                {
                    lineItems.Add(new StockAlteration(product.Id, orderLine.Quantity));
                }
            }

            if (errors.Count() > 0)
            {
                Log.Debug(string.Format("Found errors with inbound manifest: {0}", errors));
                throw new ValidationException(string.Format("Found inconsistencies in the inbound manifest: {0}", string.Join("; ", errors)));
            }

            Log.Debug(string.Format("Increasing stock levels with manifest: {0}", requestModel));
            _stockRepository.AddStock(requestModel.WarehouseId, lineItems);
            Log.Info("Stock levels increased");
        }
    }
}
