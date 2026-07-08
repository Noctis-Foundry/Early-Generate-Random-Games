namespace GameRandom.Scripts.HandleSystem.PostgresListener;

public class PayloadStructure
{
    public int OpCode { get; set; }
    public int TableCode { get; set; }
    public int RowId { get; set; }
}