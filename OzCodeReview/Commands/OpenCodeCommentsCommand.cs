using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzCodeReview.Commands
{
    [Command(PackageIds.OpenCodeCommentsCommand)]
    public sealed class OpenCodeCommentsCommand : BaseCommand<OpenCodeCommentsCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await CodeCommentReviewsWindow.ShowAsync();
        }
    }
}
