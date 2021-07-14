using System.Text;

namespace ShipIt.Models.ApiModels
{
    public class OrderLine
    {
        public string Gtin { get; set; }
        public int Quantity { get; set; }

        public override string ToString()
        {
            return new StringBuilder()
                .AppendFormat("gtin: {0}, ", Gtin)
                .AppendFormat("quantity: {0}", Quantity)
                .ToString();
        }
    }
}