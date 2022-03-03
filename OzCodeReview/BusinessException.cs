using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzCodeReview
{
    public class BusinessException : Exception
    {
        public BusinessException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }

        public BusinessException(string property, string message)
            : this(message)
        {
            this.Properties = new[] { property };
            this.Messages = new[] { message };
        }

        public BusinessException(string[] messages)
           : this(null, messages)
        {
        }

        public BusinessException(string[] properties, string[] messages)
       : this(string.Join(", ", messages))
        {
            this.Properties = properties;
            this.Messages = messages;
        }

        public string[] Properties { get; set; }

        public string[] Messages { get; set; }
    }
}
