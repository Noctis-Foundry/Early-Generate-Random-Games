using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameRandom.ViewModels.AdminPanelSystem.Interface;

public interface IAdminLoad
{
    public Task<List<AdminPanelElementData>?> LoadElementsData();
    public void Dispose();
}