using ShipIt.Models.ApiModels;
using ShipIt.Models.DataModels;
using System.Collections.Generic;

namespace ShipItTest.Builders
{
    public class EmployeeBuilder
    {
        private string Name = "Gissell Sadeem";
        private int WarehouseId = 1;
        private EmployeeRole Role = EmployeeRole.OPERATIONS_MANAGER;
        private string Ext = "73996";

        public EmployeeBuilder setName(string name)
        {
            Name = name;
            return this;
        }

        public EmployeeBuilder setWarehouseId(int warehouseId)
        {
            WarehouseId = warehouseId;
            return this;
        }

        public EmployeeBuilder setRole(EmployeeRole role)
        {
            Role = role;
            return this;
        }

        public EmployeeBuilder setExt(string ext)
        {
            Ext = ext;
            return this;
        }

        public EmployeeDataModel CreateEmployeeDataModel()
        {
            return new EmployeeDataModel()
            {
                Name = Name,
                WarehouseId = WarehouseId,
                Role = Role.ToString(),
                Ext = Ext
            };
        }

        public Employee CreateEmployee()
        {
            return new Employee()
            {
                Name = Name,
                WarehouseId = WarehouseId,
                role = Role,
                ext = Ext
            };
        }

        public AddEmployeesRequest CreateAddEmployeesRequest()
        {
            return new AddEmployeesRequest()
            {
                Employees = new List<Employee>()
                {
                    new Employee()
                    {
                        Name = Name,
                        WarehouseId = WarehouseId,
                        role = Role,
                        ext = Ext
                    }
                }
            };
        }
    }
}
