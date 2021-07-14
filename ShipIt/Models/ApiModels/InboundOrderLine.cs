namespace ShipIt.Models.ApiModels
{
    public class InboundOrderLine
    {
        public string gtin { get; set; }
        public string name { get; set; }
        public int quantity { get; set; }

        public InboundOrderLine()
        {
        }
    }
}