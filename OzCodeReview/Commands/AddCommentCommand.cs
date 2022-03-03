using EnvDTE;

using Microsoft.VisualStudio.PlatformUI;

using OzCodeReview.ToolWindows;

using System.IO;
using System.Linq;

namespace OzCodeReview
{
    [Command(PackageIds.InsertCodeCommentCommand)]
    internal sealed class AddCommentCommand : BaseCommand<AddCommentCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            if (!await (Package as OzCodeReviewPackage).CheckSettingsAsync())
            {
                return;
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var document = await VS.Documents.GetActiveDocumentViewAsync();
            var textView = document.TextView;

            var selection = textView.Selection;

            var selectedSpan = document.TextView.Selection.SelectedSpans.FirstOrDefault();

            if (selectedSpan != null)
            {
                string currentBranch = GitUtils.GetCurrentBranch(document.FilePath);

                if (string.IsNullOrEmpty(currentBranch))
                {
                    await VS.MessageBox.ShowErrorAsync("Le projet n'est pas sous git");
                    return;
                }

                string fullSolutionPath = (await VS.Solutions.GetCurrentSolutionAsync()).FullPath;
                string projectFullPath = (await VS.Solutions.GetActiveProjectAsync()).FullPath;
                string fileFullPath = document.FilePath;

                string solutionDir = Path.GetDirectoryName(fullSolutionPath);

                if (!PathUtil.IsDescendant(solutionDir, fileFullPath))
                {
                    await VS.MessageBox.ShowErrorAsync("This extension works only if project files are in solution subdirectories");
                }

                string solutionName = Path.GetFileName(fullSolutionPath);
                string projectPath = PathUtil.MakeRelative(solutionDir, projectFullPath);
                string filePath = PathUtil.MakeRelative(solutionDir, fileFullPath);


                string lastCommit = GitUtils.GetLastCommit(document.FilePath, currentBranch);

                int startLine = selectedSpan.Snapshot.GetLineNumberFromPosition(selectedSpan.Start.Position);
                int endLine = selectedSpan.Snapshot.GetLineNumberFromPosition(selectedSpan.End.Position);

                int startChar = selectedSpan.Start.Position - selectedSpan.Snapshot.GetLineFromLineNumber(startLine).Start.Position;
                int endChar = selectedSpan.End.Position - selectedSpan.Snapshot.GetLineFromLineNumber(endLine).Start.Position;

                await this.AddReviewAsync(filePath, solutionName, projectPath, currentBranch, lastCommit, startLine, endLine, startChar, endChar);
            }
        }

        private async Task AddReviewAsync(string filePath, string solutionName, string projectPath, string currentBranch, string lastCommit,
            int startLine, int endLine, int startChar, int endChar)
        {
            AddReviewWindow addReviewWindow = new(filePath, solutionName, projectPath, currentBranch, lastCommit, startLine + 1, endLine + 1, startChar + 1, endChar + 1);
            await addReviewWindow.InitializeAsync(Package);
            addReviewWindow.ShowModal();
        }
    }
}
