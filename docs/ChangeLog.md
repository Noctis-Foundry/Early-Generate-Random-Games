# ChangeLog

## [2026-03-30]
- refactor(viewmodels): reorganize admin system into AdminConfirmSystem folder
- refactor(admin): extract business logic from AdminConfirmViewModel
- feat(service): enhance TaskRunner with new execution methods

## [2026-03-28]
- feat(core): add TaskRunner service for error handling
- refactor(base): integrate TaskRunner into base classes
- refactor(admin): improve AdminPanel error handling and lifecycle
- fix(windows): improve window lifecycle and error handling
- refactor: remove ConfirmImage window and migrate to service-based approach
- feat(image): implement ImageConfirmService with file and clipboard support
- refactor(windows): integrate ImageConfirmService into ConfirmFinishGame
- fix(admin): add error handling and processing state management
- feat(admin): add PostgreSQL listener for AdminPanel
- fix(current-game): add empty state validation
- style: update window styles and positioning
- chore: remove binary AppImage file

## [2026-03-25]
- refactor: migrate from WindowAbstract to WindowBase with generic ViewModel support
- refactor: add lifecycle management (IsActive, IsClosing flags) to WindowBase
- refactor: add built-in processing handler initialization and helper methods to WindowBase
- refactor: update all window classes to inherit from WindowBase<TViewModel>
- fix(game-selection): add transaction support with rollback capability to DatabaseService
- fix(game-selection): improve cancellation token handling (use TimeSpan.FromSeconds)
- fix(game-selection): add SemaphoreSlim to prevent concurrent game selection
- fix(game-selection): refactor ValidateUserCanStartNewGame to async method
- fix: resolve CurrentGameWindow data loading issue with TaskWaiter integration
- fix: resolve CancellationTokenSource initialization in ConfirmFinishGameViewModel
- refactor(viewmodels): integrate TaskWaiter across all ViewModels
- refactor(viewmodels): fix CancellationTokenSource usage in AbstractTableWindowViewModel
- refactor(viewmodels): improve resource management with proper Dispose implementations
- refactor(window-services): simplify AbstractWindowService with primary constructor
- refactor(window-services): update AdminConfirmService and ErrorService event handling
- refactor: update namespace from GameRandom.SteamSDK to GameRandom.Src across entire codebase

## [2026-03-24]
- feat(infra): add task waiter window service for handling async task loading indicators
- feat(ui): add task loading view and update global styles for text blocks
- refactor(mvvm): implement processing state tracking in ViewModels
- feat(admin): integrate task waiter in AdminRegistration and fix CancellationTokenSource lifecycle bug
- chore: add TaskViewLoading project structure for task loading visualization
- refactor: Update Junie rules
- fix: update database service and app context for better testability
- docs: add documentation for unit and integration tests
- test: add unit and integration tests for core services and viewmodels
- refactor(di): convert SteamService to DI-managed service and refactor injection

## [2026-03-23]
- feat(AI): Add AIRules for Junie
- docs: update guidelines with git authoring rule
- feat(filter): add JSON data for game categories and genres
- fix(ui): update property name in ConfirmFinishGame and refresh local files
- feat(database): implement transactional game finishing and table data retrieval
- refactor(viewmodels): add XML documentation and minor improvements to various ViewModels
- refactor(ui): update MainWindowViewModel with XML documentation
- refactor(game): enhance game selection logic and documentation in ChooseGameViewModel
- refactor(stats): add XML documentation and real-time updates to StatisticViewModel
- refactor(admin): add XML documentation to AdminPanelViewModel
- refactor(admin): improve game confirmation and rejection in AdminConfirmViewModel
- refactor(admin): add XML documentation and improve concurrency in AdminRegistrationViewModel
- refactor(game): simplify game generation logic in RollGameViewModel
- fix: refactor logic by SOLID and fix misstakes for treading logic
