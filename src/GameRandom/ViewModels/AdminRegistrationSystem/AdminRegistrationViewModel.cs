using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using GameRandom.DataBaseContexts;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.Src;
using GameRandom.Src.Enums;
using GameRandom.Src.UserData;
using Microsoft.EntityFrameworkCore.Storage;

namespace GameRandom.ViewModels.AdminConfirmSystem;

/// <summary>
/// ViewModel for managing admin registration and permissions within a lobby.
/// </summary>
public sealed class AdminRegistrationViewModel : ViewModelBase
{
    /// <summary>
    /// Listener for database changes in PostgreSQL.
    /// </summary>
    [Inject] private PostgresListener _postgresListener = null!;
    
    /// <summary>
    /// Action to handle admin table updates from the database listener.
    /// </summary>
    private Action<PayloadStructure> _loadAdminTable = null!;
    
    /// <summary>
    /// Semaphore to ensure thread-safe execution of admin actions (add/remove).
    /// </summary>
    private SemaphoreSlim _isActionSemaphore = new(1, 1);

    private RegistrationLoad _registrationLoad; 

    #region BindingPropertys

    private ObservableCollection<AdminRegistrationData> _admins = new();

    /// <summary>
    /// Gets or sets the collection of potential and current admins for display.
    /// </summary>
    public ObservableCollection<AdminRegistrationData> Admins
    {
        get => _admins;
        set => SetProperty(ref _admins, value);
    }

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminRegistrationViewModel"/> class.
    /// Resolves dependencies and starts data loading.
    /// </summary>
    /// <exception cref="NullReferenceException">Thrown if required services are not injected.</exception>
    public AdminRegistrationViewModel()
    {
        InitializeDiContainer();
        InitializeListeners();
        InitializeSemaphoreSlim();

        _registrationLoad = new RegistrationLoad(StartTaskWaiter, CloseTaskWaiter, _isActionSemaphore);

        Dispatcher.UIThread.InvokeAsync(async () => await UpdateData());
    }

    /// <summary>
    /// Initializes listeners for database update notifications.
    /// </summary>
    private void InitializeListeners()
    {
        _loadAdminTable += e =>
        {
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                if (e.TableCode != (int)TableEnum.AdminTable)
                    return;

                await UpdateData();
            });
        };

        _postgresListener?.Subscribe(TableEnum.AdminTable, _loadAdminTable);
    }

    protected override void InitializeDiContainer()
    {
        base.InitializeDiContainer();

        if (_postgresListener == null)
            throw new NullReferenceException(nameof(_postgresListener));
    }

    private async Task UpdateData()
    {
        if (!await SemaphoreSlim.WaitAsync(SemaphoreTimeWait))
        {
            Logger.Error("Failed to start updating data, thread is not empty");
            return;
        }

        StartTaskWaiter();

        var result =
            await TaskRunner.RunWithFinallyActionT<List<AdminRegistrationData>>(
                async () => await _registrationLoad.LoadRegistrations(), CloseTaskWaiterWithSemaphore);

        if (!result.Success)
            return;

        if (result.Value is { } adminList)
            Admins = new ObservableCollection<AdminRegistrationData>(adminList);
    }

    /// <summary>
    /// Releases resources and unsubscribes from events.
    /// </summary>
    public override void Dispose()
    {
        _postgresListener?.Unsubscribe(TableEnum.AdminTable, _loadAdminTable);
        _postgresListener = null!;

        _loadAdminTable = null!;

        _isActionSemaphore.Dispose();
        
        _registrationLoad.Dispose();
        _registrationLoad = null!;

        _admins.Clear();
        Admins.Clear();

        base.Dispose();
    }
}