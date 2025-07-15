using Prensadao.Application.Models.Request;
using Prensadao.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prensadao.Application.Interfaces
{
    public interface ICustomerService
    {
        Task<List<Customer>> GetCustomers();
        Task<int> AddCustomer(CustomerDto dto);
    }
}
