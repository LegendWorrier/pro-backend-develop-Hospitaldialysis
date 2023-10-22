using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wasenshi.HemoDialysisPro.Utils
{
    public static class Extension
    {
        public static bool IsMultipartContentType(this HttpRequest request)
        {
            return request.HasFormContentType;
        }
    }
}
