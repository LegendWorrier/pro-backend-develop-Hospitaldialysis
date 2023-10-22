using MessagePack;
using Wasenshi.HemoDialysisPro.Models;

namespace Wasenshi.HemoDialysisPro.Report.Models
{
    public class ProgressNoteData : ProgressNote
    {
        [MessagePack.IgnoreMember]
        private readonly IUserResolver userResolver;

        public ProgressNoteData(IUserResolver userResolver)
        {
            this.userResolver = userResolver;
        }

        [SerializationConstructor]
        public ProgressNoteData()
        {
            // for serialization
        }

        public string CreatorName => userResolver.GetName(CreatedBy);
        public string CreatorEmployeeId => userResolver.GetEmployeeId(CreatedBy);
    }
}
