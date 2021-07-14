namespace ShipIt.Models.ApiModels
{
    public class InboundOrderLine
    {
        public string Gtin { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }

        public InboundOrderLine()
        {
        }
    }
}