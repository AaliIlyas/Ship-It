using ShipIt.Controllers;
using ShipIt.Models.ApiModels;
using ShipIt.Models.DataModels;
using ShipIt.Parsers;
using System.Collections.Generic;

namespace ShipItTest.Builders
{
    public class ProductBuilder
    {
        private int Id = 1;
        private string Gtin = "0099346374235";
        private string Gcp = "0000346";
        private string Name = "2 Count 1 T30 Torx Bit Tips TX";
        private float Weight = 300.0f;
        private int LowerThreshold = 322;
        private int Discontinued = 0;
        private int MinimumOrderQuantity = 108;

        public ProductBuilder setId(int id)
        {
            Id = id;
            return this;
        }

        public ProductBuilder setGtin(string gtin)
        {
            Gtin = gtin;
            return this;
        }

        public ProductBuilder setGcp(string gcp)
        {
            Gcp = gcp;
            return this;
        }

        public ProductBuilder setName(string name)
        {
            Name = name;
            return this;
        }

        public ProductBuilder setWeight(float weight)
        {
            Weight = weight;
            return this;
        }

        public ProductBuilder setLowerThreshold(int lowerThreshold)
        {
            LowerThreshold = lowerThreshold;
            return this;
        }

        public ProductBuilder setDiscontinued(int discontinued)
        {
            Discontinued = discontinued;
            return this;
        }

        public ProductBuilder setMinimumOrderQuantity(int minimumOrderQuantity)
        {
            MinimumOrderQuantity = minimumOrderQuantity;
            return this;
        }

        public ProductDataModel CreateProductDatabaseModel()
        {
            return new ProductDataModel()
            {
                Discontinued = Discontinued,
                Gcp = Gcp,
                Gtin = Gtin,
                Id = Id,
                LowerThreshold = LowerThreshold,
                MinimumOrderQuantity = MinimumOrderQuantity,
                Name = Name,
                Weight = Weight
            };
        }

        public Product CreateProduct()
        {
            return new Product()
            {
                Discontinued = Discontinued == 1,
                Gcp = Gcp,
                Gtin = Gtin,
                Id = Id,
                LowerThreshold = LowerThreshold,
                MinimumOrderQuantity = MinimumOrderQuantity,
                Name = Name,
                Weight = Weight
            };
        }

        public ProductsRequestModel CreateProductRequest()
        {
            return new ProductsRequestModel()
            {
                Products = new List<ProductRequestModel>()
                {
                    new ProductRequestModel()
                    {
                        Discontinued = Discontinued == 1 ? "true" : "false",
                        Gcp = Gcp,
                        Gtin = Gtin,
                        LowerThreshold = LowerThreshold.ToString(),
                        MinimumOrderQuantity = MinimumOrderQuantity.ToString(),
                        Name = Name,
                        Weight = Weight.ToString()
                    }
                }
            };
        }

        public ProductsRequestModel CreateDuplicateProductRequest()
        {
            return new ProductsRequestModel()
            {
                Products = new List<ProductRequestModel>()
                {
                    new ProductRequestModel()
                    {
                        Discontinued = Discontinued == 1 ? "true" : "false",
                        Gcp = Gcp,
                        Gtin = Gtin,
                        LowerThreshold = LowerThreshold.ToString(),
                        MinimumOrderQuantity = MinimumOrderQuantity.ToString(),
                        Name = Name,
                        Weight = Weight.ToString()
                    },
                    new ProductRequestModel()
                    {
                        Discontinued = Discontinued == 1 ? "true" : "false",
                        Gcp = Gcp,
                        Gtin = Gtin,
                        LowerThreshold = LowerThreshold.ToString(),
                        MinimumOrderQuantity = MinimumOrderQuantity.ToString(),
                        Name = Name,
                        Weight = Weight.ToString()
                    }
                }
            };
        }
    }
}