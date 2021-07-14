using ShipIt.Models.DataModels;

namespace ShipItTest.Builders
{
    public class StockBuilder
    {
        private int WarehouseId = 1;
        private int ProductId = 1;
        private int Held = 0;

        public StockBuilder SetWarehouseId(int warehouseId)
        {
            WarehouseId = warehouseId;
            return this;
        }

        public StockBuilder SetProductId(int productId)
        {
            ProductId = productId;
            return this;
        }

        public StockBuilder SetHeld(int held)
        {
            Held = held;
            return this;
        }

        public StockDataModel CreateStock()
        {
            return new StockDataModel()
            {
                WarehouseId = WarehouseId,
                ProductId = ProductId,
                held = Held
            };
        }
    }
}
