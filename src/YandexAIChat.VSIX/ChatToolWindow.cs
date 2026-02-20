using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace YandexAIChat
{
    /// <summary>
    /// Tool window that hosts the AI Chat user interface.
    /// </summary>
    [Guid("3c4d5e6f-7890-abcd-ef12-345678901234")]
    public class ChatToolWindow : ToolWindowPane
    {
        public ChatToolWindow() : base(null)
        {
            Caption = "Yandex AI Chat";
            Content = new ChatControl();
        }
    }
}
