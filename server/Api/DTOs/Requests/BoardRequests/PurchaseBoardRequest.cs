using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Requests.BoardRequests;

public record PurchaseBoardRequest
{
    [Required]
    public required Guid GameId { get; set; }

    [Required, MinLength(5), MaxLength(8)]
    public required List<int> Numbers { get; set; } = [];
}