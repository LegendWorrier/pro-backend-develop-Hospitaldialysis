using System;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Repository
{
    public class AvShuntRepository : Repository<AVShunt, Guid>, IAvShuntRepository
    {
        public AvShuntRepository(IContextAdapter context) : base(context)
        {
        }

    }
}
