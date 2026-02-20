# YaChatAI – Yandex AI Chat for Visual Studio

A Visual Studio extension (VSIX) that integrates **Yandex AI** services directly into the IDE, providing an intelligent chat interface powered by YandexGPT, YandexART, and Yandex SpeechKit.

## Features

- **AI Chat Interface** – Embedded tool window in Visual Studio
- **YandexGPT 5.1 Pro** – Code generation, refactoring, explanation, and security analysis
- **YandexART** – UML diagram and UI mockup generation from code descriptions
- **Yandex SpeechKit** – Voice input via Alice (Russian speech recognition)
- **AI Orchestrator** – Multi-step cascade pipeline: Generate → Optimize → Document → Security analysis → Visualize
- **Prompt History** – Persistent history with keyword search (stored in `%APPDATA%\YandexAIChat\`)
- **Prompt Templates** – Modes: Generation, Refactoring, Explanation, Visualization, Security Analysis

## Project Structure

```
YaChatAI/
├── src/
│   └── YandexAIChat.VSIX/
│       ├── YandexAIChat.VSIX.csproj    # VSIX project (targets .NET Framework 4.8)
│       ├── source.extension.vsixmanifest
│       ├── Package.cs                  # VS AsyncPackage entry point
│       ├── ChatToolWindow.cs           # Tool window host
│       ├── ShowChatCommand.cs          # Menu command to open chat
│       ├── ChatControl.xaml            # WPF chat UI
│       ├── ChatControl.xaml.cs         # Chat UI code-behind
│       ├── YandexAIManager.cs          # YandexGPT API client
│       ├── YandexARTManager.cs         # YandexART API client
│       ├── AIOrchestrator.cs           # Multi-agent cascade orchestrator
│       ├── OrchestrationResult.cs      # Result data model
│       ├── PromptHistory.cs            # Persistent prompt history
│       ├── VoiceInputManager.cs        # SpeechKit voice recognition
│       ├── HistoryWindow.cs            # History browser dialog
│       └── SettingsWindow.cs           # API credentials dialog
└── tests/
    └── YandexAIChat.Tests/
        ├── YandexAIChat.Tests.csproj
        └── YandexAIChatTests.cs
```

## Prerequisites

- **Visual Studio 2022** (v17.0+) with the *Visual Studio extension development* workload
- **Yandex Cloud** account with:
  - A folder ID
  - An API key with access to Foundation Models and YandexART

## Building

Open `YaChatAI.sln` in Visual Studio 2022 and press **F5** to build and launch an experimental VS instance with the extension loaded.

> **Note:** The VSIX project targets .NET Framework 4.8 and requires the Visual Studio SDK, which is only available on Windows.

## Configuration

On first launch, click the **Settings** button in the chat window and enter:

| Field | Description |
|-------|-------------|
| Folder ID | Your Yandex Cloud folder ID (e.g., `b1g8s...`) |
| API Key | Your Yandex Cloud API key |

Alternatively, set environment variables before launching Visual Studio:
```
YANDEX_FOLDER_ID=<your-folder-id>
YANDEX_API_KEY=<your-api-key>
```

## Usage

1. Open **View → Other Windows → Yandex AI Chat** (or use the toolbar button)
2. Select an interaction mode from the dropdown:
   - **Generation** – Generate code from a description
   - **Refactoring** – Optimize and improve existing code
   - **Explanation** – Explain what code does
   - **Visualization** – Generate UML diagrams via YandexART
   - **Security Analysis** – Identify vulnerabilities in code
3. Type your prompt and press **Enter** or click **Send**
4. Use **🎤 Voice** for voice input via Yandex SpeechKit
5. Browse previous prompts via **History**

## Running Tests

```bash
dotnet test tests/YandexAIChat.Tests/
```

## Architecture

```
User Input
    │
    ▼
ChatControl (WPF UI)
    │
    ▼
AIOrchestrator
    ├─→ YandexAIManager  ──→ YandexGPT 5.1 Pro API
    └─→ YandexARTManager ──→ YandexART API
```

The **AIOrchestrator** supports both single-step mode execution and a full cascade pipeline that chains multiple AI operations together.

## API Endpoints Used

| Service | Endpoint |
|---------|----------|
| YandexGPT | `https://llm.api.cloud.yandex.net/foundationModels/v1/completion` |
| YandexART | `https://llm.api.cloud.yandex.net/foundationModels/v1/imageGenerationAsync` |
| SpeechKit STT | `https://stt.api.cloud.yandex.net/speech/v1/stt:recognize` |

## License

MIT