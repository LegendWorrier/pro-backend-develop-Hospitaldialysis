using MessagePack;
using Wasenshi.HemoDialysisPro.Models;

namespace Wasenshi.HemoDialysisPro.Report.Models
{
    public class NurseRecordData : NurseRecord
    {
        [MessagePack.IgnoreMember]
        private readonly IUserResolver userResolver;

        public NurseRecordData(IUserResolver userResolver)
        {
            this.userResolver = userResolver;
        }

        [SerializationConstructor]
        public NurseRecordData()
        {
            // for serialization
        }

        public string CreatorName => userResolver.GetName(CreatedBy);
        public string CreatorEmployeeId => userResolver.GetEmployeeId(CreatedBy);
    }
}
