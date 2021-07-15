using Microsoft.AspNetCore.Mvc;
using ShipIt.Exceptions;
using ShipIt.Models.ApiModels;
using ShipIt.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShipIt.Controllers
{
    public class OutBoundOrder
    {
        public int Trucks { get; set; }
    }

    [Route("orders/outbound")]
    public class OutboundOrderController : ControllerBase
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        private readonly IStockRepository _stockRepository;
        private readonly IProductRepository _productRepository;

        public OutboundOrderController(IStockRepository stockRepository, IProductRepository productRepository)
        {
            _stockRepository = stockRepository;
            _productRepository = productRepository;
        }

        [HttpPost("")]
        public OutBoundOrder Post([FromBody] OutboundOrderRequestModel request)
        {
            Log.Info(string.Format("Processing outbound order: {0}", request));

            List<string> gtins = new List<string>();
            foreach (OrderLine orderLine in request.OrderLines)
            {
                if (gtins.Contains(orderLine.Gtin))
                {
                    throw new ValidationException(string.Format("Outbound order request contains duplicate product gtin: {0}", orderLine.Gtin));
                }
                gtins.Add(orderLine.Gtin);
            }

            IEnumerable<Models.DataModels.ProductDataModel> productDataModels = _productRepository.GetProductsByGtin(gtins);
            Dictionary<string, Product> products = productDataModels.ToDictionary(p => p.Gtin, p => new Product(p));

            List<StockAlteration> lineItems = new List<StockAlteration>();
            List<int> productIds = new List<int>();
            List<string> errors = new List<string>();

            foreach (OrderLine orderLine in request.OrderLines)
            {
                if (!products.ContainsKey(orderLine.Gtin))
                {
                    errors.Add(string.Format("Unknown product gtin: {0}", orderLine.Gtin));
                }
                else
                {
                    Product product = products[orderLine.Gtin];
                    lineItems.Add(new StockAlteration(product.Id, orderLine.Quantity));
                    productIds.Add(product.Id);
                }
            }

            if (errors.Count > 0)
            {
                throw new NoSuchEntityException(string.Join("; ", errors));
            }

            Dictionary<int, Models.DataModels.StockDataModel> stock = _stockRepository.GetStockByWarehouseAndProductIds(request.WarehouseId, productIds);

            List<OrderLine> orderLines = request.OrderLines.ToList();
            errors = new List<string>();

            for (int i = 0; i < lineItems.Count; i++)
            {
                StockAlteration lineItem = lineItems[i];
                OrderLine orderLine = orderLines[i];

                if (!stock.ContainsKey(lineItem.ProductId))
                {
                    errors.Add(string.Format("Product: {0}, no stock held", orderLine.Gtin));
                    continue;
                }

                Models.DataModels.StockDataModel item = stock[lineItem.ProductId];
                if (lineItem.Quantity > item.held)
                {
                    errors.Add(
                        string.Format("Product: {0}, stock held: {1}, stock to remove: {2}", orderLine.Gtin, item.held,
                            lineItem.Quantity));
                }
            }

            if (errors.Count > 0)
            {
                throw new InsufficientStockException(string.Join("; ", errors));
            }

            _stockRepository.RemoveStock(request.WarehouseId, lineItems);

            double totalWeight = 0;

            foreach (StockAlteration item in lineItems)
            {
                totalWeight += item.StockAlterationWeight;
            }

            var trucks = Convert.ToInt32(Math.Ceiling(totalWeight / 2000));

            return new OutBoundOrder()
            {
                Trucks = Convert.ToInt32(Math.Ceiling(totalWeight / 2000))
            };
        }
    }
}