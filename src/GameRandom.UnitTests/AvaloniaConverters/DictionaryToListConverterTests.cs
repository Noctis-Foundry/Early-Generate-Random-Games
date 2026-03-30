using System;
using System.Collections.Generic;
using System.Globalization;
using GameRandom.AvaloniaConverters;
using GameRandom.DataBaseContexts;
using CommunityToolkit.Mvvm.Input;
using GameRandom.Scr.Service;
using GameRandom.ViewModels.AdminConfirmSystem;
using GameRandom.ViewModels.AdminPanelSystem;
using Xunit;

namespace GameRandom.UnitTests.AvaloniaConverters;

public class DictionaryToListConverterTests
{
    private readonly DictionaryToListConverter _converter = new DictionaryToListConverter();

    [Fact]
    public void Convert_Dictionary_ShouldReturnList()
    {
        var gameInfo = new FinishedGames();
        var command = new RelayCommand(() => Console.WriteLine("Convert dictionary test click"));
        var data = new AdminPanelElementData(gameInfo, command, "Player1");
        
        var dict = new Dictionary<int, AdminPanelElementData>
        {
            { 1, data }
        };

        var result = _converter.Convert(dict, typeof(List<AdminPanelElementData>), null, CultureInfo.InvariantCulture);

        Assert.IsType<List<AdminPanelElementData>>(result);
        var list = (List<AdminPanelElementData>)result;
        Assert.Single(list);
        Assert.Equal("Player1", list[0].Nickname);
    }

    [Fact]
    public void Convert_NonDictionary_ShouldReturnNull()
    {
        var result = _converter.Convert("not a dictionary", typeof(List<AdminPanelElementData>), null, CultureInfo.InvariantCulture);
        Assert.Null(result);
    }
}
