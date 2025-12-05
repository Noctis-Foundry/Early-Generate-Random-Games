Early-Generate-Random-Games — Documentation

## Architecture

   - Program Layer — application entry point, service initialization, startup logic.

   - UI Layer — Avalonia windows, pages, and visual components.

   - Systems — core project logic, services, data flow, background operations.

   - Tools — utility modules, parsers, supporting components.

## Technologies Used

   - Avalonia UI
   - .NET SDK 10
   - PostgreSQL
   - JSON
   - async/await
   - Steamworks SDK
   - Steam Web API

## Systems

3.1 Database System
   - Full CRUD operations.
   - Event listening using PostgreSQL LISTEN/NOTIFY.
   - Callback system triggered by database table changes.

3.2 DI Container
   - Singleton instances for project-wide services.
   - Dependency injection via custom attributes and reflection.
   - Lightweight DI solution tailored for the project.

3.3 AvaloniaService
   - Converts shared types to Avalonia-compatible types.
   - Example: converting images into Avalonia Bitmap.
   - Provides helpers for UI-thread operations.

3.4 Steam Web Service
   - Handles communication with the Steam Web API.
   - Performs GET requests to fetch player and app information.
   - Supports resolving user data by CSteamID.

3.5 Steamworks Initialization
   - Initializes Steamworks and establishes a local Steam session.
   - Validates that Steam client is running.
   - Displays an error window if initialization fails.

3.6 Error Service
   - Centralized error/message handling for the entire application.
   - Five message levels: Error, Info, Message, Warning, Critical.
   - Two display modes:
   - Low-level MessageBox (from external solution "MessageBox")
   - High-level error window after app initialization

## Tools

4.1 SteamParser
   - Uses a local JSON file with AppIDs.
   - Fetches game information from the Steam Store.
   - Processes game metadata (name, icon, description).

## Core Logic

5.1 Roll Game Window
   - Loads parsed game list.
   - Randomly selects 1–5 games.
   - Displays selection to the user.
   - Saves chosen game(s) into the database.

5.2 Profile Window
   - Shows user avatar and nickname.
   - Displays the user’s current games table.

5.3 Lobby System
   - Functions: Create / Connect / Disconnect.
   - Reads and updates lobby state through the database.
   - Synchronizes lobby data between players.

5.4 Rules

   - Displays challenge rules.
   - Provides basic logic for rule validation.

5.5 Main Window

   - Main application window, containing navigation to user content.
   - Sections: Roll Game, Profile, Game Table.

5.6 Game Table

   - Displays all lobby game entries.
   - Shows and updates processing states.

5.7 Error Window

   - Displays all errors and logs from the Error Service.
   - Supports filtering by message level.

## Code Base Map

A full map of the codebase is located in the Documentation directory.
Each system is documented in a separate file.