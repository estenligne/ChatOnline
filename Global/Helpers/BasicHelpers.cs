using System;
using System.Collections.Generic;
using System.Text;

namespace Global.Helpers
{
    public static class BasicHelpers
    {
        public static string GetShortBody(string body, int maximumLength)
        {
            return (body != null && body.Length > maximumLength) ? body.Substring(0, maximumLength) : body;
        }
    }
}
