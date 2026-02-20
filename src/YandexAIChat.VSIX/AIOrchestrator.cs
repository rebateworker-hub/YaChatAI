using System;
using System.Threading.Tasks;

namespace YandexAIChat
{
    /// <summary>
    /// Orchestrates chains of AI agent calls (YandexGPT + YandexART).
    /// Supports single-step and multi-step cascade processing pipelines.
    /// </summary>
    public class AIOrchestrator
    {
        private readonly YandexAIManager _aiManager;
        private readonly YandexARTManager _artManager;

        public AIOrchestrator(YandexAIManager aiManager, YandexARTManager artManager)
        {
            _aiManager = aiManager ?? throw new ArgumentNullException(nameof(aiManager));
            _artManager = artManager ?? throw new ArgumentNullException(nameof(artManager));
        }

        /// <summary>
        /// Executes a single-mode AI task.
        /// </summary>
        public async Task<OrchestrationResult> ExecuteOrchestration(string prompt, string mode)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                throw new ArgumentException("Prompt cannot be empty.", nameof(prompt));

            var result = new OrchestrationResult();

            switch (mode)
            {
                case "code":
                    result.Code = await _aiManager.SendPrompt(prompt, "code");
                    break;

                case "refactor":
                    result.Code = await _aiManager.SendPrompt(
                        $"Refactor and optimize the following code:\n\n{prompt}",
                        "refactor");
                    break;

                case "explanation":
                    result.Code = await _aiManager.SendPrompt(prompt, "explanation");
                    break;

                case "security":
                    result.Analysis = await _aiManager.SendPrompt(prompt, "security");
                    break;

                case "visualization":
                    result.Image = await _artManager.GenerateUMLDiagram(prompt);
                    break;

                default:
                    result.Code = await _aiManager.SendPrompt(prompt, "general");
                    break;
            }

            return result;
        }

        /// <summary>
        /// Runs the full cascade pipeline:
        /// Generate → Optimize → Visualize → Document → Security analysis.
        /// </summary>
        public async Task<OrchestrationResult> CascadeGenerate(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                throw new ArgumentException("Prompt cannot be empty.", nameof(prompt));

            var result = new OrchestrationResult();

            // Step 1: Generate initial code
            result.Code = await _aiManager.SendPrompt(prompt, "code");

            // Step 2: Optimize for performance
            result.Code = await _aiManager.SendPrompt(
                $"Optimize the following code for performance and best practices:\n\n{result.Code}",
                "refactor");

            // Step 3: Add documentation comments
            result.Code = await _aiManager.SendPrompt(
                $"Add comprehensive documentation comments to the following code:\n\n{result.Code}",
                "documentation");

            // Step 4: Security analysis
            result.Analysis = await _aiManager.SendPrompt(
                $"Analyze the following code for security vulnerabilities:\n\n{result.Code}",
                "security");

            // Step 5: Generate UML diagram
            result.Image = await _artManager.GenerateUMLDiagram(result.Code);

            return result;
        }
    }
}
