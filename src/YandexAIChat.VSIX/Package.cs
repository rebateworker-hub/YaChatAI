using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace YandexAIChat
{
    /// <summary>
    /// Main VS package for Yandex AI Chat extension.
    /// Registers the tool window command and provides the package entry point.
    /// </summary>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PackageGuids.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(ChatToolWindow))]
    public sealed class YandexAIChatPackage : AsyncPackage
    {
        /// <summary>
        /// Initializes the package asynchronously.
        /// </summary>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress);
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await ShowChatCommand.InitializeAsync(this);
        }
    }

    /// <summary>
    /// GUID constants for the package.
    /// </summary>
    internal static class PackageGuids
    {
        public const string PackageGuidString = "1a2b3c4d-5e6f-7890-abcd-ef1234567890";
        public static readonly Guid PackageGuid = new Guid(PackageGuidString);

        public const string CommandSetGuidString = "2b3c4d5e-6f78-90ab-cdef-123456789012";
        public static readonly Guid CommandSetGuid = new Guid(CommandSetGuidString);

        public const int ShowChatCommandId = 0x0100;
    }
}
