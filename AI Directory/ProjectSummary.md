# Project: Flowerscapes - LLM Context Guide

**Last Updated:** 2025-05-10

## 1. Project Overview

*   **Project Name:** Flowerscapes
*   **Game Genre:** Likely a puzzle, matching, or board-style game with a thematic element related to flowers or gardens. (Inferred from names like `PiecesController` and project title).
*   **Target Platform(s):** WebGL (inferred from `OnApplicationQuit` handling in `Main.cs`) & Mobile (iOS/Android)
*   **Core Concept:** A game involving the manipulation of "pieces," possibly related to cultivating or arranging flowers to create landscapes.
*   **Current Development Stage:** Prototyping

## 2. Core Technologies

*   **Unity Version:** 6000.1.1f1
*   **C# Version:** Roslyn C# 9.0, .NET Standard 2.1, .NET Framework 4.8
*   **Rendering Pipeline:** Built-in
*   **Key Unity Packages in Use:**
    *   Unity's New Input System (evidenced by `InputSystemUIInputModule`, `EnhancedTouchSupport`)
    *   Unity Services (initialized in `Main.cs`)
    *   TextMeshPro
    *   Addressables (managed by `AssetResolverService`)
*   **Version Control:** Git
    *   **Commit Message Convention:** Conventional Commits (https://www.conventionalcommits.org/en/v1.0.0/)

## 3. Project Structure (Key Directories)

*   `Assets/Scripts/`: Main location for all C# game logic. Organized into the following subfolders:
    *   `Assets/Scripts/Commands/`: For implementing the Command pattern, encapsulating actions or requests.
    *   `Assets/Scripts/Configs/`: ScriptableObjects or classes for game configurations, possibly complementing `Assets/Settings`.
    *   `Assets/Scripts/Controllers/`: For managing specific game entities, systems, or complex behaviours (e.g., `PiecesController`).
    *   `Assets/Scripts/Data/`: Data structures, models, or classes holding runtime game data.
    *   `Assets/Scripts/Logic/`: Core game rules, algorithms, and calculations.
    *   `Assets/Scripts/Messages/`: Classes defining event types/payloads for the `MessageBrokerService`.
    *   `Assets/Scripts/Presenters/`: Mediators between Views (UI) and Models (Data/Logic) in MVP/MVC patterns.
    *   `Assets/Scripts/Services/`: For singleton-like classes providing global functionalities, like those in Section 5.
    *   `Assets/Scripts/StateMachines/`: Implementations of state-based behaviours for game entities or systems.
    *   `Assets/Scripts/ViewControllers/`: Controllers specifically for UI views, managing UI state and interactions.
    *   `Assets/Scripts/Views/`: Scripts attached to UI elements or GameObjects responsible for visual representation.
    *   `Assets/Scripts/Editor/`: Custom editor scripts, tools, and inspectors to extend Unity Editor functionality.
*   `Assets/Prefabs/`: For all reusable GameObjects. (Standard practice)
*   `Assets/Scenes/`: Game scenes configured in the Project Settings. (Standard practice)
*   `Assets/Art/`: Source art assets (models, textures, sprites). (Standard practice)
*   `Assets/Audio/`: Sound effects and music. (Standard practice)
*   `Assets/Resources/`: Requried by some plugins(DO NOT USE THIS FOLDER; prefer Addressables or `AssetResolverService`).
*   `Assets/Settings/`: For ScriptableObjects for configuration, game data (e.g., managed by `ConfigsProvider`).
*   `Assets/Libs/`: Third-party libraries not managed by UPM, mainly installed from Unity Asset Store
*   `Library/PackageCache/`: Unity Package Manager controlled libraries.

## 4. Architecture & Coding Conventions

*   **Primary Architectural Patterns:**
    *   **Service Locator / Dependency Injection:** `Main.cs` initializes and manages services via an `Installer` class.
    *   **State Machine:** `GameStateMachine` (initialized and run from `Main.cs`) manages game flow and states.
    *   **Locator Pattern:** `GameLogicLocator` (for core game logic modules) and `GameServicesLocator` (for initialized game services).
    *   **Component-Based Architecture:** (Unity's default).
    *   **Modular Script Organization (`Assets/Scripts`):** Scripts are well-organized into folders like `Commands`, `Configs`, `Controllers`, `Data`, `Logic`, `Messages`, `Presenters`, `Services`, `StateMachines`, `ViewControllers`, and `Views` to promote maintainability and scalability.
    *   **Entry Point (`Main.cs`):** Acts as the primary entry point after Unity loads. `Awake()` sets up services/locators. `Start()` (via `OnStart().Forget()`) kicks off async service initialization and the `GameStateMachine`, also handling global app events and critical operations like data saving.
*   **Script Communication:**
    *   **Event-Driven (Inter-Module):** Primarily through `MessageBrokerService` for communication between different high-level modules.
    *   **Intra-Module/Prefab Communication:** Unity Events are preferred for communication between scripts within the same module or between Prefab instances.
    *   **Method Parameters:** `Func` and `Action` delegates are used for passing methods as parameters.
    *   **Component Access (Same GameObject):** `GetComponent<>` is appropriate for accessing components attached to the *same* GameObject and should be assigned to a private `[SerializeField]` field in the inspector or in code via the `OnValidate` method.
    *   **To Avoid:** Direct script references across different modules and excessive use of `GetComponent<>` to find components on *other* GameObjects are generally discouraged to promote decoupling. Favor event-driven or delegate-based approaches for broader communication.
*   **Data Management:**
    *   **Configurations:** Managed by `ConfigsProvider`.
    *   **Data Persistence (Saving/Loading):** Handled by `DataService`.
    *   **ScriptableObjects:** Likely used for data containers and configurations (best practice, align with `ConfigsProvider`).
    *   **Serialization:** Newtonsoft.Json library
*   **Scene Management:**
    *   Additive scene loading with `AssetResolverService` via addressables system
*   **Naming Conventions:**
    *   **Classes, Enums, Public Members, Methods:** PascalCase (`MyClass`, `PlayerHealth`)
    *   **Private/Protected Fields:** `_camelCase` (e.g., `_myPrivateField`)
    *   **Interfaces:** `IMyInterface`
    *   **Constants:** ALL_CAPS_SNAKE_CASE (`MAX_HEALTH`)
*   **Asynchronous Operations:**
    *   **Coroutines:** Managed by `CoroutineService`.
    *   **UniTasks:** Prefer for new C# async operations (as per user memory/best practice).
*   **Error Handling:**
    *   Unobserved task exceptions are caught and logged via `AnalyticsService`.
    *   Use `try-catch` for file I/O, network operations.
    *   Use `Debug.Log/Warning/Error` for logging.
    *   Use `Debug.Assert` for logical errors during development.
*   **Variable Declaration:** Use explicitly-typed local variables (as per user memory).

## 5. Key Game Systems & Services (Brief Descriptions)

*   **`Main.cs` (Bootstrapper & Application Lifecycle):**
    *   Initializes Unity Services and loads version data.
    *   Sets up core services (via `Installer`) and the `GameStateMachine`.
    *   Configures screen sleep timeout and enables Unity's new Input System (`EnhancedTouchSupport`).
    *   Handles `OnApplicationFocus`, `OnApplicationPause`, and `OnApplicationQuit` for data saving (`DataService`), analytics flushing (`AnalyticsService`), and application state management (including a 30-second pause timeout). Specific WebGL `OnApplicationQuit` handling.
*   **`Installer` (within `Main.cs` context):** Responsible for setting up and injecting dependencies for various services.
*   **`GameStateMachine`:** Manages the game's overall flow and different states (e.g., main menu, gameplay, game over).
*   **`GameLogicLocator`:** Provides centralized access to core game logic modules.
*   **`GameServicesLocator`:** Provides centralized access to initialized game services.

*   **Services Initialized by `Installer`:**
    *   `MessageBrokerService`: For event-driven communication between different parts of the game.
    *   `TimeService`: For managing game-related time, deltas, or custom time scaling.
    *   `GameUiService`: Handles UI elements, interactions, and likely UI state. Uses a `UiAssetLoader` (suggesting dynamic UI loading).
    *   `PoolService`: For object pooling to optimize performance by reusing frequently instantiated objects.
    *   `TickService`: Provides a centralized way to manage frame-based updates for game logic.
    *   `CoroutineService`: Manages and provides a controlled way to run coroutines.
    *   `AssetResolverService`: For loading and managing game assets (potentially an abstraction over Addressables or Resources).
    *   `ConfigsProvider`: For accessing game configurations and settings.
    *   `DataService`: Handles data persistence, including saving and loading all game data.
    *   `AnalyticsService`: Integrates analytics for tracking game events, player behavior, and errors. Handles logging of unobserved task exceptions.
*   **Gameplay Core (Inferred from `PiecesController`):**
    *   `PiecesController`: Suggests a core gameplay mechanic involving the manipulation or management of "pieces" on a board or in a puzzle.
*   **Input Management System:**
    *   Utilizes Unity's new Input System (e.g., `InputSystemUIInputModule`, `EnhancedTouchSupport` enabled in `Main.cs`).

## 6. Asset Management

*   **Primary Loading Mechanism:** `AssetResolverService`, which utilizes the Unity Addressables system for dynamic asset loading and management. Direct use of `Resources` folders is discouraged.
*   **UI Assets:** Loaded via `UiAssetLoader` (used by `GameUiService`).
*   **Object Pooling:** Handled by `PoolService`.
*   **Prefabs:** Extensively used for reusable GameObjects. (Standard Unity practice).
*   **Addressables:** Recommended as the primary method for dynamic asset loading (align with `AssetResolverService` if applicable).
*   **Tags and Layers:** Used for object categorization and collision filtering. (Standard Unity practice).

## 7. Important Third-Party Libraries/SDKs

*   **Unity Packages:**
    *   Addressables (`com.unity.addressables`): For advanced asset management.
    *   Input System (`com.unity.inputsystem`).
    *   TextMeshPro (`com.unity.textmeshpro`): For rich text rendering.
    *   UI (UGUI) (`com.unity.ugui`).
    *   And many others for core engine functionalities, testing, and editor integration.
*   **Third-Party Libraries (`Assets/Libs` & `PackageCache`):**
    *   `com.cysharp.unitask`: For asynchronous operations.
    *   `Demigiant` (likely DOTween): For animations and tweening.
    *   Analytics SDKs: `Aptabase`, `Mixpanel`.
*   **Internal/Custom Packages (`com.gamelovers.*`):**
    *   A suite of packages prefixed with `com.gamelovers`, suggesting shared internal libraries for common functionalities like configuration, data extensions, Google Sheet importing, services, state charts, and UI services. This points to a mature development pipeline.

## 8. Build & Deployment

*   **Known Platforms:** WebGL & Mobile (iOS/Android)

## 9. LLM Interaction Guidelines (Specific to this Project)

*   "When creating new scripts, place them in the appropriate subfolder within `Assets/Scripts/` as outlined in **Section 3**."
*   "Prioritize using ScriptableObjects (via `ConfigsProvider` or directly in `Assets/Settings/`) for configuration data over hardcoding values."
*   "Ensure all new public methods have XML documentation comments for clarity."
*   "If unsure about an architectural decision, ask for clarification based on existing patterns (e.g., 'Should this new system be a service managed by the `Installer`? How should it interact with the `MessageBrokerService`?')."
*   "Adhere to the Naming Conventions outlined in **Section 4**."
*   "For asynchronous operations, prefer UniTasks for new C# code; use `CoroutineService` for Unity-specific lifecycle coroutines."
*   "Leverage existing services (see **Section 5**) before creating new ones with overlapping functionality."
*   "All significant game events, player actions, and errors should be logged via `AnalyticsService`."