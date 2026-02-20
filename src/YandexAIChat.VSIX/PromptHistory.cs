using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace YandexAIChat
{
    /// <summary>
    /// Stores and retrieves prompt history, persisted to a local JSON file.
    /// </summary>
    public class PromptHistory
    {
        private readonly string _filePath;
        private readonly List<PromptEntry> _entries;
        private const int MaxEntries = 500;

        public PromptHistory()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dir = Path.Combine(appData, "YandexAIChat");
            Directory.CreateDirectory(dir);
            _filePath = Path.Combine(dir, "prompt_history.json");
            _entries = Load();
        }

        /// <summary>All recorded prompt entries, most recent first.</summary>
        public IReadOnlyList<PromptEntry> Entries => _entries;

        /// <summary>Adds a prompt to the history and persists it.</summary>
        public void Add(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                return;

            _entries.Insert(0, new PromptEntry
            {
                Prompt = prompt,
                Timestamp = DateTime.UtcNow
            });

            if (_entries.Count > MaxEntries)
                _entries.RemoveRange(MaxEntries, _entries.Count - MaxEntries);

            Save();
        }

        /// <summary>Searches entries by keyword (case-insensitive).</summary>
        public IEnumerable<PromptEntry> Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return _entries;

            return _entries.FindAll(e =>
                e.Prompt.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        /// <summary>Clears all history entries.</summary>
        public void Clear()
        {
            _entries.Clear();
            Save();
        }

        private List<PromptEntry> Load()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    var json = File.ReadAllText(_filePath);
                    return JsonConvert.DeserializeObject<List<PromptEntry>>(json)
                           ?? new List<PromptEntry>();
                }
            }
            catch (Exception)
            {
                // If the file is corrupt, start fresh
            }
            return new List<PromptEntry>();
        }

        private void Save()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_entries, Formatting.Indented);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception)
            {
                // Silently ignore persistence failures
            }
        }
    }

    /// <summary>
    /// A single entry in the prompt history.
    /// </summary>
    public class PromptEntry
    {
        public string Prompt { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
