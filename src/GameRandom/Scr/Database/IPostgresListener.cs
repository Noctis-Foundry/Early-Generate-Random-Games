using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace GameRandom.Scr.Service;

public interface IPostgresListener
{
    public Task UpdateAsync(PayloadStructure payloadStructure);

    public Task UpdateOpCode(TableEnum tableCode, int rowId, object? targetCollection);
    public Task DeleteOpCode(TableEnum tableCode, int rowId, object? targetCollection);
    public Task AddOpCode(TableEnum tableCode, int rowId, object? targetCollection);
}