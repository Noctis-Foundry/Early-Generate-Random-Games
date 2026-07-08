using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameRandom.ViewModels.AdminRegistrationSystem.Interface;

public interface IRegistrationLoad
{
    public Task<List<AdminRegistrationData>?> LoadRegistrations();
    
    public void Dispose();
}