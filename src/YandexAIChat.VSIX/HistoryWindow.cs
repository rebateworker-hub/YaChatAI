using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace YandexAIChat
{
    /// <summary>
    /// Dialog window for browsing and searching prompt history.
    /// </summary>
    public class HistoryWindow : Window
    {
        private readonly PromptHistory _history;
        private ListBox _listBox = null!;
        private TextBox _searchBox = null!;

        public event Action<string>? PromptSelected;

        public HistoryWindow(PromptHistory history)
        {
            _history = history ?? throw new ArgumentNullException(nameof(history));
            Title = "Prompt History";
            Width = 600;
            Height = 400;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            BuildUI();
        }

        private void BuildUI()
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Search row
            var searchPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };
            searchPanel.Children.Add(new TextBlock
            {
                Text = "Search: ",
                VerticalAlignment = VerticalAlignment.Center
            });
            _searchBox = new TextBox { Width = 300, Margin = new Thickness(5, 0, 0, 0) };
            _searchBox.TextChanged += (s, e) => RefreshList();
            searchPanel.Children.Add(_searchBox);

            var clearBtn = new Button { Content = "Clear History", Margin = new Thickness(10, 0, 0, 0) };
            clearBtn.Click += (s, e) =>
            {
                if (MessageBox.Show("Clear all history?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    _history.Clear();
                    RefreshList();
                }
            };
            searchPanel.Children.Add(clearBtn);
            Grid.SetRow(searchPanel, 0);
            grid.Children.Add(searchPanel);

            // List row
            _listBox = new ListBox { Margin = new Thickness(5) };
            _listBox.MouseDoubleClick += (s, e) => UseSelected();
            Grid.SetRow(_listBox, 1);
            grid.Children.Add(_listBox);

            // Button row
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(5)
            };
            var useBtn = new Button { Content = "Use Prompt", Width = 100, Margin = new Thickness(5, 0, 0, 0) };
            useBtn.Click += (s, e) => UseSelected();
            var closeBtn = new Button { Content = "Close", Width = 80, Margin = new Thickness(5, 0, 0, 0) };
            closeBtn.Click += (s, e) => Close();
            buttonPanel.Children.Add(useBtn);
            buttonPanel.Children.Add(closeBtn);
            Grid.SetRow(buttonPanel, 2);
            grid.Children.Add(buttonPanel);

            Content = grid;
            RefreshList();
        }

        private void RefreshList()
        {
            var keyword = _searchBox?.Text ?? string.Empty;
            var entries = _history.Search(keyword);

            _listBox.Items.Clear();
            foreach (var entry in entries)
            {
                _listBox.Items.Add(new ListBoxItem
                {
                    Content = $"[{entry.Timestamp:yyyy-MM-dd HH:mm}] {entry.Prompt}",
                    Tag = entry.Prompt
                });
            }
        }

        private void UseSelected()
        {
            if (_listBox.SelectedItem is ListBoxItem item && item.Tag is string prompt)
            {
                PromptSelected?.Invoke(prompt);
                Close();
            }
        }
    }
}
