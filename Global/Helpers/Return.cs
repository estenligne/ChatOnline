using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Global.Helpers
{
    public class Return<T>
    {
        public readonly T value;
        public readonly HttpStatusCode code;
        public readonly string message;
        public readonly Exception exception;

        public Return(T value)
        {
            this.value = value;
        }

        public Return(HttpStatusCode code, string message)
        {
            this.code = code;
            this.message = message;
        }

        public Return(Exception exception)
        {
            this.exception = exception;
        }

        public bool Failure()
        {
            return value == null || exception != null;
        }
    }
}
