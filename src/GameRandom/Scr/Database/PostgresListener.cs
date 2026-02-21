using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace GameRandom.Scr.Service;

public class PostgresListener : IDisposable
{
    private NpgsqlConnection _connection;
    private const string HostPath = "Host=80.93.62.153;Database=steamdata;Username=users;Password=ninokuriko212410";

    private Dictionary<TableEnum, Action<PayloadStructure>?> _tableCallbacks =
        new Dictionary<TableEnum, Action<PayloadStructure>?>
        {
            { TableEnum.Lobby, null },
            { TableEnum.UserGames, null },
            { TableEnum.GameProgress, null },
            { TableEnum.Users, null }
        };

    public PostgresListener()
    {
        _connection = new NpgsqlConnection(HostPath);
        _connection.Open();

        ListenChanel();
    }

    private void ListenChanel()
    {
        var cmd = new NpgsqlCommand("LISTEN changes_channel", _connection);
        cmd.ExecuteNonQuery();

        _connection.Notification += (o, e) =>
        {
            PayloadStructure? payload = ParsingPayload(e.Payload);

            if (payload != null)
            {
                SendCallbacks((TableEnum)payload.TableCode, payload);
            }
        };

        Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    await _connection.WaitAsync();
                    await Task.Delay(10);
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed to connect to database {e.Message}");
                    break;
                }
            }
        });
    }

    public void Subscribe(TableEnum table, Action<PayloadStructure> subscriber)
    {
        if (!_tableCallbacks.ContainsKey(table))
        {
            Logger.Error($"Table {table} not registered");
            return;
        }

        _tableCallbacks[table] += subscriber;
    }

    public void Unsubscribe(TableEnum table, Action<PayloadStructure> subscriber)
    {
        if (_tableCallbacks.TryGetValue(table, out var callback))
        {
            callback -= subscriber;
        }
        else
        {
            Logger.Error($"Table {table} not registered");
        }
    }

    private PayloadStructure? ParsingPayload(string payload)
    {
        var split = payload.Split('.');

        if (split.Length != 3)
        {
            Logger.Error("Non correctable payload");
            return null;
        }

        if (!int.TryParse(split[0], out var opCode) ||
            !int.TryParse(split[1], out var tableCode) ||
            !int.TryParse(split[2], out var rowId))
        {
            Logger.Error("Invalid payload format");
            return null;
        }

        return new PayloadStructure
        {
            OpCode = opCode,
            TableCode = tableCode,
            RowId = rowId
        };
    }

    private void SendCallbacks(TableEnum table, PayloadStructure payload)
    {
        Logger.Info($"Sending callbacks with table {table}");

        if (_tableCallbacks.TryGetValue(table, out var callback))
        {
            callback?.Invoke(payload);
        }
        else
        {
            Logger.Error($"Table {table} not registered");
        }
    }

    public void Dispose()
    {
        _connection.Close();
    }
}

public class PayloadStructure
{
    public int OpCode { get; set; }
    public int TableCode { get; set; }
    public int RowId { get; set; }
}

public enum TableEnum
{
    GameProgress = 0,
    Lobby = 1,
    UserGames = 2,
    Users = 3
}

public enum OperationsEnum
{
    Add = 0,
    Update = 1,
    Delete = 2,
}