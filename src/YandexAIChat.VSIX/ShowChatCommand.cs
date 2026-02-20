using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace YandexAIChat
{
    /// <summary>
    /// Command that opens the Yandex AI Chat tool window.
    /// </summary>
    internal sealed class ShowChatCommand
    {
        private readonly AsyncPackage _package;

        private ShowChatCommand(AsyncPackage package)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            var commandService = await package.GetServiceAsync(typeof(System.ComponentModel.Design.IMenuCommandService))
                as System.ComponentModel.Design.OleMenuCommandService;

            if (commandService != null)
            {
                var menuCommandID = new System.ComponentModel.Design.CommandID(
                    PackageGuids.CommandSetGuid,
                    PackageGuids.ShowChatCommandId);

                var menuItem = new System.ComponentModel.Design.MenuCommand(
                    Execute,
                    menuCommandID);

                commandService.AddCommand(menuItem);
            }
        }

        private static void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var package = (YandexAIChatPackage)ServiceProvider.GlobalProvider.GetService(typeof(YandexAIChatPackage));
            if (package == null) return;

            var window = package.FindToolWindow(typeof(ChatToolWindow), 0, true);
            if (window?.Frame == null) return;

            var windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }
    }
}
