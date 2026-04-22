# GitHub Copilot Instructions

## Mod Overview and Purpose

**Resource Generator** is a mod for RimWorld that introduces a new type of building called the "Resource Generator." It aims to provide players with a reliable method of generating various resources over time. This mod is inspired by Atysoona's original Resource Generators mod, updating and expanding its functionality for the current game version.

## Key Features and Systems

- **Resource Generators:** Generate resources based on their market value, producing items equivalent to 75x the value of Steel every two days with adequate power supply.
  
- **Dynamic Resource Management:** Generators can be set to stop production when sufficient quantities of the resource exist on the map.

- **Resource Diversity:** Capable of generating all stackable, minable, or stuff-type items, including metals, stones, textiles, and additional resources introduced by other mods.

- **Advanced Generators:** Offer more efficient resource generation with enhanced capabilities.

- **Fueled Generators:** A less efficient variant that operates without the resource limiter.

- **Multilingual Support:** Includes translations for Chinese (by asavikle, updated by CNCG779) and Russian (by AveAcVale).

## Coding Patterns and Conventions

- Follow a consistent naming convention using PascalCase for class names (e.g., `ResourceGenerator`, `CompResourceSpawner`).
- Method names are also in PascalCase, and private helper methods typically start in lowercase.
- Organize code into methods with specific responsibilities to improve readability and maintainability.
- Use structured class definitions to encapsulate different components and their functionalities in the game.

## XML Integration

- XML files define item attributes, building designs, and configurations for use within the game. Ensure all resources generated are properly declared in XML files to enable seamless integration and interaction with game mechanics.

## Harmony Patching

- Use Harmony to patch existing game functionality if you need to alter or extend base game behavior safely.
- Ensure all patches are non-destructive and reversible, respecting the original game logic as much as possible.
- Document Harmony patches with appropriate comments to explain their purpose and usage.

## Suggestions for Copilot

- **Context-aware Assistance:** When working on a new feature, provide ample context in comments and variable names to allow Copilot to understand your intent and offer relevant suggestions.
  
- **Refactoring and Optimization:** Use Copilot to help identify potential areas for code optimization and refactoring, especially in areas concerning resource calculation and spawning logic.

- **Internationalization Tips:** Suggest methods to integrate additional translations or simplify current multilingual support processes.

- **Test-Driven Development:** While implementing new features, guide Copilot by integrating unit test stubs, helping automate repetitive test case generation.

- **Event-Driven Suggestions:** Leverage Copilot to propose event-driven architectures, especially when dealing with UI interactions or resource threshold triggers.

Utilizing GitHub Copilot as a tool will enhance efficiency and maintain code quality throughout the development lifecycle of the Resource Generator mod.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.
