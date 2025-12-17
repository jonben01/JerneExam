using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Requests.GameRequests;

public record PublishWinningNumbersRequest
{
    [Required]
    public required Guid GameId { get; set; }
    
    [Required]
    [Range(1,16, ErrorMessage = "Number must be between 1 and 16")]
    public required int WinningNumber1 { get; set; }
    
    [Required]
    [Range(1,16, ErrorMessage = "Number must be between 1 and 16")]
    public required int WinningNumber2 { get; set; }
    
    [Required]
    [Range(1,16, ErrorMessage = "Number must be between 1 and 16")]
    public required int WinningNumber3 { get; set; }
}