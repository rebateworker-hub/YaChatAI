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

        /// <summary>Development plan or structured documentation outline.</summary>
        public string? Plan { get; set; }

        /// <summary>Suggested tools, methods, or approaches for the given problem.</summary>
        public string? Suggestions { get; set; }
    }
}
