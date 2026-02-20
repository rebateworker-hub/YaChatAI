using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace YandexAIChat
{
    /// <summary>
    /// Manages communication with Yandex AI (YandexGPT) for text-based AI tasks.
    /// </summary>
    public class YandexAIManager
    {
        private readonly string _folderId;
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        private const string TextGenerationEndpoint =
            "https://llm.api.cloud.yandex.net/foundationModels/v1/completion";

        public YandexAIManager(string folderId, string apiKey)
        {
            _folderId = folderId ?? throw new ArgumentNullException(nameof(folderId));
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));

            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(120)
            };
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Api-Key", _apiKey);
        }

        /// <summary>
        /// Sends a prompt to YandexGPT and returns the AI-generated response.
        /// </summary>
        /// <param name="prompt">The user prompt.</param>
        /// <param name="mode">Interaction mode: code, refactor, explanation, security, documentation, general.</param>
        public async Task<string> SendPrompt(string prompt, string mode)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                throw new ArgumentException("Prompt cannot be empty.", nameof(prompt));

            var modelUri = $"gpt://{_folderId}/yandexgpt-5.1-pro/latest";
            var systemMessage = GetSystemMessage(mode);

            var requestBody = new
            {
                modelUri = modelUri,
                completionOptions = new
                {
                    stream = false,
                    temperature = 0.3,
                    maxTokens = "2000"
                },
                messages = new object[]
                {
                    new { role = "system", text = systemMessage },
                    new { role = "user", text = prompt }
                }
            };

            var json = JsonConvert.SerializeObject(requestBody);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(TextGenerationEndpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(
                    $"YandexGPT API returned {(int)response.StatusCode}: {errorBody}");
            }

            var body = await response.Content.ReadAsStringAsync();
            var parsed = JObject.Parse(body);

            var text = parsed["result"]?["alternatives"]?[0]?["message"]?["text"]?.ToString();
            if (text == null)
                throw new InvalidOperationException($"Unexpected API response format: {body}");

            return text;
        }

        private static string GetSystemMessage(string mode) => mode switch
        {
            "code" =>
                "You are an expert software engineer. Generate clean, well-structured, production-ready code. " +
                "Include only the code and brief inline comments. No extra explanation unless asked.",
            "refactor" =>
                "You are an expert code reviewer. Optimize the provided code for performance, readability, " +
                "and best practices. Return only the improved code.",
            "explanation" =>
                "You are a senior developer mentor. Explain the provided code in clear, simple language. " +
                "Describe what each part does and why.",
            "security" =>
                "You are a cybersecurity expert. Analyze the code for security vulnerabilities, " +
                "injection risks, and bad practices. List each issue with severity and recommended fix.",
            "documentation" =>
                "You are a technical writer. Add clear, helpful comments and documentation to the code. " +
                "Use the language's standard documentation style (XML docs for C#, JSDoc for JS, etc.).",
            "planning" =>
                "You are a software architect and project planner. Create a structured development plan " +
                "for the described feature or project. Include: goal summary, breakdown of tasks with priorities, " +
                "suggested architecture and file structure, and milestones. Use numbered lists and clear headings.",
            "bugfix" =>
                "You are an expert debugger. Analyze the provided code or error description, identify all bugs, " +
                "warnings, and potential issues. For each issue state: location, root cause, severity, and the " +
                "corrected code snippet. Also explain how to prevent similar issues in the future.",
            "suggest" =>
                "You are a senior software architect. Given the problem description, recommend the best tools, " +
                "libraries, design patterns, and implementation methods. Explain why each is appropriate, " +
                "provide short usage examples, and generate a reusable helper method or utility tailored to the task.",
            _ =>
                "You are a helpful AI assistant specialized in software development."
        };
    }
}
