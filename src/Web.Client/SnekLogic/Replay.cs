namespace Web.Client.SnekLogic;

public class Replay
{
    public int Score { get; set; }
    public int Seed { get; set; }
    public Dictionary<int, Direction> Inputs { get; set; } = new();
}
