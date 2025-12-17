using Api.DTOs.Responses.PlayerResponses;

namespace Api.DTOs;

public class BoardSubscriptionDto
{
    public Guid Id { get; set; }
    
    public Guid PlayerId { get; set; }
    
    public int PricePerGameDkk { get; set; }
    
    public Guid StartGameId { get; set; }
    
    public int? TotalGames { get; set; }
    
    public int GamesAlreadyPlayed { get; set; }
    
    public bool IsActive { get; set; }
    
    public DateTime? CancelledAt { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    //no need to pass multiple board numbers when a user is making a guess
    public List<int> Numbers { get; set; } = [];
    
    //nested data to avoid repeat db calls, when relevant.
    public GameDto? StartGame { get; set; }
    public ApplicationUserDto? Player { get; set; }
   
}