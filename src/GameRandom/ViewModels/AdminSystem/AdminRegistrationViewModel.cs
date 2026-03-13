using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;

namespace GameRandom.ViewModels.AdminSystem;

public class AdminRegistrationViewModel : ViewModelBase
{
    [Inject] private DatabaseService? _databaseService;
    
    private ObservableCollection<Users> _admins;
    public ObservableCollection<Users> Admins
    {
        get => _admins;
        set => SetProperty(ref _admins, value);
    }

    public async Task LoadData()
    {
        var users = await _databaseService.GetTableListAsync<Users>();
        
        if (users is null)
            throw new NullReferenceException(nameof(users));
    }
}