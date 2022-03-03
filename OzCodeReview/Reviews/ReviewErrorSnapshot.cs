
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;

using OzCodeReview.ClientApi.Models;

using System.Collections.Generic;
using System.IO;

namespace OzCodeReview.Reviews
{
    internal class ReviewErrorSnapshot : WpfTableEntriesSnapshotBase
    {
        private const int UndefinedLineOrColumn = -1;


        internal readonly List<ReviewErrorTag> Errors = new List<ReviewErrorTag>();

        public ReviewErrorSnapshot NextSnapshot { get; set; }

        public ReviewErrorSnapshot(int versionNumber, IList<ReviewErrorTag> errors, string solutionDir)
        {

            this.versionNumber = versionNumber;
            this.SolutionDir = solutionDir;
            this.Errors = new List<ReviewErrorTag>(errors);
        }

        private int versionNumber;

        public override int Count
        {
            get
            {
                return this.Errors.Count;
            }
        }

        public override int VersionNumber
        {
            get
            {
                return this.versionNumber;
            }
        }

        public string SolutionDir { get; }

        public __VSERRORCATEGORY ToVSERRORCATEGORY(ReviewType reviewType)
        {
            if (reviewType == ReviewType.ShouldFix)
            {
                return __VSERRORCATEGORY.EC_WARNING;
            }
            else if (reviewType == ReviewType.MustFix)
            {
                return __VSERRORCATEGORY.EC_ERROR;
            }

            return __VSERRORCATEGORY.EC_MESSAGE;
        }

        public override bool TryGetValue(int index, string columnName, out object content)
        {
            if (index >= 0 && index < this.Errors.Count)
            {
                var error = this.Errors[index];


                switch (columnName)
                {
                    case StandardTableKeyNames.Text:
                        content = error.ToolTipContent;
                        return true;
                    case StandardTableKeyNames.ProjectName:
                        content = Path.GetFileNameWithoutExtension(error.Review.ProjectPath);
                        return true;
                    case StandardTableKeyNames.ProjectGuid:
                        content = "";
                        return true;
                    case StandardTableKeyNames.DocumentName:
                        content = Path.Combine(this.SolutionDir, error.Review.FileName);
                        return true;
                    case StandardTableKeyNames.Line:
                        content = error.Review.StartLineNumber - 1;
                        return true;
                    case StandardTableKeyNames.Column:
                        content = error.Review.StartCharIndex - 1;
                        return true;
                    case StandardTableKeyNames.ErrorSeverity:
                        content = this.ToVSERRORCATEGORY(error.Review.Type);
                        return true;
                    case StandardTableKeyNames.TaskCategory:
                        content = VSTASKCATEGORY.CAT_BUILDCOMPILE;
                        return true;
                    case StandardTableKeyNames.ErrorSource:
                        content = ErrorSource.Other;
                        return true;
                    case StandardTableKeyNames.ErrorCode:
                        content = error.Review.Type.ToString();
                        return true;
                    case StandardTableKeyNames.BuildTool:
                        content = Vsix.Name;
                        return true;
                }
            }

            // This method gets called for anything that the control supports. If we do not have a value
            // to supply for the column or if somehow we got an invalid index the content needs to be
            // set to null and return false which lets the table control set the value.
            content = null;
            return false;
        }         

    }
}