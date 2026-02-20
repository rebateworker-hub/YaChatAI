using System;
using System.Windows;
using System.Windows.Controls;

namespace YandexAIChat
{
    /// <summary>
    /// Dialog for configuring Yandex API credentials.
    /// </summary>
    public class SettingsWindow : Window
    {
        private TextBox _folderIdBox = null!;
        private TextBox _apiKeyBox = null!;

        public string FolderId { get; private set; }
        public string ApiKey { get; private set; }

        public SettingsWindow(string folderId, string apiKey)
        {
            FolderId = folderId;
            ApiKey = apiKey;
            Title = "Yandex AI Chat Settings";
            Width = 450;
            Height = 220;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            BuildUI();
        }

        private void BuildUI()
        {
            var grid = new Grid { Margin = new Thickness(12) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            AddRow(grid, 0, "Folder ID:", _folderIdBox = new TextBox
            {
                Text = FolderId,
                ToolTip = "Your Yandex Cloud folder ID (e.g., b1g8s...)"
            });

            AddRow(grid, 1, "API Key:", _apiKeyBox = new TextBox
            {
                Text = ApiKey,
                ToolTip = "Your Yandex Cloud API key"
            });

            var helpText = new TextBlock
            {
                Text = "Get credentials at: console.cloud.yandex.ru",
                FontStyle = System.Windows.FontStyles.Italic,
                Foreground = System.Windows.Media.Brushes.Gray,
                Margin = new Thickness(0, 4, 0, 8)
            };
            Grid.SetRow(helpText, 2);
            Grid.SetColumnSpan(helpText, 2);
            grid.Children.Add(helpText);

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            var saveBtn = new Button { Content = "Save", Width = 80, IsDefault = true, Margin = new Thickness(0, 0, 8, 0) };
            saveBtn.Click += SaveButton_Click;
            var cancelBtn = new Button { Content = "Cancel", Width = 80, IsCancel = true };
            cancelBtn.Click += (s, e) => DialogResult = false;
            buttonPanel.Children.Add(saveBtn);
            buttonPanel.Children.Add(cancelBtn);

            Grid.SetRow(buttonPanel, 3);
            Grid.SetColumnSpan(buttonPanel, 2);
            grid.Children.Add(buttonPanel);

            Content = grid;
        }

        private static void AddRow(Grid grid, int row, string label, TextBox input)
        {
            var lbl = new TextBlock
            {
                Text = label,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 8, 8)
            };
            Grid.SetRow(lbl, row);
            Grid.SetColumn(lbl, 0);
            grid.Children.Add(lbl);

            input.Margin = new Thickness(0, 0, 0, 8);
            Grid.SetRow(input, row);
            Grid.SetColumn(input, 1);
            grid.Children.Add(input);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            FolderId = _folderIdBox.Text.Trim();
            ApiKey = _apiKeyBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(FolderId) || string.IsNullOrWhiteSpace(ApiKey))
            {
                MessageBox.Show("Both Folder ID and API Key are required.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
        }
    }
}
