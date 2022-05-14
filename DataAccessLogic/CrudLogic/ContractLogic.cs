using DataAccessLogic.DatabaseModels;
using DataAccessLogic.Enums;
using DataAccessLogic.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace DataAccessLogic.CrudLogic
{
    public class ContractLogic : ICrudLogic<Contract>
    {
        private readonly ApplicationContext context;

        public ContractLogic(ApplicationContext context)
        {
            this.context = context;
        }

        public Task Create(Contract model)
        {
            throw new NotImplementedException();
        }

        public Task Delete(Contract model)
        {
            throw new NotImplementedException();
        }

        public Task<List<Contract>> Read(Contract model)
        {
            throw new NotImplementedException();
        }

        public Task Update(Contract model)
        {
            throw new NotImplementedException();
        }
    }
}
