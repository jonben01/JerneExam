using Api.DTOs;
using Api.DTOs.Requests;

namespace Api.Services.Interfaces;

public interface IBoardSubscriptionService
{
    Task<BoardSubscriptionDto> CreateBoardSubscriptionAsync(CreateBoardSubscriptionRequest request, Guid userId);
    Task CancelBoardSubscriptionAsync(Guid boardSubscriptionId, Guid userId); //expand so admin can cancel it
    
    //get all, for admin
    //getbyasync
    
    
}