using Api.DTOs;
using Api.DTOs.Requests;
using Api.Services.Interfaces;

namespace Api.Services;

public class BoardSubscriptionService : IBoardSubscriptionService
{
    public Task<BoardSubscriptionDto> CreateBoardSubscriptionAsync(CreateBoardSubscriptionRequest request, Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task CancelBoardSubscriptionAsync(Guid boardSubscriptionId, Guid userId)
    {
        throw new NotImplementedException();
    }
}