using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Tagging;

using OzCodeReview.ClientApi.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzCodeReview.Reviews
{
    public class ReviewErrorTag : IErrorTag, IReviewTag
    {
        public Review Review { get; set; }

        public UserModel Recipient { get; set; }

        public UserModel Commentator { get; set; }

        public string ErrorType
        {
            get
            {
                if (this.Review == null)
                {
                    return PredefinedErrorTypeNames.OtherError;
                }

                switch (this.Review.Type)
                {
                    case ReviewType.NotSet:
                        return PredefinedErrorTypeNames.HintedSuggestion;
                    case ReviewType.Comment:
                        return PredefinedErrorTypeNames.HintedSuggestion;                        
                    case ReviewType.ShouldFix:
                        return PredefinedErrorTypeNames.Warning;
                    case ReviewType.MustFix:
                        return PredefinedErrorTypeNames.SyntaxError;
                    default:
                        return PredefinedErrorTypeNames.HintedSuggestion;
                }
            }
        }

        public object ToolTipContent
        {
            get
            {
                return $"{this.Commentator?.FirstName} {this.Commentator?.LastName}, {this.Review.CreationDate:dd-MM-yyyy HH:mm}: {this.Review.Comment}";
            }
        }
    }
}
