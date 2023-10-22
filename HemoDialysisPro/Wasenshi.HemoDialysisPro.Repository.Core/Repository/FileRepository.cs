using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Repository
{
    public class FileRepository : Repository<FileEntry, string>, IFileRepository
    {
        public FileRepository(IContextAdapter context) : base(context)
        {
        }

    }
}
