using System.ComponentModel.DataAnnotations;

namespace Api.DTOs;

public class BoardPurchaseResponseDto
{
    public required BoardDto Board { get; set; }
    
    public required int NewBalance { get; set; }
}