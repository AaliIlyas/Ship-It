using Npgsql;
using ShipIt.Exceptions;
using ShipIt.Models.DataModels;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ShipIt.Repositories
{
    public interface IProductRepository
    {
        int GetCount();
        ProductDataModel GetProductByGtin(string gtin);
        IEnumerable<ProductDataModel> GetProductsByGtin(List<string> gtins);
        IEnumerable<ProductDataModel> GetProductsByIds(IEnumerable<int> ids);
        ProductDataModel GetProductById(int id);
        void AddProducts(IEnumerable<ProductDataModel> products);
        void DiscontinueProductByGtin(string gtin);
    }

    public class ProductRepository : RepositoryBase, IProductRepository
    {
        public int GetCount()
        {
            string EmployeeCountSQL = "SELECT COUNT(*) FROM gcp";
            return (int)QueryForLong(EmployeeCountSQL);
        }

        public ProductDataModel GetProductByGtin(string gtin)
        {

            string sql = "SELECT p_id, gtin_cd, gcp_cd, gtin_nm, m_g, l_th, ds, min_qt FROM gtin WHERE gtin_cd = @gtin_cd";
            NpgsqlParameter parameter = new NpgsqlParameter("@gtin_cd", gtin);
            return RunSingleGetQuery(sql, reader => new ProductDataModel(reader),
                string.Format("No products found with gtin of value {0}", gtin), parameter);
        }

        public IEnumerable<ProductDataModel> GetProductsByGtin(List<string> gtins)
        {

            string sql = string.Format("SELECT p_id, gtin_cd, gcp_cd, gtin_nm, m_g, l_th, ds, min_qt FROM gtin WHERE gtin_cd IN ('{0}')",
                string.Join("','", gtins));
            return RunGetQuery(sql, reader => new ProductDataModel(reader), "No products found with given gtin ids", null);
        }

        public ProductDataModel GetProductById(int id)
        {
            string sql = "SELECT p_id, gtin_cd, gcp_cd, gtin_nm, m_g, l_th, ds, min_qt FROM gtin WHERE p_id = @p_id";
            NpgsqlParameter parameter = new NpgsqlParameter("@p_id", id);
            string noProductWithIdErrorMessage = string.Format("No products found with id of value {0}", id.ToString());
            return RunSingleGetQuery(sql, reader => new ProductDataModel(reader), noProductWithIdErrorMessage, parameter);
        }

        public IEnumerable<ProductDataModel> GetProductsByIds(IEnumerable<int> ids)
        {
            string sql = string.Format("SELECT p_id, gtin_cd, gcp_cd, gtin_nm, m_g, l_th, ds, min_qt FROM gtin WHERE p_id IN ('{0}')",
                string.Join("','", ids));
            return RunGetQuery(sql, reader => new ProductDataModel(reader), "No products found with given ids", null).ToList();
        }

        public void DiscontinueProductByGtin(string gtin)
        {
            string sql = "UPDATE gtin SET ds = 1 WHERE gtin_cd = @gtin_cd";
            NpgsqlParameter parameter = new NpgsqlParameter("@gtin_cd", gtin);
            string noProductWithGtinErrorMessage =
                string.Format("No products found with gtin of value {0}", gtin.ToString());

            RunSingleQuery(sql, noProductWithGtinErrorMessage, parameter);
        }

        public void AddProducts(IEnumerable<ProductDataModel> products)
        {
            string sql = "INSERT INTO gtin (gtin_cd, gcp_cd, gtin_nm, m_g, l_th, ds, min_qt) VALUES (@gtin_cd, @gcp_cd, @gtin_nm, @m_g, @l_th, @ds, @min_qt)";

            List<NpgsqlParameter[]> parametersList = new List<NpgsqlParameter[]>();
            List<string> gtins = new List<string>();

            foreach (ProductDataModel product in products)
            {
                if (gtins.Contains(product.Gtin))
                {
                    throw new MalformedRequestException(string.Format("Cannot add products with duplicate gtins: {0}",
                        product.Gtin));
                }
                gtins.Add(product.Gtin);
                parametersList.Add(product.GetNpgsqlParameters().ToArray());
            }

            IEnumerable<ProductDataModel> conflicts = TryGetProductsByGtin(gtins);
            if (conflicts.Any())
            {
                throw new MalformedRequestException(string.Format("Cannot add products with existing gtins: {0}",
                    string.Join(", ", conflicts.Select(c => c.Gtin))));
            }

            RunTransaction(sql, parametersList);
        }

        private IEnumerable<ProductDataModel> TryGetProductsByGtin(List<string> gtins)
        {
            try
            {
                List<ProductDataModel> products = GetProductsByGtin(gtins).ToList();
                return products;
            }
            catch (NoSuchEntityException)
            {
                return new List<ProductDataModel>();
            }
        }
    }
}