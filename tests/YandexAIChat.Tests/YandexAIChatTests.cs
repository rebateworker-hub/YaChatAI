using System;
using System.Collections.Generic;
using Xunit;

namespace YandexAIChat.Tests
{
    public class PromptHistoryTests
    {
        [Fact]
        public void Add_SinglePrompt_AppearsInEntries()
        {
            var history = new PromptHistory();
            history.Clear();

            history.Add("Generate a REST API");

            Assert.Contains(history.Entries, e => e.Prompt == "Generate a REST API");
        }

        [Fact]
        public void Add_EmptyPrompt_IsIgnored()
        {
            var history = new PromptHistory();
            history.Clear();
            int countBefore = history.Entries.Count;

            history.Add(string.Empty);
            history.Add("   ");

            Assert.Equal(countBefore, history.Entries.Count);
        }

        [Fact]
        public void Add_SetsTimestamp()
        {
            var history = new PromptHistory();
            history.Clear();
            var before = DateTime.UtcNow.AddSeconds(-1);

            history.Add("Test prompt");

            var after = DateTime.UtcNow.AddSeconds(1);
            Assert.All(history.Entries, e =>
                Assert.InRange(e.Timestamp, before, after));
        }

        [Fact]
        public void Add_InsertsAtFront()
        {
            var history = new PromptHistory();
            history.Clear();

            history.Add("First");
            history.Add("Second");

            Assert.Equal("Second", history.Entries[0].Prompt);
            Assert.Equal("First", history.Entries[1].Prompt);
        }

        [Fact]
        public void Clear_RemovesAllEntries()
        {
            var history = new PromptHistory();
            history.Add("Something");
            history.Clear();

            Assert.Empty(history.Entries);
        }

        [Fact]
        public void Search_FindsMatchingEntries()
        {
            var history = new PromptHistory();
            history.Clear();
            history.Add("Generate REST API for users");
            history.Add("Refactor authentication code");
            history.Add("Generate unit tests");

            var results = new List<PromptEntry>(history.Search("generate"));

            Assert.Equal(2, results.Count);
            Assert.All(results, e =>
                Assert.Contains("generate", e.Prompt, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void Search_EmptyKeyword_ReturnsAllEntries()
        {
            var history = new PromptHistory();
            history.Clear();
            history.Add("A");
            history.Add("B");
            history.Add("C");

            var results = new List<PromptEntry>(history.Search(string.Empty));

            Assert.Equal(3, results.Count);
        }

        [Fact]
        public void Search_NoMatch_ReturnsEmpty()
        {
            var history = new PromptHistory();
            history.Clear();
            history.Add("Hello world");

            var results = new List<PromptEntry>(history.Search("nonexistent_xyz"));

            Assert.Empty(results);
        }
    }

    public class OrchestrationResultTests
    {
        [Fact]
        public void DefaultValues_AreNull()
        {
            var result = new OrchestrationResult();

            Assert.Null(result.Code);
            Assert.Null(result.Image);
            Assert.Null(result.Analysis);
            Assert.Null(result.Plan);
            Assert.Null(result.Suggestions);
        }

        [Fact]
        public void Properties_CanBeSetAndRead()
        {
            var imageBytes = new byte[] { 1, 2, 3 };
            var result = new OrchestrationResult
            {
                Code = "Console.WriteLine(\"Hello\");",
                Image = imageBytes,
                Analysis = "No vulnerabilities found.",
                Plan = "1. Define requirements\n2. Implement feature\n3. Write tests",
                Suggestions = "Use AutoMapper for object mapping."
            };

            Assert.Equal("Console.WriteLine(\"Hello\");", result.Code);
            Assert.Equal(imageBytes, result.Image);
            Assert.Equal("No vulnerabilities found.", result.Analysis);
            Assert.Equal("1. Define requirements\n2. Implement feature\n3. Write tests", result.Plan);
            Assert.Equal("Use AutoMapper for object mapping.", result.Suggestions);
        }

        [Fact]
        public void Plan_CanBeSetIndependently()
        {
            var result = new OrchestrationResult
            {
                Plan = "Step 1: Design API"
            };

            Assert.Equal("Step 1: Design API", result.Plan);
            Assert.Null(result.Code);
            Assert.Null(result.Suggestions);
        }

        [Fact]
        public void Suggestions_CanBeSetIndependently()
        {
            var result = new OrchestrationResult
            {
                Suggestions = "Use Polly for retry policies."
            };

            Assert.Equal("Use Polly for retry policies.", result.Suggestions);
            Assert.Null(result.Code);
            Assert.Null(result.Plan);
        }
    }
}
