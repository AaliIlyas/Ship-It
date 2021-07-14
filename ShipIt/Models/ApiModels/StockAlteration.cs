using ShipIt.Exceptions;
using ShipIt.Repositories;


namespace ShipIt.Models.ApiModels
{
    public class StockAlteration
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public double StockAlterationWeight { get; set; }
        public StockAlteration(int productId, int quantity)
        {
            ProductRepository _product = new ProductRepository();
            ProductId = productId;
            Quantity = quantity;
            StockAlterationWeight = _product.GetProductById(productId).Weight * Quantity;

            if (quantity < 0)
            {
                throw new MalformedRequestException("Alteration must be positive");
            }
        }
    }
}