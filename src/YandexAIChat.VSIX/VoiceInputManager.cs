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
    /// Handles voice input using Yandex SpeechKit for speech recognition.
    /// </summary>
    public class VoiceInputManager
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        private const string SpeechKitEndpoint =
            "https://stt.api.cloud.yandex.net/speech/v1/stt:recognize";

        public VoiceInputManager(string apiKey)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Api-Key", _apiKey);
        }

        /// <summary>
        /// Recognizes speech from the system microphone and returns transcribed text.
        /// </summary>
        public async Task<string> RecognizeAsync()
        {
            var audioBytes = await RecordAudioAsync();
            if (audioBytes == null || audioBytes.Length == 0)
                throw new InvalidOperationException("No audio captured from microphone.");

            return await SendToSpeechKitAsync(audioBytes);
        }

        /// <summary>
        /// Recognizes speech from a pre-recorded audio byte array (OGG/Opus format).
        /// </summary>
        public async Task<string> RecognizeFromBytesAsync(byte[] audioBytes)
        {
            if (audioBytes == null || audioBytes.Length == 0)
                throw new ArgumentException("Audio bytes cannot be empty.", nameof(audioBytes));

            return await SendToSpeechKitAsync(audioBytes);
        }

        private async Task<string> SendToSpeechKitAsync(byte[] audioBytes)
        {
            var url = $"{SpeechKitEndpoint}?lang=ru-RU&format=oggopus&sampleRateHertz=16000";

            using var content = new ByteArrayContent(audioBytes);
            content.Headers.ContentType = new MediaTypeHeaderValue("audio/ogg");

            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(
                    $"SpeechKit API returned {(int)response.StatusCode}: {errorBody}");
            }

            var body = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(body)["result"]?.ToString();

            if (string.IsNullOrWhiteSpace(result))
                throw new InvalidOperationException("SpeechKit returned an empty transcription.");

            return result;
        }

        /// <summary>
        /// Records audio from the system microphone.
        /// Returns raw PCM/OGG bytes suitable for SpeechKit.
        /// </summary>
        private static Task<byte[]> RecordAudioAsync()
        {
            // Microphone recording requires platform-specific APIs (NAudio on Windows).
            // This method serves as the integration point for audio capture.
            // In a real implementation, use NAudio or System.Speech to capture audio.
            throw new NotSupportedException(
                "Microphone recording requires the NAudio package and Windows platform. " +
                "Please use RecognizeFromBytesAsync() to provide audio data directly.");
        }
    }
}
