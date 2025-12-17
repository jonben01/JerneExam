using Api.DTOs.Responses.PlayerResponses;

namespace Api.DTOs;

public class BoardDto
{
    public Guid Id { get; set; }
    
    public Guid GameId { get; set; }
    
    public Guid PlayerId { get; set; }
    
    public int NumberCount { get; set; }
    
    public int PriceDkk { get; set; }
    
    public bool IsWinningBoard { get; set; }
    
    public Guid? SubscriptionId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    //no need to pass multiple board numbers when a user is making a guess
    public List<int> Numbers { get; set; } = [];
    
    //nested data to avoid repeat db calls, when relevant.
    public GameDto? Game { get; set; }
    public ApplicationUserDto? Player { get; set; }
    
    //in case I want just the name and not everything else
    public string? PlayerName { get; set; }
    

}