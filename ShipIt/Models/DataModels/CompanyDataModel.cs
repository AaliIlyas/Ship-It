using ShipIt.Models.ApiModels;
using System.Data;

namespace ShipIt.Models.DataModels
{
    public class CompanyDataModel : DataModel
    {
        [DatabaseColumnName("gcp_cd")]
        public string Gcp { get; set; }
        [DatabaseColumnName("gln_nm")]
        public string Name { get; set; }
        [DatabaseColumnName("gln_addr_02")]
        public string Addr2 { get; set; }
        [DatabaseColumnName("gln_addr_03")]
        public string Addr3 { get; set; }
        [DatabaseColumnName("gln_addr_04")]
        public string Addr4 { get; set; }
        [DatabaseColumnName("gln_addr_postalcode")]
        public string PostalCode { get; set; }
        [DatabaseColumnName("gln_addr_city")]
        public string City { get; set; }
        [DatabaseColumnName("contact_tel")]
        public string Tel { get; set; }
        [DatabaseColumnName("contact_mail")]
        public string Mail { get; set; }

        public CompanyDataModel(IDataReader dataReader) : base(dataReader)
        {
        }

        public CompanyDataModel(Company company)
        {
            Gcp = company.Gcp;
            Name = company.Name;
            Addr2 = company.Addr2;
            Addr3 = company.Addr3;
            Addr4 = company.Addr4;
            PostalCode = company.PostalCode;
            City = company.City;
            Tel = company.Tel;
            Mail = company.Mail;
        }
    }
}