namespace Api.DTOs;

public class GameStatsDto
{
    //TODO add total money spent on boards as well, + separate by numbers e.g. 342 5 tile boards, 225 6 tile boards 40 7 tile boards, 1 8 tile board.
    public Guid GameId { get; set; }
    public int WeekNumber { get; set; }
    public int Year { get; set; }
    
    public int TotalBoards { get; set; }
    public int TotalWinningBoards { get; set; }
     
    public int? WinningNumber1  { get; set; }
    public int? WinningNumber2 { get; set; }
    public int? WinningNumber3 { get; set; }
    
    public DateTime? NumbersPublishedAt { get; set; }
    public bool IsFinished { get; set; }
}