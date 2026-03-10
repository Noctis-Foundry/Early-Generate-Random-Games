# GameRandom Integration Tests

Integration tests for the GameRandom application, focusing on the ConfirmFinishGame system.

## Running Tests

```bash
dotnet test
```

## Test Coverage

### ConfirmFinishGameTests

Tests for the ConfirmFinishGame window and ViewModel:

- **ViewModel_Properties_Are_Initialized** - Verifies initial state of ViewModel properties
- **ViewModel_GameProgress_Can_Be_Set** - Tests setting GameProgress data
- **SaveEditAsync_Executes_Without_Exception** - Validates SaveEdit method execution
- **ViewModel_PropertyChanged_Fires_On_GameProgress_Change** - Tests property change notifications
- **GameProgress_Comment_Can_Be_Updated** - Verifies comment updates
- **Multiple_Property_Changes_Fire_Events** - Tests multiple property change events

## Test Structure

```
tests/
└── GameRandom.IntegrationTests/
    ├── ConfirmFinishGameTests.cs
    ├── GameRandom.IntegrationTests.csproj
    └── README.md
```

## Dependencies

- xUnit - Testing framework
- Microsoft.NET.Test.Sdk - Test SDK
- GameRandom project reference
