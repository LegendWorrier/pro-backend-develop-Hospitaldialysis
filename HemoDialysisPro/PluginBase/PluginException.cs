using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wasenshi.HemoDialysisPro.PluginBase
{
    [Serializable]
    public class PluginException : Exception
    {
        public string Code { get; set; } = "UNKNOWN";
        public PluginException() { }
        public PluginException(string code, string message) : base(message) { Code = code; }
        public PluginException(string code, string message, Exception inner) : base(message, inner) { Code = code; }
        protected PluginException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
