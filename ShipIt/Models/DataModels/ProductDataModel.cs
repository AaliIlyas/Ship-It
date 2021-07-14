using Npgsql;
using ShipIt.Models.ApiModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ShipIt.Models.DataModels
{
    public class DatabaseColumnName : Attribute
    {
        public string Name { get; set; }

        public DatabaseColumnName(string name)
        {
            Name = name;
        }
    }


    public abstract class DataModel
    {
        protected DataModel()
        {
        }

        public DataModel(IDataReader dataReader)
        {
            Type type = GetType();
            System.Reflection.PropertyInfo[] properties = type.GetProperties();

            foreach (System.Reflection.PropertyInfo property in properties)
            {
                DatabaseColumnName attribute = (DatabaseColumnName)property.GetCustomAttributes(typeof(DatabaseColumnName), false).First();
                property.SetValue(this, dataReader[attribute.Name], null);
            }
        }

        public IEnumerable<NpgsqlParameter> GetNpgsqlParameters()
        {
            Type type = GetType();
            System.Reflection.PropertyInfo[] properties = type.GetProperties();
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            foreach (System.Reflection.PropertyInfo property in properties)
            {
                DatabaseColumnName attribute = (DatabaseColumnName)property.GetCustomAttributes(typeof(DatabaseColumnName), false).First();
                parameters.Add(new NpgsqlParameter("@" + attribute.Name, property.GetValue(this, null)));
            }

            return parameters;
        }
    }

    public class ProductDataModel : DataModel
    {
        [DatabaseColumnName("p_id")]
        public int Id { get; set; }

        [DatabaseColumnName("gtin_cd")]
        public string Gtin { get; set; }

        [DatabaseColumnName("gcp_cd")]
        public string Gcp { get; set; }

        [DatabaseColumnName("gtin_nm")]
        public string Name { get; set; }

        [DatabaseColumnName("m_g")]
        public double Weight { get; set; }

        [DatabaseColumnName("l_th")]
        public int LowerThreshold { get; set; }

        [DatabaseColumnName("ds")]
        public int Discontinued { get; set; }

        [DatabaseColumnName("min_qt")]
        public int MinimumOrderQuantity { get; set; }

        public ProductDataModel(IDataReader dataReader) : base(dataReader)
        { }

        public ProductDataModel()
        { }

        public ProductDataModel(Product apiModel)
        {
            Id = apiModel.Id;
            Gtin = apiModel.Gtin;
            Gcp = apiModel.Gcp;
            Name = apiModel.Name;
            Weight = apiModel.Weight;
            LowerThreshold = apiModel.LowerThreshold;
            Discontinued = apiModel.Discontinued ? 1 : 0;
            MinimumOrderQuantity = apiModel.MinimumOrderQuantity;
        }
    }

}