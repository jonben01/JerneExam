namespace Api.DTOs;

public class GameDto
{
    public Guid Id { get; set; }
    
    public int WeekNumber { get; set; }
    
    public int Year { get; set; }
    
    public DateTime StartDate { get; set; }
    
    public DateTime EndDate { get; set; }
    
    public DateTime GuessDeadline { get; set; }
    
    public bool IsActive { get; set; }
    
    public int? WinningNumber1 { get; set; }
    
    public int? WinningNumber2 { get; set; }
    
    public int? WinningNumber3 { get; set; }
    
    public DateTime? NumbersPublishedAt  { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    
}