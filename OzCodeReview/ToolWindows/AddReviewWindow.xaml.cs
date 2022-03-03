using Microsoft;
using Microsoft.VisualStudio.PlatformUI;

using OzCodeReview.ClientApi;
using OzCodeReview.ClientApi.Models;

using System.Collections.Generic;
using System.Linq;

namespace OzCodeReview.ToolWindows
{
    public partial class AddReviewWindow : DialogWindow
    {
        private ReviewType selectedReviewType;
        private UserModel selectedRecipient;

        public AddReviewWindow()
        {
            this.InitializeComponent();

            this.ReviewTypeList.ItemsSource = this.ReviewTypes;
        }

        public ReviewCreationModel Model { get; set; } = new();

        public AddReviewWindow(string fileName, string solutionName, string projectPath, string branch, string commit, int startLine, int endLine, int startCharIndex, int endCharIndex) : this()
        {
            this.SelectedReviewType = ReviewType.NotSet;

            this.Model = new()
            {
                SolutionName = solutionName,
                FileName = fileName,
                Branch = branch,
                Commit = commit,
                EndLineNumber = endLine,
                StartCharIndex = startCharIndex,
                LastCharIndex = endCharIndex,
                StartLineNumber = startLine,
                Type = ReviewType.ShouldFix,
                ProjectPath = projectPath
            };
        }

        public async Task InitializeAsync(AsyncPackage asyncPackage)
        {
            this.Package = asyncPackage;

            this.UsersService = await Package.GetServiceAsync(typeof(UsersService)) as UsersService;
            this.ReviewsService = await Package.GetServiceAsync(typeof(ReviewsService)) as ReviewsService;

            Assumes.Present(this.UsersService);
            Assumes.Present(this.ReviewsService);

            await this.LoadUsersAsync();
        }

        public async Task LoadUsersAsync()
        {
            this.Users = (await this.UsersService.GetAllUsersAsync()).ToList();
            this.RecipientList.ItemsSource = this.Users;
        }

        public List<OptionValue<ReviewType>> ReviewTypes { get; set; } = new()
        {
            new() { Value = ReviewType.MustFix, Label = "Must Fix" },
            new() { Value = ReviewType.ShouldFix, Label = "Should Fix" },
            new() { Value = ReviewType.Comment, Label = "Comment" },
        };

        public AsyncPackage Package { get; private set; }

        public UsersService UsersService { get; private set; }
        public ReviewsService ReviewsService { get; private set; }
        public List<UserModel> Users { get; set; }

        public bool CanProcess { get; set; }

        private void CancelButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Close();
        }


        public ReviewType SelectedReviewType
        {
            get
            {
                return selectedReviewType;
            }
            set
            {
                selectedReviewType = value;
                this.UpdateCanProcess();
            }
        }

        public UserModel SelectedRecipient
        {
            get
            {
                return selectedRecipient;
            }
            set
            {
                selectedRecipient = value;
                this.UpdateCanProcess();
            }
        }

        protected void UpdateCanProcess()
        {
            this.CanProcess = this.SelectedReviewType > ReviewType.NotSet
                && this.SelectedRecipient != null;
        }

        private void ValidationButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!this.CanProcess)
            {
                VS.MessageBox.ShowError("Please set text, review type and recipient");
                return;
            }

            this.Model.Comment = this.ReviewTextBox.Text;
            this.Model.Type = this.SelectedReviewType;
            this.Model.RecipientId = this.SelectedRecipient.Id;

            Task.Run(async () =>
            {

                try
                {
                    var result = await this.ReviewsService.CreateReviewAsync(this.Model);

                    if (!result.Success)
                    {
                        await VS.MessageBox.ShowErrorAsync(String.Join(", ", result.Errors));
                        return;
                    }

                    await this.ReviewsService.LoadReviewsAsync(this.Model.SolutionName);
                }
                catch (Exception ex)
                {
                    await VS.MessageBox.ShowErrorAsync(ex.Message);
                    return;
                }

                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                this.Close();
            }).FireAndForget();
        }

        private void ReviewTypeList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            this.SelectedReviewType = (this.ReviewTypeList.SelectedItem as OptionValue<ReviewType>).Value;
        }

        private void RecipientList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            this.SelectedRecipient = this.RecipientList.SelectedItem as UserModel;
        }
    }
}
