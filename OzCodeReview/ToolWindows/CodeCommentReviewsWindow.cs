using Microsoft.VisualStudio.Imaging;

using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace OzCodeReview
{
    public class CodeCommentReviewsWindow : BaseToolWindow<CodeCommentReviewsWindow>
    {
        public override string GetTitle(int toolWindowId) => "Code reviews list";

        public override Type PaneType => typeof(Pane);

        public override Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken)
        {
            return Task.FromResult<FrameworkElement>(new CodeCommentReviewsWindowControl());
        }

        [Guid("2cf18c3f-74ad-4a35-9554-e9b08c6a4b69")]
        internal class Pane : ToolWindowPane
        {
            public Pane()
            {
                BitmapImageMoniker = KnownMonikers.NewListQuery;
            }
        }      
    }
}
