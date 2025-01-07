using Web.Client.SnekLogic;

namespace Web.Data;

public class Score
{
    public int Id { get; set; }

    public required int Points { get; set; }

    public string? UserId { get; set; }
    
    public required ApplicationUser User { get; set; }

    public required DateTime Timestamp { get; set; }
    
    public Replay? ReplayData { get; set; }
    
    public int? InputCount => ReplayData?.Inputs.Count;
}