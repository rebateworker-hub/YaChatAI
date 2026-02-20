using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace YandexAIChat
{
    /// <summary>
    /// Code-behind for the AI Chat user control.
    /// Handles UI interactions and delegates AI processing to AIOrchestrator.
    /// </summary>
    public partial class ChatControl : UserControl
    {
        private AIOrchestrator? _orchestrator;
        private PromptHistory _history;
        private string _folderId = string.Empty;
        private string _apiKey = string.Empty;

        public ChatControl()
        {
            InitializeComponent();
            _history = new PromptHistory();
            LoadSettings();
            InitializeOrchestrator();
        }

        private void LoadSettings()
        {
            _folderId = Environment.GetEnvironmentVariable("YANDEX_FOLDER_ID") ?? string.Empty;
            _apiKey = Environment.GetEnvironmentVariable("YANDEX_API_KEY") ?? string.Empty;
        }

        private void InitializeOrchestrator()
        {
            if (!string.IsNullOrWhiteSpace(_folderId) && !string.IsNullOrWhiteSpace(_apiKey))
            {
                var aiManager = new YandexAIManager(_folderId, _apiKey);
                var artManager = new YandexARTManager(_folderId, _apiKey);
                _orchestrator = new AIOrchestrator(aiManager, artManager);
            }
        }

        private void PromptInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
            {
                e.Handled = true;
                _ = SendPromptAsync();
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            _ = SendPromptAsync();
        }

        private async void VoiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                ShowConfigurationRequired();
                return;
            }

            SetLoading(true, "Listening for voice input...");
            try
            {
                var voiceManager = new VoiceInputManager(_apiKey);
                var recognizedText = await voiceManager.RecognizeAsync();
                if (!string.IsNullOrWhiteSpace(recognizedText))
                {
                    PromptInput.Text = recognizedText;
                }
            }
            catch (Exception ex)
            {
                AppendErrorMessage($"Voice recognition failed: {ex.Message}");
            }
            finally
            {
                SetLoading(false);
            }
        }

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            var historyWindow = new HistoryWindow(_history);
            historyWindow.PromptSelected += (prompt) =>
            {
                PromptInput.Text = prompt;
            };
            historyWindow.ShowDialog();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(_folderId, _apiKey);
            if (settingsWindow.ShowDialog() == true)
            {
                _folderId = settingsWindow.FolderId;
                _apiKey = settingsWindow.ApiKey;
                InitializeOrchestrator();
            }
        }

        private async Task SendPromptAsync()
        {
            var userPrompt = PromptInput.Text?.Trim();
            if (string.IsNullOrWhiteSpace(userPrompt))
                return;

            if (_orchestrator == null)
            {
                ShowConfigurationRequired();
                return;
            }

            var selectedMode = GetSelectedMode();

            AppendUserMessage(userPrompt);
            PromptInput.Clear();
            _history.Add(userPrompt);

            SetLoading(true, "Processing with Yandex AI...");

            try
            {
                OrchestrationResult result;
                if (selectedMode == "visualization")
                {
                    var codeSnippet = string.IsNullOrWhiteSpace(userPrompt)
                        ? GetCurrentCodeSnippet()
                        : userPrompt;
                    result = await _orchestrator.ExecuteOrchestration(codeSnippet, "visualization");
                }
                else
                {
                    result = await _orchestrator.ExecuteOrchestration(userPrompt, selectedMode);
                }

                DisplayResult(result);
            }
            catch (Exception ex)
            {
                AppendErrorMessage($"Error: {ex.Message}");
            }
            finally
            {
                SetLoading(false);
            }
        }

        private string GetSelectedMode()
        {
            var item = ModeSelector.SelectedItem as ComboBoxItem;
            return item?.Content?.ToString()?.ToLowerInvariant() switch
            {
                "generation" => "code",
                "refactoring" => "refactor",
                "explanation" => "explanation",
                "visualization" => "visualization",
                "security analysis" => "security",
                _ => "code"
            };
        }

        private void DisplayResult(OrchestrationResult result)
        {
            if (result.Image != null && result.Image.Length > 0)
            {
                AppendImageMessage(result.Image);
            }

            if (!string.IsNullOrWhiteSpace(result.Code))
            {
                AppendAIMessage(result.Code, isCode: true);
            }

            if (!string.IsNullOrWhiteSpace(result.Analysis))
            {
                AppendAIMessage($"Security Analysis:\n{result.Analysis}", isCode: false);
            }
        }

        private void AppendUserMessage(string text)
        {
            var border = new Border
            {
                Background = System.Windows.Media.Brushes.AliceBlue,
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(8),
                Margin = new Thickness(40, 4, 4, 4)
            };
            border.Child = new TextBlock
            {
                Text = $"You: {text}",
                TextWrapping = TextWrapping.Wrap
            };
            ChatContainer.Children.Add(border);
            ScrollToBottom();
        }

        private void AppendAIMessage(string text, bool isCode)
        {
            var border = new Border
            {
                Background = isCode
                    ? System.Windows.Media.Brushes.LightGray
                    : System.Windows.Media.Brushes.LightYellow,
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(8),
                Margin = new Thickness(4, 4, 40, 4)
            };

            var textBlock = new TextBlock
            {
                Text = text,
                TextWrapping = TextWrapping.Wrap,
                FontFamily = isCode
                    ? new System.Windows.Media.FontFamily("Consolas, Courier New")
                    : null
            };

            border.Child = textBlock;
            ChatContainer.Children.Add(border);

            if (isCode)
            {
                var copyButton = new Button
                {
                    Content = "Copy to Clipboard",
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(4, 0, 0, 8),
                    Tag = text
                };
                copyButton.Click += (s, e) => Clipboard.SetText(text);
                ChatContainer.Children.Add(copyButton);
            }

            ScrollToBottom();
        }

        private void AppendImageMessage(byte[] imageBytes)
        {
            var bitmap = LoadBitmapFromBytes(imageBytes);
            var image = new Image
            {
                Source = bitmap,
                MaxHeight = 400,
                MaxWidth = 600,
                Stretch = System.Windows.Media.Stretch.Uniform,
                Margin = new Thickness(4)
            };

            var saveButton = new Button
            {
                Content = "Save Diagram",
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(4, 0, 0, 8),
                Tag = imageBytes
            };
            saveButton.Click += SaveImage_Click;

            var panel = new StackPanel { Margin = new Thickness(4) };
            panel.Children.Add(image);
            panel.Children.Add(saveButton);
            ChatContainer.Children.Add(panel);

            ScrollToBottom();
        }

        private void AppendErrorMessage(string text)
        {
            var border = new Border
            {
                Background = System.Windows.Media.Brushes.MistyRose,
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(8),
                Margin = new Thickness(4)
            };
            border.Child = new TextBlock
            {
                Text = text,
                TextWrapping = TextWrapping.Wrap,
                Foreground = System.Windows.Media.Brushes.DarkRed
            };
            ChatContainer.Children.Add(border);
            ScrollToBottom();
        }

        private void SaveImage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is byte[] imageBytes)
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "PNG files (*.png)|*.png",
                    FileName = $"diagram_{DateTime.Now:yyyyMMdd_HHmmss}.png"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    File.WriteAllBytes(saveDialog.FileName, imageBytes);
                    StatusText.Text = $"Diagram saved: {saveDialog.FileName}";
                }
            }
        }

        private static BitmapImage LoadBitmapFromBytes(byte[] bytes)
        {
            using var stream = new MemoryStream(bytes);
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = stream;
            image.EndInit();
            image.Freeze();
            return image;
        }

        private string GetCurrentCodeSnippet()
        {
            return PromptInput.Text;
        }

        private void SetLoading(bool isLoading, string? message = null)
        {
            LoadingIndicator.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
            StatusText.Text = isLoading ? (message ?? "Processing...") : "Ready";
            SendButton.IsEnabled = !isLoading;
        }

        private void ShowConfigurationRequired()
        {
            AppendErrorMessage("Please configure your Yandex API key and folder ID in Settings.");
        }

        private void ScrollToBottom()
        {
            ChatScrollViewer.ScrollToEnd();
        }
    }
}
