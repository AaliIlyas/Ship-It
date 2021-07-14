using NUnit.Framework;
using ShipIt.Controllers;
using ShipIt.Exceptions;
using ShipIt.Models.ApiModels;
using ShipIt.Repositories;
using ShipItTest.Builders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShipItTest
{
    public class EmployeeControllerTests : AbstractBaseTest
    {
        private readonly EmployeeController employeeController = new EmployeeController(new EmployeeRepository());
        private readonly EmployeeRepository employeeRepository = new EmployeeRepository();

        private const string NAME = "Gissell Sadeem";
        private const int WAREHOUSE_ID = 1;

        [Test]
        public void TestRoundtripEmployeeRepository()
        {
            onSetUp();
            Employee employee = new EmployeeBuilder().CreateEmployee();
            employeeRepository.AddEmployees(new List<Employee>() { employee });
            Assert.AreEqual(employeeRepository.GetEmployeeByName(employee.Name).Name, employee.Name);
            Assert.AreEqual(employeeRepository.GetEmployeeByName(employee.Name).Ext, employee.ext);
            Assert.AreEqual(employeeRepository.GetEmployeeByName(employee.Name).WarehouseId, employee.WarehouseId);
        }

        [Test]
        public void TestGetEmployeeByName()
        {
            onSetUp();
            EmployeeBuilder employeeBuilder = new EmployeeBuilder().setName(NAME);
            employeeRepository.AddEmployees(new List<Employee>() { employeeBuilder.CreateEmployee() });
            EmployeeResponse result = employeeController.Get(NAME);

            Employee correctEmployee = employeeBuilder.CreateEmployee();
            Assert.IsTrue(EmployeesAreEqual(correctEmployee, result.Employees.First()));
            Assert.IsTrue(result.Success);
        }

        [Test]
        public void TestGetEmployeesByWarehouseId()
        {
            onSetUp();
            EmployeeBuilder employeeBuilderA = new EmployeeBuilder().setWarehouseId(WAREHOUSE_ID).setName("A");
            EmployeeBuilder employeeBuilderB = new EmployeeBuilder().setWarehouseId(WAREHOUSE_ID).setName("B");
            employeeRepository.AddEmployees(new List<Employee>() { employeeBuilderA.CreateEmployee(), employeeBuilderB.CreateEmployee() });
            List<Employee> result = employeeController.Get(WAREHOUSE_ID).Employees.ToList();

            Employee correctEmployeeA = employeeBuilderA.CreateEmployee();
            Employee correctEmployeeB = employeeBuilderB.CreateEmployee();

            Assert.IsTrue(result.Count == 2);
            Assert.IsTrue(EmployeesAreEqual(correctEmployeeA, result.First()));
            Assert.IsTrue(EmployeesAreEqual(correctEmployeeB, result.Last()));
        }

        [Test]
        public void TestGetNonExistentEmployee()
        {
            onSetUp();
            try
            {
                employeeController.Get(NAME);
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (NoSuchEntityException e)
            {
                Assert.IsTrue(e.Message.Contains(NAME));
            }
        }

        [Test]
        public void TestGetEmployeeInNonexistentWarehouse()
        {
            onSetUp();
            try
            {
                List<Employee> employees = employeeController.Get(WAREHOUSE_ID).Employees.ToList();
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (NoSuchEntityException e)
            {
                Assert.IsTrue(e.Message.Contains(WAREHOUSE_ID.ToString()));
            }
        }

        [Test]
        public void TestAddEmployees()
        {
            onSetUp();
            EmployeeBuilder employeeBuilder = new EmployeeBuilder().setName(NAME);
            AddEmployeesRequest addEmployeesRequest = employeeBuilder.CreateAddEmployeesRequest();

            Response response = employeeController.Post(addEmployeesRequest);
            ShipIt.Models.DataModels.EmployeeDataModel databaseEmployee = employeeRepository.GetEmployeeByName(NAME);
            Employee correctDatabaseEmploye = employeeBuilder.CreateEmployee();

            Assert.IsTrue(response.Success);
            Assert.IsTrue(EmployeesAreEqual(new Employee(databaseEmployee), correctDatabaseEmploye));
        }

        [Test]
        public void TestDeleteEmployees()
        {
            onSetUp();
            EmployeeBuilder employeeBuilder = new EmployeeBuilder().setName(NAME);
            employeeRepository.AddEmployees(new List<Employee>() { employeeBuilder.CreateEmployee() });

            RemoveEmployeeRequest removeEmployeeRequest = new RemoveEmployeeRequest() { Name = NAME };
            employeeController.Delete(removeEmployeeRequest);

            try
            {
                employeeController.Get(NAME);
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (NoSuchEntityException e)
            {
                Assert.IsTrue(e.Message.Contains(NAME));
            }
        }

        [Test]
        public void TestDeleteNonexistentEmployee()
        {
            onSetUp();
            RemoveEmployeeRequest removeEmployeeRequest = new RemoveEmployeeRequest() { Name = NAME };

            try
            {
                employeeController.Delete(removeEmployeeRequest);
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (NoSuchEntityException e)
            {
                Assert.IsTrue(e.Message.Contains(NAME));
            }
        }

        [Test]
        public void TestAddDuplicateEmployee()
        {
            onSetUp();
            EmployeeBuilder employeeBuilder = new EmployeeBuilder().setName(NAME);
            employeeRepository.AddEmployees(new List<Employee>() { employeeBuilder.CreateEmployee() });
            AddEmployeesRequest addEmployeesRequest = employeeBuilder.CreateAddEmployeesRequest();

            try
            {
                employeeController.Post(addEmployeesRequest);
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (Exception)
            {
                Assert.IsTrue(true);
            }
        }

        private bool EmployeesAreEqual(Employee A, Employee B)
        {
            return A.WarehouseId == B.WarehouseId
                   && A.Name == B.Name
                   && A.role == B.role
                   && A.ext == B.ext;
        }
    }
}
