namespace ShipIt.Models.ApiModels
{
    public class CompanyResponse : Response
    {
        public Company Company { get; set; }
        public CompanyResponse(Company company)
        {
            Company = company;
            Success = true;
        }

        public CompanyResponse() { }
    }
}