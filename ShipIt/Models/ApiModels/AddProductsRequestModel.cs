using System.Collections.Generic;
using System.Text;

namespace ShipIt.Models.ApiModels
{
    public class InboundManifestRequestModel
    {
        public int WarehouseId { get; set; }
        public string Gcp { get; set; }
        public IEnumerable<OrderLine> OrderLines { get; set; }

        public override string ToString()
        {
            return new StringBuilder()
                .AppendFormat("warehouseId: {0}, ", WarehouseId)
                .AppendFormat("gcp: {0}, ", Gcp)
                .AppendFormat("orderLines: {0}, ", OrderLines)
                .ToString();
        }
    }
}