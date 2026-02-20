namespace YandexAIChat
{
    /// <summary>
    /// Holds the result of an AI orchestration pipeline execution.
    /// </summary>
    public class OrchestrationResult
    {
        /// <summary>Generated or processed code text.</summary>
        public string? Code { get; set; }

        /// <summary>Generated image bytes (e.g. UML diagram from YandexART).</summary>
        public byte[]? Image { get; set; }

        /// <summary>Security or code analysis text.</summary>
        public string? Analysis { get; set; }
    }
}
