# Flowerscapes - A Flower-Sorting puzzle game

Flowerscapes is a Unity-based game, likely centered around puzzle mechanics involving sorting and matching flower petals. 

*(Placeholder: Add a compelling screenshot or GIF of the gameplay here)*


## Playable Build

You can play - [Flowerscapes on Unity Play](https://play.unity.com/en/games/1c5441a1-1695-4678-a5dd-03066fd25946/build)

## Getting Started (For Developers)

### Prerequisites

*   **Unity Hub** installed.
*   **Unity Editor version `6000.1.1f1`** (or compatible) installed.

### Setup

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/Tropa-Elite/Flowerscapes.git
    cd Flowerscapes
    ```
2.  **Open in Unity Hub:**
    *   Open Unity Hub.
    *   Click "Add" or "Open".
    *   Navigate to the cloned `Flowerscapes` directory and select it.
    *   Unity should automatically resolve the packages listed in `Packages/manifest.json`. If you encounter issues, try restarting Unity.
3.  **Open the Boot Scene:**
    * The main entry point of the project is `Assets/Scenes/Boot.unity` scene. You can automatically open it by CMD+1 (Mac) or ALT+1 (Windows).

## Commands

The following custom editor shortcuts are available under the "Tools/Scene" menu:

*   **Force Script Reload (`Alt+R` or `Cmd+R`):** Requests a manual reload of scripts in the editor.
*   **Open Boot Scene (`Alt+1` or `Cmd+1`):** Opens the `Assets/Scenes/Boot.unity` scene.
*   **Open Main Scene (`Alt+2` or `Cmd+2`):** Opens the `Assets/Scenes/Main.unity` scene.
*   **Open First Scene (`Alt+3` or `Cmd+3`):** Opens the first scene asset found in the project (order determined by Project Settings configuration).

These commands are defined in `Assets/Scripts/Editor/EditorTools/EditorShortcuts.cs`.

## Technical Overview

*   **Core Architecture Principles:**
    *   **Service Locator / Dependency Injection:** Centralized management of game services via an `Installer`.
    *   **State Machine:** `GameStateMachine` controls the overall game flow and states.
    *   **Modular Design:** Scripts are organized into logical folders (e.g., `Commands`, `Controllers`, `Services`, `Logic`) promoting separation of concerns.
    *   **Platform Support:** Includes considerations for iOS (ATT) and WebGL.
*   **Key Technologies & Libraries:**
    *   **Unity Input System:** Utilizes Unity's new Input System.
    *   **UniTask (`com.cysharp.unitask`):** For enhanced and efficient asynchronous programming.
    *   **Demigiant DOTween (likely):** For powerful animation and tweening.
    *   **TextMeshPro:** For advanced text rendering.
    *   **Unity Addressables:** For efficient asset management.
    *   **Analytics SDKs:** Aptabase, Mixpane for detailed player behavior tracking and error reporting.
    *   **Internal `com.gamelovers.*` Packages:** A suite of custom packages for configuration, data extensions, Google Sheet importing, shared services, state charts, and UI services, indicating a mature development pipeline.

## Project Structure Highlight

The main application entry point is `Assets/Scripts/Main.cs`.
The core C# scripts are located in `Assets/Scripts` and are organized into subdirectories reflecting their roles:

*   **`Cheats`**: Contains scripts for implementing cheat codes or debugging functionalities for development and testing purposes.
*   **`Commands`**: Implements the command pattern, encapsulating actions and requests as objects. This helps in decoupling senders and receivers of requests.
*   **`Configs`**: Stores game configuration data, often as `ScriptableObject` assets or classes that define game settings, balance parameters, or feature toggles.
*   **`Controllers`**: Houses scripts that manage specific game systems, GameObject behaviors, or coordinate interactions between different game components (e.g., `PiecesController`).
*   **`Data`**: Contains data structures, data containers (like `ScriptableObjects`), and scripts related to data serialization, persistence, and management.
*   **`Editor`**: Holds scripts that extend the Unity Editor, such as custom inspectors, editor windows, utility tools, and build scripts (e.g., `EditorShortcuts.cs`). These scripts are not included in the final game build.
*   **`Ids`**: Contains scripts for managing unique identifiers for assets, game objects, data entries, or other uniquely identifiable elements within the game.
*   **`Logic`**: Contains the core game logic, algorithms, rules, and business logic that define how the game operates, independent of presentation or services.
*   **`Messages`**: Defines message or event types used by the `MessageBrokerService` for decoupled communication between different parts of the game.
*   `Main.cs`: The primary MonoBehaviour and entry point for the game, responsible for initializing services, the game state machine, and handling application lifecycle events.
*   **`Presenters`**: In architectures like MVP or MVVM, these scripts act as intermediaries between the Views (UI) and the Models/Logic, formatting data for display and handling user input.
*   **`Services`**: Contains implementations for various services providing specific functionalities to the game (e.g., `DataService`, `AnalyticsService`, `TimeService`, `PoolService`).
*   **`StateMachines`**: Implements state machine patterns to manage game states, character states, or UI flows.
*   **`Utils`**: A collection of utility scripts providing helper functions, common tools, or extension methods used across various parts of the project.
*   **`ViewControllers`**: Scripts that manage UI views, their presentation logic, and interactions with underlying UI elements and services.
*   **`Views`**: Contains scripts directly controlling visual aspects of the game, often UI elements, or components that render game objects.

## Contributing

Contributions are welcome! If you'd like to contribute, please follow the existing code style and consider opening an issue first to discuss your proposed changes.

## License

This project is licensed under the **Creative Commons Attribution-NonCommercial 4.0 International License (CC BY-NC 4.0)**.

This means you are free to:
*   **Share** — copy and redistribute the material in any medium or format.
*   **Adapt** — remix, transform, and build upon the material.

Under the following terms:
*   **Attribution** — You must give appropriate credit, provide a link to the license, and indicate if changes were made. You may do so in any reasonable manner, but not in any way that suggests the licensor endorses you or your use.
*   **NonCommercial** — You may not use the material for commercial purposes.

There are no additional restrictions — You may not apply legal terms or technological measures that legally restrict others from doing anything the license permits.

For the full license text, please see: [Creative Commons Attribution-NonCommercial 4.0 International License](https://creativecommons.org/licenses/by-nc/4.0/legalcode)

It is recommended to include a copy of the `LICENSE` file (e.g., from the link above) in the root of your repository.
