using MessagePack;
using Wasenshi.HemoDialysisPro.Models;

namespace Wasenshi.HemoDialysisPro.Report.Models
{
    public class DoctorRecordData : DoctorRecord
    {
        [MessagePack.IgnoreMember]
        private readonly IUserResolver userResolver;

        public DoctorRecordData(IUserResolver userResolver)
        {
            this.userResolver = userResolver;
        }

        [SerializationConstructor]
        public DoctorRecordData()
        {
            // for serialization
        }

        public string CreatorName => userResolver.GetName(CreatedBy);
    }
}
