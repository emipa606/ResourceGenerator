# GitHub Copilot Instructions for RimWorld Resource Generator Mod

## Mod Overview and Purpose
The Resource Generator Mod for RimWorld is designed to add a new dynamic to resource management by allowing players to create buildings that generate resources over time. This mod aims to enhance gameplay by providing players with a customizable and interactive way to automate resource generation, making it easier to manage colonies in the later stages of the game.

## Key Features and Systems
- **Resource Generation Building**: Introduces a new building type, `ResourceGenerator`, which produces specified resources at configurable intervals.
- **Customizable Output**: Players can select and change the type of resource the generator produces, providing flexibility in resource management strategies.
- **Dynamic Resource Management**: The mod allows for automatic spawning of resources, which can be tailored based on the player's selection and requirements.
- **Spawning Mechanics**: Uses the `CompResourceSpawner` to handle the logic of spawning resources with methods like `tryDoSpawn()` and `checkShouldSpawn()`.

## Coding Patterns and Conventions
- **Class Naming**: Follows C# naming conventions with PascalCase for class names, e.g., `CompProperties_ResourceSpawner`.
- **Method Naming**: Methods are named in camelCase, e.g., `tickInterval()` and `increaseBy()`.
- **Visibility**: Classes such as `Main` and `ResourceGenerator` are public, while mod-specific classes like `ResourceGeneratorMod` are internal, maintaining encapsulation.

## XML Integration
- **Base Definitions**: XML files define the basic properties of the resource generator, including its appearance, cost, and other in-game attributes.
- **Mod Settings**: XML entries are utilized to adjust settings that dictate how the resource generator behaves within the game environment.

## Harmony Patching
Harmony is used to apply patches necessary for integrating the resource generator's functionality into the RimWorld engine without directly modifying the game's source code. This ensures compatibility with other mods and future game updates. Key patches involve modifying spawning cycles and managing resource outputs effectively.

## Suggestions for Copilot
- **Method Autocompletion**: Utilize Copilot to suggest completions for methods within `CompResourceSpawner` and `ResourceGenerator` to streamline logic related to resource generation cycles.
- **Harmony Patching Examples**: Copilot can suggest examples for writing and applying Harmony patches, ensuring seamless integration of new features.
- **XML Template Suggestions**: Providing XML skeleton examples where Copilot can assist in automating code generation for defining new resource properties and settings.
- **Optimization Patterns**: Detect inefficient patterns in resource checks and spawning methods, suggesting potential optimizations or refactors for maintaining performance.

By following these guidelines and utilizing Copilot suggestions optimally, developers can enhance the modding experience and produce efficient, well-integrated modifications to RimWorld's resource management mechanics.
