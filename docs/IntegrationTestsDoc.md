# Integration Tests Documentation

## GameRandom.IntegrationTests

### ConfirmFinishGameTests
- `ViewModel_Properties_Are_Initialized`: Verifies that `ConfirmFinishGameViewModel` starts with null properties.
- `ViewModel_GameProgress_Can_Be_Set`: Ensures `GameProgress` can be correctly assigned to the ViewModel.
- `SaveEditAsync_Executes_Without_Exception`: Basic sanity check that the save method doesn't crash (mocked environment).
- `ViewModel_PropertyChanged_Fires_On_GameProgress_Change`: Verifies MVVM property notification.
- `GameProgress_Comment_Can_Be_Updated`: Tests data binding/property updates on the model.
- `Multiple_Property_Changes_Fire_Events`: Ensures multiple updates trigger multiple notifications.

### RollGameIntegrationTests
- `GenerateGames_Should_LoadGames_From_Json_File`: Verifies the full chain: `RollGameViewModel` -> `GenerateRandomApps` -> JSON file -> `SteamService` -> Web. Ensures games are correctly loaded and images are fetched.
- `GenerateGames_With_Filter_Should_Only_Return_Matching_Games`: Tests the integration of the filtering logic with the real data source (JSON).

### DatabaseServiceIntegrationTests
- `AddItemAsync_ShouldAddItemToDatabase`: Проверяет успешное добавление новой сущности (пользователя) в базу данных.
- `GetTableListAsync_ShouldReturnList`: Проверяет получение списка всех записей из таблицы.
- `UpdateAsync_ShouldUpdateEntity`: Проверяет корректное обновление существующей записи.
- `DeleteItemAsync_ShouldRemoveItem`: Проверяет удаление записи из базы данных.
- `TryGetOrCreateUserGame_ShouldCreateIfNotExist`: Проверяет логику "получить или создать" для игровой сессии пользователя.
