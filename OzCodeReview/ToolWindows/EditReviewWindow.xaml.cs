using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.PlatformUI;

using OzCodeReview.ClientApi;
using OzCodeReview.ClientApi.Models;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace OzCodeReview.ToolWindows
{
    public partial class EditReviewWindow : DialogWindow, INotifyPropertyChanged
    {
        private OptionValue<ReviewStatus> currentStatus;

        private ObservableCollection<OptionValue<ReviewStatus>> statusList = new();
        private ObservableCollection<OptionValue<ReviewType>> typesList = new();

        private ReviewInfo reviewInfo = new();
        private bool canEditType;
        private OptionValue<ReviewType> currentType;
        private string newComment;
        private double currentWidth;

        public EditReviewWindow()
        {
            this.InitializeComponent();
            this.CurrentWidth = this.ActualWidth - 20;

            this.SizeChanged += this.EditReviewWindow_SizeChanged;
        }

        private void EditReviewWindow_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            this.CurrentWidth = this.ActualWidth - 20;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<ReviewComment> Comments { get; set; } = new();

        public OptionValue<ReviewStatus> CurrentStatus
        {
            get
            {
                return this.currentStatus;
            }
            set
            {
                this.currentStatus = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.CurrentStatus)));
            }
        }

        public UserModel CurrentUser { get; set; }

        public double CurrentWidth
        {
            get
            {
                return this.currentWidth;
            }
            set
            {
                this.currentWidth = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.CurrentWidth)));
            }
        }

        public ReviewInfo ReviewInfo
        {
            get
            {
                return this.reviewInfo;
            }
            set
            {
                this.reviewInfo = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.ReviewInfo)));
            }
        }
        public ReviewsService ReviewsService { get; private set; }

        public ObservableCollection<OptionValue<ReviewStatus>> StatusList
        {
            get
            {
                return this.statusList;
            }
            set
            {
                this.statusList = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.StatusList)));
            }
        }

        public ObservableCollection<OptionValue<ReviewType>> TypesList
        {
            get
            {
                return this.typesList;
            }
            set
            {
                this.typesList = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.TypesList)));
            }
        }

        public bool CanEditType
        {
            get
            {
                return this.canEditType;
            }
            set
            {
                this.canEditType = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.CanEditType)));
            }
        }

        public OptionValue<ReviewType> CurrentType
        {
            get
            {
                return this.currentType;
            }
            set
            {
                this.currentType = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.CurrentType)));
            }
        }

        public string NewComment
        {
            get
            {
                return this.newComment;
            }
            set
            {
                this.newComment = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.NewComment)));
            }
        }

        public UsersService UsersService { get; private set; }

        public async Task InitializeAsync(ReviewInfo reviewInfo)
        {
            this.ReviewInfo = reviewInfo;

            var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));

            this.UsersService = componentModel.GetService<UsersService>();
            this.ReviewsService = componentModel.GetService<ReviewsService>();

            this.CurrentUser = await this.UsersService.GetCurrentUserAsync();

            var review = this.ReviewsService.Reviews.FirstOrDefault(r => r.Id == reviewInfo.Id);

            this.Comments.Clear();

            if (review.Comments != null)
            {
                foreach(var comment in review.Comments)
                {
                    this.Comments.Add(comment);
                }
            }

            switch (review.Status)
            {
                case ReviewStatus.Pending:

                    this.StatusList.Add(new OptionValue<ReviewStatus>
                    {
                        Label = "Pending correction",
                        Value = ReviewStatus.Pending
                    });

                    this.StatusList.Add(new OptionValue<ReviewStatus>
                    {
                        Label = "Corrected",
                        Value = ReviewStatus.Corrected
                    });

                    if (review.CommentatorId == this.CurrentUser.Id)
                    {
                        this.StatusList.Add(new OptionValue<ReviewStatus>
                        {
                            Label = "Closed",
                            Value = ReviewStatus.Closed
                        });
                    }
                    else
                    {
                        this.StatusList.Add(new OptionValue<ReviewStatus>
                        {
                            Label = "Rejected",
                            Value = ReviewStatus.Rejected
                        });
                    }

                    break;

                case ReviewStatus.Rejected:

                    this.StatusList.Add(new OptionValue<ReviewStatus>
                    {
                        Label = "Rejected",
                        Value = ReviewStatus.Rejected
                    });

                    this.StatusList.Add(new OptionValue<ReviewStatus>
                    {
                        Label = "Pending correction",
                        Value = ReviewStatus.Pending
                    });

                    this.StatusList.Add(new OptionValue<ReviewStatus>
                    {
                        Label = "Corrected",
                        Value = ReviewStatus.Corrected
                    });

                    if (review.CommentatorId == this.CurrentUser.Id)
                    {
                        this.StatusList.Add(new OptionValue<ReviewStatus>
                        {
                            Label = "Closed",
                            Value = ReviewStatus.Closed
                        });
                    }

                    break;

                case ReviewStatus.Corrected:

                    this.StatusList.Add(new OptionValue<ReviewStatus>
                    {
                        Label = "Corrected",
                        Value = ReviewStatus.Corrected
                    });

                    this.StatusList.Add(new OptionValue<ReviewStatus>
                    {
                        Label = "Pending correction",
                        Value = ReviewStatus.Pending
                    });

                    this.StatusList.Add(new OptionValue<ReviewStatus>
                    {
                        Label = "Rejected",
                        Value = ReviewStatus.Rejected
                    });

                    if (review.CommentatorId == this.CurrentUser.Id)
                    {
                        this.StatusList.Add(new OptionValue<ReviewStatus>
                        {
                            Label = "Closed",
                            Value = ReviewStatus.Closed
                        });
                    }

                    break;

                case ReviewStatus.Closed:

                    this.StatusList.Add(new OptionValue<ReviewStatus>
                    {
                        Label = "Closed",
                        Value = ReviewStatus.Closed
                    });

                    break;

                default:
                    break;
            }

            this.CurrentStatus = this.StatusList.FirstOrDefault();

            this.CanEditType = review.CommentatorId == this.CurrentUser?.Id;

            this.TypesList = new(new()
            {
                new() { Value = ReviewType.MustFix, Label = "Must Fix" },
                new() { Value = ReviewType.ShouldFix, Label = "Should Fix" },
                new() { Value = ReviewType.Comment, Label = "Comment" },
            });

            this.CurrentType = this.TypesList.FirstOrDefault(tl => tl.Value == review.Type);
        }

        private void CancelButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Close();
        }

        private void ValidationButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Task.Run(async () =>
            {

                try
                {
                    var result = await this.ReviewsService.UpdateReviewAsync(new ReviewUpdateModel
                    {
                        Comment = this.NewComment,
                        Id = this.ReviewInfo.Id,
                        ReviewStatus = this.CurrentStatus.Value,
                        ReviewType = this.CurrentType.Value,
                    });

                    if (!result.Success)
                    {
                        await VS.MessageBox.ShowErrorAsync(String.Join(", ", result.Errors));
                        return;
                    }

                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                    await this.ReviewsService.LoadReviewsAsync(result.Data.SolutionName);
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
    }
}