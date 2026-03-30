# Future Changes

This document contains AI-generated analysis and recommendations for the GameRandom project.

---

#priority Приоритетные задачи
---

- [ ] Fix critical memory leak in async event handlers
     - AdminConfirmWindow: Dispatcher.UIThread.InvokeAsync creates fire-and-forget tasks
     - If window closes during operation, task continues with disposed ViewModel
     - Can cause crashes or data corruption
     - Priority: HIGH - affects admin workflow stability

- [ ] Fix NullReferenceException in CurrentGame.ShowSteamStore()
     - Direct access to vm.UserGame.AppId without null check
     - Crashes when user clicks Steam button on empty game
     - Priority: HIGH - user-facing crash

- [ ] Standardize window disposal pattern across all views
     - Inconsistent OnClosing implementations (Hide vs Close vs Cancel)
     - Some windows leak resources, others dispose prematurely
     - Affects: LobbyWindow, ErrorWindow, AdminConfirmWindow, ConfirmFinishGame
     - Priority: MEDIUM - technical debt causing maintenance issues

#ai #review #todo

---

#fix Fix UX experience
---

- [ ] Add loading indicator in RollGame during game generation
     - GenerateGame() can take several seconds with filters
     - User has no feedback except disabled button
     - _loadGif exists but only shows in grid, not during ViewModel.GenerateGames()
     - Show loading overlay or progress indicator during async operation

- [ ] Improve error messages in ChooseGameWindow
     - ChooseGame() logs result to console: Logger.Debug($"Choose game is {isAdd}")
     - User gets no feedback if operation fails
     - Add ErrorService.ShowWindow() with descriptive message

- [ ] Add confirmation dialog before closing windows with unsaved data
     - ConfirmFinishGame has comment and image input
     - User can accidentally close window losing data
     - Use ConfirmService before closing if data entered

- [ ] Improve FilterGameWindow usability
     - No visual feedback when filters applied
     - No "Clear All" or "Select All" buttons
     - No indication of how many games match current filters
     - Add filter summary and quick action buttons

- [ ] Add keyboard shortcuts for common actions
     - Enter key should trigger primary action (Confirm, Choose, etc.)
     - Escape key should close modal windows
     - Currently requires mouse clicks for all actions

#ai #review #todo

---

#bug Fix Bug
---

- [ ] Fix potential memory leak in AdminConfirmWindow
     - ConfirmGame() and RejectGame() use Dispatcher.UIThread.InvokeAsync without proper cancellation
     - If window closes during async operation, callbacks may execute on disposed window
     - Add CancellationTokenSource tied to window lifetime
     - Cancel pending operations in Dispose()

- [ ] Fix race condition in RollGame.GenerateGame()
     - _filterGameWindow can be reassigned while previous window is still open
     - Multiple filter windows can exist simultaneously
     - Store single instance and reuse, or ensure proper disposal before creating new

- [ ] Fix missing null check in CurrentGame.ShowSteamStore()
     - vm.UserGame can be null when IsEmpty = true
     - Accessing vm.UserGame.AppId without null check causes NullReferenceException
     - Add null validation before accessing UserGame properties

- [ ] Fix event handler leak in ConfirmFinishGame
     - CommentBox.TextChanging handler stored in _textChanging field
     - Handler unsubscribed in Dispose() but Dispose() may not be called if window crashes
     - Consider using WeakEventManager or ensure Dispose() called in all exit paths

- [ ] Fix potential deadlock in LobbyWindow.OnClosing()
     - Sets IsClosing = true and cancels event with e.Cancel = true
     - Window never actually closes, just hides
     - IsClosing flag prevents future close attempts
     - Reset IsClosing when showing window again or remove cancel logic

#ai #review #todo

---

#refactor Refactoring plan
---



#ai #review #todo

---

#refactor View Models
---



#ai #review #todo

---

#refactor Window Base
---

- [ ] Improve WindowBase<TViewModel> generic constraint
     - Current constraint: where TViewModel : ViewModelBase, new()
     - new() constraint forces parameterless constructor
     - Prevents ViewModels with constructor dependencies (e.g., RollGameViewModel needs IGenApp)
     - Consider factory pattern or remove new() constraint, require explicit ViewModel passing

- [ ] Fix SavedProcessingHandler lifecycle
     - Field declared but not initialized in constructor
     - Can be null when Dispose() tries to unsubscribe: vm.StartProcessing -= SavedProcessingHandler
     - Add null check before unsubscribing or initialize to empty delegate

- [ ] Clarify InitializeProcessingHandler hostWindow parameter
     - Parameter has default value null! (null-forgiving operator)
     - Passed to TaskWaiterWindow.ShowAsyncWaiter() which may require valid window
     - Either make parameter required or handle null case explicitly

- [ ] Remove redundant IsActive/IsClosing state tracking
     - Avalonia Window already has IsVisible and lifecycle events
     - Custom flags can desync from actual window state
     - Use built-in properties or justify why custom tracking needed

- [ ] Improve Dispose() pattern implementation
     - Implements IDisposable but not full pattern (no Dispose(bool disposing))
     - Virtual Dispose() can be overridden without calling base
     - No finalizer or unmanaged resource handling
     - Consider sealed Dispose() with protected virtual DisposeManagedResources()

- [ ] Extract EventBus subscription helper to base class
     - InitializeEventBusListener<TEvent>() useful but only in WindowBase
     - MainWindowUserControlAbstract could benefit from same pattern
     - Move to shared base or create mixin/extension

#ai #review #todo

---

#refactor View classes
---

- [ ] Refactor WindowBase<TViewModel> lifecycle management
     - IsActive and IsClosing flags have unclear state management
     - Show() checks IsActive but doesn't handle case when window is closing
     - OnClosing() sets IsClosing but derived classes may override without calling base
     - Consider state machine pattern: Closed -> Opening -> Active -> Closing -> Closed

- [ ] Extract common DI initialization pattern
     - Multiple views repeat: Di.Container.ResolveFieldsFromClassInstance(this)
     - Followed by null checks throwing NullReferenceException
     - Create base method ValidateInjectedDependencies() with reflection-based validation
     - Reduce boilerplate in 10+ view classes

- [ ] Consolidate processing handler initialization
     - InitializeProcessingHandler() duplicated in WindowBase and MainWindowUserControlAbstract
     - Identical implementation in both classes
     - Extract to shared base class or static helper
     - Ensure single source of truth for processing UI logic

- [ ] Refactor filter window lifecycle in RollGame
     - _filterGameWindow created in constructor, recreated in GoToFilter()
     - No clear ownership or disposal strategy
     - Consider lazy initialization or service-based approach
     - Ensure single instance with proper disposal

- [ ] Improve error handling in view event handlers
     - Many async void event handlers (GenerateGame, FinishedGame, etc.)
     - Exceptions silently swallowed or only logged to console
     - Add try-catch with ErrorService.ShowWindow() for user feedback
     - Consider TaskRunner.Run() wrapper for consistent error handling

- [ ] Remove tight coupling to concrete window types
     - RollGame creates ChooseGameWindow directly: new ChooseGameWindow()
     - AdminPanel creates AdminRegistrationWindow directly
     - Violates dependency inversion principle
     - Inject window factories or use service locator pattern

- [ ] Standardize window closing behavior
     - Some windows use Hide() in OnClosing (LobbyWindow, ErrorWindow)
     - Others call Dispose() and allow close (AdminConfirmWindow)
     - Inconsistent e.Cancel usage
     - Define clear guidelines: modal vs modeless, reusable vs disposable

#ai #review #todo

---

#dowork Task Runner
---



#ai #review #todo
