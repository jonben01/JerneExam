
using Api.DTOs.Requests.TransactionRequests;
using Api.DTOs.Responses.TransactionResponses;

namespace Api.Services.Interfaces;

public interface ITransactionService
{
    //TODO just bite the bullet and add cancellation tokens to all other interfaces and service methods, like here
    Task CreateDepositRequestAsync(Guid userId, CreateDepositRequest request, CancellationToken ct = default);

    Task<IReadOnlyList<TransactionHistoryListItemDto>> GetPersonalTransactionHistoryAsync(Guid userId, CancellationToken ct = default);
    
    Task<int> GetBalanceAsync(Guid userId, CancellationToken ct = default);
    
    Task<IReadOnlyList<PendingDepositsListItemDto>> GetPendingDepositsListAsync(CancellationToken ct = default);
    
    Task ApproveDepositAsync(Guid transactionId,Guid adminUserId, CancellationToken ct = default);
    
    Task RejectDepositAsync(Guid transactionId,Guid adminUserId, CancellationToken ct = default);
    
    Task<IReadOnlyList<TransactionHistoryListItemDto>> GetTransactionHistoryAsync(CancellationToken ct = default);
    
    Task<IReadOnlyList<TransactionHistoryListItemDto>> GetUserTransactionHistoryAsync(Guid userId, CancellationToken ct = default);
    
    Task<TransactionDto?> GetByMobilePayReferenceAsync(string mobilePayReference, CancellationToken ct = default);
    
    Task<TransactionDto?> GetByIdAsync(Guid transactionId, CancellationToken ct = default);
}