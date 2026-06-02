using System;
using System.Threading.Tasks;
using GameRandom.Scr.Service;
using GameRandom.Src.StartupLogic;
using Npgsql;

namespace GameRandom.Scripts.HandleSystem.PostgresListener;

public class PostgresListener : IDisposable
{
    private NpgsqlConnection _connection;

    public Action<TableEnum, PayloadStructure> NotificationCallback { get; set; }
    
    public PostgresListener()
    {
        if (!GameEnvLoad._envCollection.TryGetValue(EnvType.DatabaseEnv, out var hostPath))
            throw new Exception("Database env not found");
        
        _connection = new NpgsqlConnection(hostPath);
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
                NotificationCallback?.Invoke((TableEnum)payload.TableCode, payload);
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

    public void Dispose()
    {
        _connection.Close();
    }
}