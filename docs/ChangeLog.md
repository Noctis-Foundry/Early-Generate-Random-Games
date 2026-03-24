# ChangeLog

## [2026-03-24]
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
