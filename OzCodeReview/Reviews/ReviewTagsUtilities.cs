using EnvDTE;

using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

using OzCodeReview.ClientApi;

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

namespace OzCodeReview.Reviews
{
    [Export(typeof(ReviewTagsUtilities))]
    public class ReviewTagsUtilities
    {
        public ReviewsService ReviewsService { get; }


        public UsersService UsersService { get; }

        [ImportingConstructor]
        public ReviewTagsUtilities(ReviewsService reviewsService, UsersService usersService)
        {
            this.ReviewsService = reviewsService;
            this.UsersService = usersService;
        }

        public IEnumerable<ITagSpan<T>> GetTags<T>(ITextSnapshot currentSnapShot, string filePath,
            IEnumerable<SnapshotSpan> spans, EventHandler<SnapshotSpanEventArgs> tagsChanged)
            where T : IReviewTag, new()
        {
            string currentBranch = GitUtils.GetCurrentBranch(filePath);

            if (this.ReviewsService?.Reviews != null)
            {
                string solutionDir = Path.GetDirectoryName(VS.Solutions.GetCurrentSolution().FullPath);
                filePath = PathUtil.MakeRelative(solutionDir, filePath);

                var reviewsSpans = this.ReviewsService.Reviews
                    .Where(s => s.FileName == filePath && s.Branch == currentBranch && s.Status < ClientApi.Models.ReviewStatus.Closed)
                    .Select(review =>
                    {
                        int start = currentSnapShot.GetLineFromLineNumber(review.StartLineNumber - 1).Start.Position + review.StartCharIndex - 1;
                        int end = currentSnapShot.GetLineFromLineNumber(review.EndLineNumber - 1).Start.Position + review.LastCharIndex - 1;

                        var span = new
                        {
                            Review = review,
                            Span = new SnapshotSpan(currentSnapShot, new Span(start, end - start)),
                            Commentator = this.UsersService.Users?.FirstOrDefault(u => u.Id == review.CommentatorId),
                            Recipient = this.UsersService.Users?.FirstOrDefault(u => u.Id == review.RecipientId)
                        };

                        return span;
                    })
                    .ToArray();

                if (reviewsSpans.Any())
                {
                    int changeStart = int.MaxValue;
                    int changeEnd = int.MinValue;

                    bool oneFound = false;

                    foreach (var span in reviewsSpans)
                    {
                        var found = spans.FirstOrDefault(sspan => sspan.Start.Position <= span.Span.Start.Position && sspan.End.Position >= span.Span.End.Position);

                        if (found != null)
                        {
                            changeStart = Math.Min(changeStart, span.Span.Start.Position);
                            changeEnd = Math.Max(changeEnd, span.Span.End.Position);

                            oneFound = true;

                            yield return new TagSpan<T>(new SnapshotSpan(span.Span.Start, span.Span.Length), new T
                            {
                                Review = span.Review,
                                Commentator = span.Commentator,
                                Recipient = span.Recipient
                            });
                        }
                    }

                    if (oneFound)
                    {
                        tagsChanged?.Invoke(this, new SnapshotSpanEventArgs(new SnapshotSpan(currentSnapShot, Span.FromBounds(changeStart, changeEnd))));
                    }
                }
            }
        }
    }
}
