using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.Service;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GameRandom.IntegrationTests;

public class DatabaseServiceIntegrationTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;
    private readonly DatabaseService _databaseService;

    public DatabaseServiceIntegrationTests()
    {
        // Использование SQLite in-memory
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        // Инициализация схемы БД
        using (var context = new AppDbContext(_options))
        {
            context.Database.EnsureCreated();
        }

        _databaseService = new DatabaseService(_options);
    }

    [Fact]
    public async Task AddItemAsync_ShouldAddItemToDatabase()
    {
        // Arrange
        var user = new Users
        {
            SteamId = 123456789,
            Nickname = "TestUser",
            AvatarURL = 1
        };

        // Act
        var result = await _databaseService.AddItemAsync(user);

        // Assert
        Assert.True(result);
        
        using var context = new AppDbContext(_options);
        var addedUser = await context.Users.FirstOrDefaultAsync(u => u.SteamId == 123456789);
        Assert.NotNull(addedUser);
        Assert.Equal("TestUser", addedUser.Nickname);
    }

    [Fact]
    public async Task GetTableListAsync_ShouldReturnList()
    {
        // Arrange
        using (var context = new AppDbContext(_options))
        {
            context.Users.Add(new Users { SteamId = 1, Nickname = "User1", AvatarURL = 1 });
            context.Users.Add(new Users { SteamId = 2, Nickname = "User2", AvatarURL = 2 });
            await context.SaveChangesAsync();
        }

        // Act
        var result = await _databaseService.GetTableListAsync<Users>();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateEntity()
    {
        // Arrange
        var user = new Users { SteamId = 1, Nickname = "OldName", AvatarURL = 1 };
        using (var context = new AppDbContext(_options))
        {
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        user.Nickname = "NewName";

        // Act
        var result = await _databaseService.UpdateAsync(user);

        // Assert
        Assert.True(result);
        using (var context = new AppDbContext(_options))
        {
            var updatedUser = await context.Users.FirstOrDefaultAsync(u => u.SteamId == 1);
            Assert.Equal("NewName", updatedUser.Nickname);
        }
    }

    [Fact]
    public async Task DeleteItemAsync_ShouldRemoveItem()
    {
        // Arrange
        var user = new Users { SteamId = 1, Nickname = "ToDelete", AvatarURL = 1 };
        using (var context = new AppDbContext(_options))
        {
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        // В DatabaseService.DeleteItemAsync используется Attach/Remove
        // Чтобы это сработало с новым контекстом, объект должен иметь правильный Id
        var userToDelete = new Users { Id = user.Id, SteamId = 1, Nickname = "ToDelete", AvatarURL = 1 };

        // Act
        var result = await _databaseService.DeleteItemAsync(userToDelete);

        // Assert
        Assert.True(result);
        using (var context = new AppDbContext(_options))
        {
            var deletedUser = await context.Users.FirstOrDefaultAsync(u => u.SteamId == 1);
            Assert.Null(deletedUser);
        }
    }

    [Fact]
    public async Task TryGetOrCreateUserGame_ShouldCreateIfNotExist()
    {
        // Arrange
        var user = new Users { SteamId = 100, Nickname = "User100", AvatarURL = 1 };

        // Act
        var result = await _databaseService.TryGetOrCreateUserGame(user);

        // Assert
        Assert.True(result);
        using (var context = new AppDbContext(_options))
        {
            var userGame = await context.UserGames.FirstOrDefaultAsync(ug => ug.UserId == 100);
            Assert.NotNull(userGame);
        }
    }

    public void Dispose()
    {
        _connection.Close();
    }
}
