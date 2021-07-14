﻿using ShipIt.Exceptions;
using ShipIt.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace ShipIt.Models.ApiModels
{
    public class StockAlteration
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public double TotalWeight {get; set;}
        public StockAlteration(int productId, int quantity)
        {
            var _product = new ProductRepository();
            ProductId = productId;
            Quantity = quantity;
            TotalWeight = _product.GetProductById(productId).Weight * Quantity;

            if (quantity < 0)
            {
                throw new MalformedRequestException("Alteration must be positive");
            }
        }
    }
}