using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using GameRandom.ViewModels.AdminPanelSystem;

namespace GameRandom.ViewModels.AdminSystem.Interface;

public interface IAdminLoad
{
    public Task<List<AdminPanelElementData>> LoadElementsData();
    public void Dispose();
}