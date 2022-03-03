using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzCodeReview.ClientApi
{
    public class OperationResult
    {
        public bool Success { get; set; }

        public string[] Errors { get; set; }

        public bool Delayed { get; set; }

        public void CheckIfSuccess()
        {
            if (this.Success)
            {
                return;
            }

            throw new BusinessException(this.Errors);
        }

    }

    public class OperationResult<T> : OperationResult
    {
        public T Data { get; set; }

        public new T CheckIfSuccess()
        {
            if (this.Success)
            {
                return this.Data;
            }

            throw new BusinessException(this.Errors);
        }
    }
}
