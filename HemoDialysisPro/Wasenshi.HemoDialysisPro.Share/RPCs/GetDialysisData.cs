using Wasenshi.HemoDialysisPro.Models;

namespace Wasenshi.HemoDialysisPro.Share.RPCs
{
    public class GetDialysisData
    {
        public string ConnectionId { get; set; }
    }

    public class GetDialysisDataResponse
    {
        public DialysisRecord Data { get; set; }
    }
}
