# Unit Tests Documentation

## Scr/DI
### DiContainerTests
- `RegisterSingleInstance_Should_StoreInstance`: Verifies that an instance can be registered and retrieved as a singleton.
- `GetInstance_NotRegistered_Should_ThrowException`: Ensures an exception is thrown when requesting a type that hasn't been registered.
- `TryGetInstance_NotRegistered_Should_ReturnNull`: Verifies that `TryGetInstance` returns null instead of throwing when a type is missing.
- `Constructor_Should_RegisterSelf`: Checks that the container registers itself upon creation.
- `ResolveFieldsFromClassInstance_Should_InjectDependencies`: Tests dependency injection into fields marked with `[Inject]`.
- `ResolveField_Should_ReturnTrue_When_Registered`: Verifies that `ResolveField` correctly finds and returns a registered dependency.
- `ResolveField_Should_ReturnFalse_When_NotRegistered`: Ensures `ResolveField` returns false when a dependency is missing.
- `ResolveFieldsFromClassInstance_Should_Ignore_Fields_Without_Inject_Attribute`: Verifies that only fields marked with `[Inject]` are modified.
- `GetInstance_ByType_Should_ReturnInstance`: Tests retrieving a registered instance by specifying its Type.
- `TryGetInstance_ByType_Should_ReturnNull_When_NotRegistered`: Ensures `TryGetInstance` returns null when a dependency is missing.

### DiFactoryTests
- `Create_OneArg_Should_InitAndRegister`: Verifies that `Create` with one argument calls `Init` and registers the instance.
- `Create_WithInterface_Should_InitAndRegisterAsInterface`: Tests registration of an instance under its interface type.
- `Create_TwoArgs_Should_InitAndRegister`: Verifies that `Create` with two arguments correctly initializes and registers the instance.

## Scr/GenerateGames
### GenerateRandomAppsTests
- `Constructor_WithValidPath_ShouldInitialize`: Verifies that `GenerateRandomApps` initializes correctly when given a valid JSON file path.
- `Constructor_WithInvalidPath_ShouldThrowFileNotFoundException`: Ensures that an exception is thrown if the provided path is invalid.
- `GetRandomGame_WithYear_ShouldReturnMatchingGame`: Tests that the random game returned matches the requested release year.
- `GetRandomGame_WithYear_NoMatch_ShouldReturnNull`: Verifies that null is returned if no games match the given year.
- `GetRandomGame_NoYear_ShouldReturnAnyGame`: Ensures that calling `GetRandomGame` without parameters returns an existing game.

## Services
### EventBusTests
- Tests for event publication and subscription.

### ObservableConverterTests
- Tests for converting collections to observable formats.

### RegisterTests
- Tests for the generic `Register<TKey, TValue>` class.

### SteamServiceTests
- Tests for Steam-related functionality (profile, images).

## AvaloniaConverters
### BoolConverterTests
- `Convert_True_ShouldReturnCompleted`: Verifies conversion from true to "Completed".
- `Convert_False_ShouldReturnInProgress`: Verifies conversion from false to "In Progress".
- `ConvertBack_CompletedString_ShouldReturnTrue`: Verifies conversion back from "Completed" string to true.

### LongToStringConverterTests
- `Convert_LongValue_ShouldIncludePrefix`: Verifies formatting of long values with a prefix string.
- `Convert_NonLong_ShouldReturnEmptyLobbyID`: Ensures non-long values return a default "Empty lobby ID".

### TimeSpanFormatConverterTests
- `Convert_DateTime_ShouldFormatWithPrefix`: Verifies that DateTime values are formatted as a long date string with a prefix.

### DictionaryToListConverterTests
- `Convert_Dictionary_ShouldReturnList`: Tests converting a Dictionary of AdminPanelElementData to a List.

### ArrayTextJoinConverterTests
- `Convert_HashSet_ShouldJoinWithPrefix`: Verifies joining a HashSet of strings with a prefix.

### DictionaryValuesToHashSetConverterTests
- `Deserialize_Dictionary_ShouldReturnHashSetOfValues`: Verifies custom JSON deserialization from a dictionary to a hashset of its values.

## DbContext
### CloningServiceTests
- `Clone_SimpleObject_ShouldReturnNewInstanceWithSameValues`: Verifies that CloningService produces a separate instance with identical property values.
- `Clone_Null_ShouldReturnDefault`: Ensures that cloning a null object returns null/default.
