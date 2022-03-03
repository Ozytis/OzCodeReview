using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzCodeReview.ClientApi.Models
{
    public enum ReviewType
    {
        NotSet = 0,
        Comment = 10,
        ShouldFix = 20,
        MustFix = 30
    }
}
