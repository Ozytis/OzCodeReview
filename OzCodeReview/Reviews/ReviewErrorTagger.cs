using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

using System.Collections.Generic;
using System.Linq;

namespace OzCodeReview.Reviews
{
    internal class ReviewErrorTagger : ITagger<ReviewErrorTag>
    {
        public ReviewErrorTagger(string filePath, ITextSnapshot currentSnapshot, ReviewTagsUtilities reviewTagsUtilities,
            ReviewErrorsFactory factory)
        {
            this.FilePath = filePath;
            this.CurrentSnapshot = currentSnapshot;
            this.ReviewTagsUtilities = reviewTagsUtilities;
            this.Factory = factory;
            this.ReviewTagsUtilities.ReviewsService.OnReviewChanged += this.ReviewsService_OnReviewChanged;
        }

        private void ReviewsService_OnReviewChanged(object sender, IEnumerable<ClientApi.Models.Review> e)
        {
            this.TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(new SnapshotSpan(CurrentSnapshot, Span.FromBounds(0, CurrentSnapshot.Length))));
        }

        public string FilePath { get; }

        public ITextSnapshot CurrentSnapshot { get; }

        public ReviewTagsUtilities ReviewTagsUtilities { get; }
      
        public ReviewErrorsFactory Factory { get; }

        
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public IEnumerable<ITagSpan<ReviewErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            var tags = this.ReviewTagsUtilities.GetTags<ReviewErrorTag>(this.CurrentSnapshot, this.FilePath, spans, null);

            this.Factory.AddErrorItems(tags.Select(t => t.Tag));

            return tags;
        }
    }
}