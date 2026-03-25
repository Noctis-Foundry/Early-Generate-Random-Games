using System.Collections.ObjectModel;
using GameRandom.Scr.DI;
using GameRandom.Scr.Service;
using GameRandom.Src;
using System.Threading.Tasks;
using GameRandom.DataBaseContexts;
using System.Collections.Generic;
using Avalonia.Threading;
using GameRandom.Src.Enums;
using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace GameRandom.ViewModels.AdminSystem;

public class StatisticGameTableViewModel : AbstractTableWindowViewModel<GameProgresses>
{
}