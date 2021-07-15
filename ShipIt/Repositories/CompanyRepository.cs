using Npgsql;
using ShipIt.Models.ApiModels;
using ShipIt.Models.DataModels;
using System.Collections.Generic;
using System.Linq;

namespace ShipIt.Repositories
{
    public interface ICompanyRepository
    {
        int GetCount();
        CompanyDataModel GetCompanyByGcp(string gcp);
        void AddCompanies(IEnumerable<Company> companies);
        IEnumerable<CompanyDataModel> GetCompaniesByGcps(List<string> gcp);
    }

    public class CompanyRepository : RepositoryBase, ICompanyRepository
    {
        public int GetCount()
        {
            string CompanyCountSQL = "SELECT COUNT(*) FROM gcp";
            return (int)QueryForLong(CompanyCountSQL);
        }

        public CompanyDataModel GetCompanyByGcp(string gcp)
        {
            string sql =
                "SELECT gcp_cd, gln_nm, gln_addr_02, gln_addr_03, gln_addr_04, gln_addr_postalcode, gln_addr_city, contact_tel, contact_mail " +
                "FROM gcp " +
                "WHERE gcp_cd = @gcp_cd";
            NpgsqlParameter parameter = new NpgsqlParameter("@gcp_cd", gcp);
            string noProductWithIdErrorMessage = string.Format("No companies found with gcp: {0}", gcp);
            return base.RunSingleGetQuery(sql, reader => new CompanyDataModel(reader), noProductWithIdErrorMessage, parameter);
        }

        public List<CompanyDataModel> GetCompaniesByGcps(List<string> gcp)
        {
            string sql = string.Format("SELECT gcp_cd, gln_nm, gln_addr_02, gln_addr_03, gln_addr_04, gln_addr_postalcode, gln_addr_city, contact_tel, contact_mail " +
                "FROM gcp " +
                "WHERE gcp_cd = @gcp_cd IN ('{0}')",
                string.Join("','", gcp));
            return RunGetQuery(sql, reader => new CompanyDataModel(reader), "No company found with given gtin ids", null).ToList();
        }

        public void AddCompanies(IEnumerable<Company> companies)
        {
            string sql =
                "INSERT INTO gcp (gcp_cd, gln_nm, gln_addr_02, gln_addr_03, gln_addr_04, gln_addr_postalcode, gln_addr_city, contact_tel, contact_mail) " +
                "VALUES (@gcp_cd, @gln_nm, @gln_addr_02, @gln_addr_03, @gln_addr_04, @gln_addr_postalcode, @gln_addr_city, @contact_tel, @contact_mail)";

            List<NpgsqlParameter[]> parametersList = new List<NpgsqlParameter[]>();
            foreach (Company company in companies)
            {
                CompanyDataModel companyDataModel = new CompanyDataModel(company);
                parametersList.Add(companyDataModel.GetNpgsqlParameters().ToArray());
            }

            base.RunTransaction(sql, parametersList);
        }
    }

}