using System.Collections.ObjectModel;
using GameRandom.DependenceInjectSystem.DiSystem;
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

namespace GameRandom.ViewModels.AdminConfirmSystem;

public class StatisticGameTableViewModel : AbstractTableWindowViewModel<GameProgresses>
{
}