using NUnit.Framework;
using ShipIt.Controllers;
using ShipIt.Exceptions;
using ShipIt.Models.ApiModels;
using ShipIt.Repositories;
using ShipItTest.Builders;
using System.Collections.Generic;

namespace ShipItTest
{
    public class CompanyControllerTests : AbstractBaseTest
    {
        private readonly CompanyController companyController = new CompanyController(new CompanyRepository());
        private readonly CompanyRepository companyRepository = new CompanyRepository();

        private const string GCP = "0000346";

        [Test]
        public void TestRoundtripCompanyRepository()
        {
            onSetUp();
            Company company = new CompanyBuilder().CreateCompany();
            companyRepository.AddCompanies(new List<Company>() { company });
            Assert.AreEqual(companyRepository.GetCompany(company.Gcp).Name, company.Name);
        }

        [Test]
        public void TestGetCompanyByGcp()
        {
            onSetUp();
            CompanyBuilder companyBuilder = new CompanyBuilder().setGcp(GCP);
            companyRepository.AddCompanies(new List<Company>() { companyBuilder.CreateCompany() });
            CompanyResponse result = companyController.Get(GCP);

            Company correctCompany = companyBuilder.CreateCompany();
            Assert.IsTrue(CompaniesAreEqual(correctCompany, result.Company));
            Assert.IsTrue(result.Success);
        }

        [Test]
        public void TestGetNonExistentCompany()
        {
            onSetUp();
            try
            {
                companyController.Get(GCP);
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (NoSuchEntityException e)
            {
                Assert.IsTrue(e.Message.Contains(GCP));
            }
        }

        [Test]
        public void TestAddCompanies()
        {
            onSetUp();
            CompanyBuilder companyBuilder = new CompanyBuilder().setGcp(GCP);
            AddCompaniesRequest addCompaniesRequest = companyBuilder.CreateAddCompaniesRequest();

            Response response = companyController.Post(addCompaniesRequest);
            ShipIt.Models.DataModels.CompanyDataModel databaseCompany = companyRepository.GetCompany(GCP);
            Company correctCompany = companyBuilder.CreateCompany();

            Assert.IsTrue(response.Success);
            Assert.IsTrue(CompaniesAreEqual(new Company(databaseCompany), correctCompany));
        }

        private bool CompaniesAreEqual(Company A, Company B)
        {
            return A.Gcp == B.Gcp
                   && A.Name == B.Name
                   && A.Addr2 == B.Addr2
                   && A.Addr3 == B.Addr3
                   && A.Addr4 == B.Addr4
                   && A.PostalCode == B.PostalCode
                   && A.City == B.City
                   && A.Tel == B.Tel
                   && A.Mail == B.Mail;
        }
    }
}
