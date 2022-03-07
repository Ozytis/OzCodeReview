using EnvDTE;

using EnvDTE80;

using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Threading;

using OzCodeReview.ClientApi;
using OzCodeReview.ToolWindows;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace OzCodeReview
{
    public partial class CodeCommentReviewsWindowControl : UserControl, INotifyPropertyChanged
    {
        private double currentWidth;

        public event PropertyChangedEventHandler PropertyChanged;

        public CodeCommentReviewsWindowControl()
        {
            this.DataContext = this;
            var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));

            this.ReviewsService = componentModel.GetService<ReviewsService>();
            this.UsersService = componentModel.GetService<UsersService>();

            InitializeComponent();

            this.ReviewsService.OnReviewChanged += this.ReviewsService_OnReviewChanged;
            this.IsVisibleChanged += this.CodeCommentReviewsWindowControl_IsVisibleChanged;

            this.CurrentWidth = this.ActualWidth;

            this.SizeChanged += this.CodeCommentReviewsWindowControl_SizeChanged;
        }

        private void CodeCommentReviewsWindowControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.CurrentWidth = this.ActualWidth - 2;
        }

        public double CurrentWidth
        {
            get
            {
                return currentWidth;
            }
            set
            {
                currentWidth = value;

                this.PropertyChanged?.Invoke(this, new(nameof(CurrentWidth)));
            }
        }

#pragma warning disable VSTHRD100 // Avoid async void methods
        private async void CodeCommentReviewsWindowControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
#pragma warning restore VSTHRD100 // Avoid async void methods
        {
            string solutionName = (await VS.Solutions.GetCurrentSolutionAsync()).Name;

            if (this.ReviewsService?.Reviews == null)
            {
                await this.ReviewsService.LoadReviewsAsync(solutionName);
            }

            await this.LoadReviewsAsync(this.ReviewsService.Reviews);
        }

#pragma warning disable VSTHRD100 // Avoid async void methods
        private async void ReviewsService_OnReviewChanged(object sender, System.Collections.Generic.IEnumerable<ClientApi.Models.Review> reviews)
#pragma warning restore VSTHRD100 // Avoid async void methods
        {
            await this.LoadReviewsAsync(reviews);
        }

        private async Task LoadReviewsAsync(IEnumerable<ClientApi.Models.Review> reviews)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            this.Reviews.Clear();

            foreach (var review in reviews.Where(r => r.Status < ClientApi.Models.ReviewStatus.Closed)
                .OrderByDescending(r => r.CreationDate))
            {
                this.Reviews.Add(new ReviewInfo
                {
                    CommentatorName = this.UsersService.Users?.FirstOrDefault(u => u.Id == review.CommentatorId)?.ToString(),
                    RecipientName = this.UsersService.Users?.FirstOrDefault(u => u.Id == review.RecipientId)?.ToString(),
                    Comment = review.Comment,
                    CreationDate = review.CreationDate.ToString("dd/MM/yy HH:mm"),
                    FileName = review.FileName,
                    ProjectPath = Path.GetFileNameWithoutExtension(review.ProjectPath),
                    StartLineNumber = review.StartLineNumber,
                    LastCharIndex = review.LastCharIndex,
                    Branch = review.Branch,
                    Commit = review.Commit,
                    Id = review.Id,
                    EndLineNumber = review.EndLineNumber,
                    SolutionName = review.SolutionName,
                    StartCharIndex = review.StartCharIndex
                });
            }

            //this.ItemsList.ItemsSource = reviews;
        }

        public ReviewsService ReviewsService { get; }

        public UsersService UsersService { get; }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            VS.MessageBox.Show("CodeCommentReviewsWindowControl", "Button clicked");
        }

        public ObservableCollection<ReviewInfo> Reviews { get; set; } = new();

#pragma warning disable VSTHRD100 // Avoid async void methods
        private async void EditTextBlock_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)

        {
            EditReviewWindow editReviewWindow = new EditReviewWindow();
            await editReviewWindow.InitializeAsync((sender as TextBlock)?.DataContext as ReviewInfo);
            editReviewWindow.ShowModal();
        }

#pragma warning disable VSTHRD100 // Avoid async void methods
        private async void NavigateTextBlock_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
#pragma warning restore VSTHRD100 // Avoid async void methods
            ReviewInfo review = (sender as TextBlock)?.DataContext as ReviewInfo;

            string solutionDir = Path.GetDirectoryName(VS.Solutions.GetCurrentSolution().FullPath);

            var documentView = await VS.Documents.OpenAsync(Path.Combine(solutionDir, review.FileName));

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var startLine = documentView.TextView.TextSnapshot.GetLineFromLineNumber(review.StartLineNumber - 1);
            var start = startLine.Start + review.StartCharIndex - 1;

            DTE2 dte2 = Package.GetGlobalService(typeof(DTE)) as DTE2;
            var textSelection = dte2.ActiveDocument.Selection as TextSelection;
            textSelection.GotoLine(review.StartLineNumber, false);

            documentView.TextView.Caret.MoveTo(new SnapshotPoint(documentView.TextView.TextSnapshot, start));
        }

#pragma warning disable VSTHRD100 // Avoid async void methods
        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
#pragma warning restore VSTHRD100 // Avoid async void methods
        {           
            string solutionName = (await VS.Solutions.GetCurrentSolutionAsync()).Name;
            await this.ReviewsService.LoadReviewsAsync(solutionName);
        }
    }
}
