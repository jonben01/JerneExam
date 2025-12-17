namespace Api.DTOs;

public record GameAdminOverviewDto
{
    public required GameDto Game { get; set; }
    public IReadOnlyList<int> WinningNumbers { get; set; } = [];
    
    public int TotalBoards { get; set; }
    
    public int WinningBoards { get; set; }

    public IReadOnlyList<BoardDto> Boards { get; set; } = [];

    public IReadOnlyList<Guid> WinningBoardIds { get; set; } = [];
}