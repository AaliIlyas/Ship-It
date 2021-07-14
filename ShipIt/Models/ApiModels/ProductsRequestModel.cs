using ShipIt.Parsers;
using System.Collections.Generic;

namespace ShipIt.Controllers
{
    public class ProductsRequestModel
    {
        public IEnumerable<ProductRequestModel> Products { get; set; }
    }
}