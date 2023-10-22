using System;
using System.Collections.Generic;

namespace Wasenshi.HemoDialysisPro.Services.BusinessLogic
{
    [Serializable]
    public class SystemBoundException : Exception
    {
        public List<string> Errors { get; set; }

        public SystemBoundException() { }
        public SystemBoundException(List<string> errors) : base("There is multiple errors in system bound checking.") { Errors = errors; }
        public SystemBoundException(string message) : base(message) { }
        public SystemBoundException(string message, Exception inner) : base(message, inner) { }
        protected SystemBoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
