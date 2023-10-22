using System;
using System.Globalization;

namespace Wasenshi.HemoDialysisPro.Utils
{
    public class AppException : Exception
    {
        public string Code { get; set; }
        public AppException(string code) : base() { Code = code; }

        public AppException(string code, string message) : base(message) { Code = code; }

        public AppException(string message, params object[] args)
            : base(string.Format(CultureInfo.InvariantCulture, message, args))
        {
        }

        public override string ToString()
        {
            return $"Code: {Code} | " + base.ToString();
        }
    }
}
