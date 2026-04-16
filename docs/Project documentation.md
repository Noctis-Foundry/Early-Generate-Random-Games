# Project Documentation

## Core Systems

### Dependency Injection
Custom lightweight DI system managing service lifetimes and automatic dependency resolution. Supports singleton registration, field injection via [Inject] attribute, and flexible initialization with DiFactory.

[Detailed Documentation](ClassesAndSystems/DependencyInjection.md)

### UserControlFactory
Factory for creating user control instances with navigation callback injection. Ensures consistent initialization of IUserControl implementations with type-safe instantiation.

[Detailed Documentation](ClassesAndSystems/UserControlFactory.md)

### IUserControl
Interface defining contract for user controls with navigation support and lifecycle management (AddListener, Open, Close).

[Detailed Documentation](ClassesAndSystems/IUserControl.md)

### EventBus
Lightweight publish-subscribe event system for decoupled component communication. Enables type-safe event handling with dynamic subscription management.

[Detailed Documentation](ClassesAndSystems/EventBus.md)

### ErrorService
Modal error dialog service with queuing and global exception handling. Displays errors sequentially with thread-safe UI updates.

[Detailed Documentation](ClassesAndSystems/ErrorService.md)

### ConfirmService
Thread-safe confirmation dialog service for user decision prompts. Prevents multiple simultaneous dialogs and provides Yes/No confirmation interface.

[Detailed Documentation](ClassesAndSystems/ConfirmService.md)

## UI Systems

### Main Window
Primary application window managing navigation, lobby integration, and global UI state. Features top menu bar with lobby avatars, preloaded page system, and event-driven updates.

[Detailed Documentation](ClassesAndSystems/MainWindow.md)

### RollGame System
Random game selection system with filtering capabilities. Generates 1-5 random games from Steam library with optional category, genre, and year filters using MVVM pattern.

[Detailed Documentation](ClassesAndSystems/RollGame.md)

### Filter Game System
Game filtering system for configuring selection criteria by categories, genres, and release years. Loads filter options from JSON files and provides data structure for filter validation.

[Detailed Documentation](ClassesAndSystems/FilterGameSystem.md)

### Current Game Status System
Real-time game session tracking system displaying current game information with automatic database synchronization. Uses PostgreSQL listener for live updates and timer-based countdown display.

[Detailed Documentation](ClassesAndSystems/CurrentGameStatusSystem.md)

### Game Table System
Real-time game progress table displaying completion status for all players in current lobby. Features automatic updates via PostgresListener, lobby-based filtering, and abstract base class architecture.

[Detailed Documentation](ClassesAndSystems/GameTableSystem.md)

### Lobby System
Lobby management system for multiplayer game sessions. Groups players into isolated lobbies with automatic member tracking, event-driven updates, and real-time lobby ID display.

[Detailed Documentation](ClassesAndSystems/LobbyService.md)

## ViewModel Architecture

### MVVM Pattern
All ViewModels follow MVVM pattern with separated business logic. ViewModels are organized in system-specific folders with Interface/ subdirectories and service action classes.

### Base Classes
- **ViewModelBase** - Base class for all ViewModels with INotifyPropertyChanged
- **BaseModelService** - Base class for service action classes
- **AbstractTableWindowViewModel<T>** - Abstract base for table ViewModels

### System Structure
Each ViewModel system contains:
- **Interface/** - Interface definitions (e.g., IAdminConfirm, IChooseGame)
- **[System]Actions.cs** - Service class implementing business logic
- **[System]ViewModel.cs** - ViewModel for UI binding and state management

### Implemented Systems
- AdminConfirmSystem - Game completion approval workflow
- ChooseGameSystem - Game selection and database persistence
- ConfirmFinishGameSystem - Game completion with screenshot/comment
- CurrentGameStatusSystem - Real-time game session tracking
- FilterGameSystem - Game filter configuration
- LobbySystem - Lobby management and display
- RollGameViewModel - Random game generation
- StatisticSystem - Player statistics display
- TablesSystem - Game progress tables

## Global Styles
Application-wide XAML style definitions for consistent UI appearance.

### Button Style
Global button styling with Rye-Font, size 16, dark gray text on light gray background.

[Detailed Documentation](ClassesAndSystems/GlobalStyles/ButtonStyle.md)

### CheckBox Style
Comprehensive checkbox styling with animated gradient borders, rounded corners, and state-specific appearances using Anime Ace font.

[Detailed Documentation](ClassesAndSystems/GlobalStyles/CheckBoxStyle.md)

### MenuItem Style
Menu item styling with WhiteSmoke text and DarkSeaGreen hover effect using Anime Ace font.

[Detailed Documentation](ClassesAndSystems/GlobalStyles/MenuItemStyle.md)

### TextBlock Style
Global text styling with Rye-Font and dark gray color for all TextBlock controls.

[Detailed Documentation](ClassesAndSystems/GlobalStyles/TextBlockStyle.md)

## Navigation and Content

### UserControlNavigate
Reactive navigation service managing user control switching in the main window. Uses `BehaviorSubject` to expose the active control as an observable, disposes previous controls on navigation, and delegates factory creation to `UserControlFactory`.

[Detailed Documentation](ClassesAndSystems/UserControlNavigate.md)


### Main Window Content
Primary navigation hub with three large image-based buttons (Table, Profile, Roll). Features animated rotating gradient borders on hover.

[Detailed Documentation](ClassesAndSystems/MainWindowContentSystem.md)

### Profile System
Comprehensive user profile displaying Steam avatar, nickname, player statistics cards, and games history table. Features animated gradients, dynamic card generation, and separate table window.

[Detailed Documentation](ClassesAndSystems/ProfileSystem.md)

### Rules System
Scrollable challenge rules display with 10 rules and pricing structure. Supports bilingual content (English/Russian) with manga-style theming.

[Detailed Documentation](ClassesAndSystems/RulesSystem.md)

### Game Table System
Real-time game progress table for lobby members with automatic database updates. Shows player games, completion status, and dates with manga aesthetic.

[Detailed Documentation](ClassesAndSystems/GameTableSystem.md)

## Steam Integration

### SteamManager
Singleton manager for Steamworks API lifecycle. Handles initialization, callback processing via timer, and user ID retrieval.

[Detailed Documentation](ClassesAndSystems/SteamManager.md)

### SteamWebApi
Service for querying Steam Web API to retrieve user profile data and avatars. Returns ProfilerContext with user information.

[Detailed Documentation](ClassesAndSystems/SteamWebApi.md)

## Services

### AvaloniaService
Static utility for Avalonia bitmap operations. Handles RGBA to BGRA conversion and Steam avatar rendering.

[Detailed Documentation](ClassesAndSystems/AvaloniaService.md)

### Logger
Color-coded console logging utility with severity levels (Info, Error, Warning, Debug).

[Detailed Documentation](ClassesAndSystems/Logger.md)

### MainWindowFactory
Factory for dynamically creating Avalonia grid layouts with buttons and images. Simplifies game display grid generation.

[Detailed Documentation](ClassesAndSystems/MainWindowFactory.md)

### ObservableConverter
Utility for converting collections to ObservableCollection for UI data binding.

[Detailed Documentation](ClassesAndSystems/ObservableConverter.md)

### Register<TKey, TValue>
Generic key-value registry with validation and null safety checks.

[Detailed Documentation](ClassesAndSystems/Register.md)

### SteamService
Singleton service for downloading web images and converting to Avalonia Bitmap format.

[Detailed Documentation](ClassesAndSystems/SteamService.md)

### Colors
Static utility providing predefined Avalonia color constants (LightSlateGray, Teal) for consistent UI theming.

[Detailed Documentation](ClassesAndSystems/Colors.md)

## Game Systems

### Game Generation
System for generating random game selections from JSON catalog. Organizes games by release year, supports category filtering, and provides random selection via GenerateRandomApps service.

[Detailed Documentation](ClassesAndSystems/GameGeneration.md)

## Database

### AppDbContext
Entity Framework Core database context managing PostgreSQL database connections. Handles 5 main entities: Users, Lobbies, GameProgresses, UserGame, and LobbyData with configured relationships.

[Detailed Documentation](ClassesAndSystems/AppDbContext.md)

### DatabaseService & PostgresListener
Generic repository service for CRUD operations and real-time change notification system. DatabaseService provides async data operations, PostgresListener monitors database changes via LISTEN/NOTIFY mechanism.

[Detailed Documentation](ClassesAndSystems/DatabaseServices.md)

## Lobby System

### LobbyService
Manages multiplayer game sessions by grouping players into isolated lobbies. Handles lobby creation, connection, disconnection, and publishes LobbyUpdate events via EventBus. Displays lobby members as avatars in the top panel.

[Detailed Documentation](ClassesAndSystems/LobbyService.md)
