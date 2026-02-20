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
    /// Manages communication with YandexART for image and diagram generation.
    /// </summary>
    public class YandexARTManager
    {
        private readonly string _folderId;
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        private const string ImageGenerationEndpoint =
            "https://llm.api.cloud.yandex.net/foundationModels/v1/imageGenerationAsync";

        private const string OperationEndpoint =
            "https://llm.api.cloud.yandex.net/operations/";

        public YandexARTManager(string folderId, string apiKey)
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
        /// Generates an image from a text description using YandexART.
        /// Returns raw PNG bytes.
        /// </summary>
        public async Task<byte[]> GenerateImage(string prompt, int width = 1024, int height = 1024)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                throw new ArgumentException("Prompt cannot be empty.", nameof(prompt));

            var requestBody = new
            {
                modelUri = $"art://{_folderId}/yandex-art/latest",
                generationOptions = new
                {
                    seed = new Random().Next(1, 999999).ToString(),
                    aspectRatio = new
                    {
                        widthRatio = width,
                        heightRatio = height
                    }
                },
                messages = new[]
                {
                    new
                    {
                        weight = "1",
                        text = prompt
                    }
                }
            };

            var json = JsonConvert.SerializeObject(requestBody);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(ImageGenerationEndpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(
                    $"YandexART API returned {(int)response.StatusCode}: {errorBody}");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var operationId = JObject.Parse(responseBody)["id"]?.ToString();
            if (string.IsNullOrEmpty(operationId))
                throw new InvalidOperationException("No operation ID returned from YandexART.");

            return await PollForImageResult(operationId);
        }

        private async Task<byte[]> PollForImageResult(string operationId)
        {
            var url = OperationEndpoint + operationId;
            const int maxAttempts = 30;
            const int delayMs = 2000;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                await Task.Delay(delayMs);

                var response = await _httpClient.GetAsync(url);
                var body = await response.Content.ReadAsStringAsync();
                var parsed = JObject.Parse(body);

                var done = parsed["done"]?.ToObject<bool>() ?? false;
                if (!done)
                    continue;

                var error = parsed["error"];
                if (error != null)
                    throw new InvalidOperationException($"YandexART generation failed: {error}");

                var imageBase64 = parsed["response"]?["image"]?.ToString();
                if (string.IsNullOrEmpty(imageBase64))
                    throw new InvalidOperationException("No image data in completed operation.");

                return Convert.FromBase64String(imageBase64);
            }

            throw new TimeoutException("YandexART image generation timed out.");
        }

        /// <summary>
        /// Generates a UML diagram from a code snippet.
        /// </summary>
        public async Task<byte[]> GenerateUMLDiagram(string code, string diagramType = "class")
        {
            var prompt = $"Create a professional {diagramType} UML diagram for the following code. " +
                         "Use a clean, minimalistic black-and-white style. " +
                         "Show classes, methods, attributes, and relationships clearly.\n\n" +
                         $"Code:\n{code}";

            return await GenerateImage(prompt, 800, 600);
        }

        /// <summary>
        /// Generates a UI mockup from a description.
        /// </summary>
        public async Task<byte[]> GenerateUI(string description)
        {
            var prompt = $"Create a modern application UI design based on this description: {description}. " +
                         "Use Material Design style. Clean layout with blue, white, and gray colors.";

            return await GenerateImage(prompt, 1200, 800);
        }
    }
}
