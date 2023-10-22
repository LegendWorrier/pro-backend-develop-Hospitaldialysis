using System;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Repository
{
    public class TagRepository : Repository<Tag, Guid>, ITagRepository
    {
        public TagRepository(IContextAdapter context) : base(context)
        {
        }
    }
}
