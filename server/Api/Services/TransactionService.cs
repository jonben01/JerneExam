using Api.DTOs.Requests.TransactionRequests;
using Api.DTOs.Responses.TransactionResponses;
using Api.DTOs.Util;
using Api.Services.Interfaces;
using Api.Services.Util;
using DataAccess;
using DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Api.Services;

public class TransactionService : ITransactionService
{
    private readonly MyDbContext _context;

    public TransactionService(MyDbContext context)
    {
        _context = context;
    }
    
    
    public async Task CreateDepositRequestAsync(Guid userId, CreateDepositRequest request,
        CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(request.AmountDkk, 0);
        
        if (string.IsNullOrWhiteSpace(request.MobilePayReference))
        {
            throw new ArgumentException("MobilePay transaction number is required");
        }
        
        var now =  DateTime.UtcNow;

        var mobilePayRef = request.MobilePayReference.Trim();
        
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            PlayerId = userId,
            TransactionType = TransactionType.Deposit,
            AmountDkk = request.AmountDkk,
            Status = TransactionStatus.Pending,
            MobilePayReference = mobilePayRef,
            BoardId = null,
            ProcessedBy = null,
            ProcessedAt = null,
            CreatedAt = now,
            UpdatedAt = null,
            IsDeleted = false,
            DeletedAt = null,
        };
        
        _context.Transactions.Add(transaction);

        try
        {
            await _context.SaveChangesAsync(ct);
            return;
        } //catch unique constraint violation
        catch (DbUpdateException ex) when (ex.GetBaseException() is Npgsql.PostgresException pgEx 
                                           && pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            var isMine = await _context.Transactions
                .AsNoTracking()
                .AnyAsync(t => t.MobilePayReference == mobilePayRef 
                               && t.PlayerId == userId 
                               && t.TransactionType == TransactionType.Deposit 
                               && t.AmountDkk == request.AmountDkk, ct);

            //Idempotent silent succession if user already submitted this, mostly for double clicks.
            if (isMine)
            {
                return;
            }
            
            throw new InvalidOperationException("A deposit with this MobilePay transaction number already exists");
        }
    }

    public async Task<IReadOnlyList<TransactionHistoryListItemDto>> GetPersonalTransactionHistoryAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Transactions
            .AsNoTracking()
            .Where(t => t.PlayerId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(EntityToDtoMapper.TransactionHistoryListItemToDto)
            .ToListAsync(ct);
    }

    //TODO uhhh dont do that??
    public async Task<int> GetBalanceAsync(Guid userId, CancellationToken ct = default)
    {
        return await GetPlayerBalanceInternalAsync(userId, ct);
    }
    
    private async Task<int> GetPlayerBalanceInternalAsync(Guid userId, CancellationToken ct)
    {
        //Deposits add to total, Purchases subtract (logic is if type = purchase then subtract, if not, add.)
        var balance = await _context.Transactions
            .AsNoTracking()
            .Where(t => 
                t.PlayerId == userId &&
                t.Status == TransactionStatus.Approved &&
                (t.TransactionType == TransactionType.Deposit || t.TransactionType == TransactionType.Purchase))
            .SumAsync(t => (int?)(
                t.TransactionType == TransactionType.Purchase
                    ? -t.AmountDkk
                    : t.AmountDkk), cancellationToken: ct) ?? 0;
        
        //Anything below 0 doesn't matter
        return Math.Max(0, balance);
    }

    public async Task<IReadOnlyList<PendingDepositsListItemDto>> GetPendingDepositsListAsync(CancellationToken ct = default)
    {
        return await _context.Transactions
            .AsNoTracking()
            .Where(t => t.TransactionType == TransactionType.Deposit && 
                                  t.Status == TransactionStatus.Pending)
            .OrderBy(t => t.CreatedAt)
            .Select(EntityToDtoMapper.PendingTransactionListItemToDto)
            .ToListAsync(ct);
    }

    public Task ApproveDepositAsync(Guid transactionId,Guid adminUserId, CancellationToken ct = default) => 
        ProcessDepositAsync(transactionId, adminUserId, approve: true, ct);

    public Task RejectDepositAsync(Guid transactionId,Guid adminUserId, CancellationToken ct = default) => 
        ProcessDepositAsync(transactionId, adminUserId, approve: false, ct);
    
    private async Task ProcessDepositAsync(Guid transactionId, Guid adminUserId, bool approve, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        
        var newStatus = approve ? TransactionStatus.Approved : TransactionStatus.Rejected;

        var rows = await _context.Transactions
            .Where(t =>
                t.Id == transactionId &&
                t.TransactionType == TransactionType.Deposit &&
                t.Status == TransactionStatus.Pending)
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.Status, newStatus)
                .SetProperty(t => t.UpdatedAt, now)
                .SetProperty(t => t.ProcessedAt, now)
                .SetProperty(t => t.ProcessedBy, adminUserId), 
                ct);

        if (rows != 0)
        {
            return;
        }
        
        var state = await _context.Transactions
            .AsNoTracking()
            .Where(t => t.Id == transactionId)
            .Select(t => new { t.TransactionType, t.Status })
            .SingleOrDefaultAsync(ct);

        if (state is null)
        {
            throw new KeyNotFoundException($"Transaction with id {transactionId} not found");
        }
        
        if (state.TransactionType != TransactionType.Deposit)
        {
            throw new InvalidOperationException("Only deposits can be processed");
        }

        if (state.Status != TransactionStatus.Pending)
        {
            throw new InvalidOperationException("Only pending deposits can be processed");
        }
            
        throw new InvalidOperationException("Deposit could not be processed");
    }

    public async Task<IReadOnlyList<TransactionHistoryListItemDto>> GetTransactionHistoryAsync(CancellationToken ct = default)
    {
        return await _context.Transactions
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .Select(EntityToDtoMapper.TransactionHistoryListItemToDto)
            .ToListAsync(ct);
    }

    //TODO just make 2 different endpoints in controller and keep just one method.
    //functionally identical to GetPersonalHistory method, just keeping this for clarity in the controller endpoints
    public async Task<IReadOnlyList<TransactionHistoryListItemDto>> GetUserTransactionHistoryAsync(Guid userId, CancellationToken ct = default)
    {
        return await GetPersonalTransactionHistoryAsync(userId, ct);
    }

    public async Task<TransactionDto?> GetByMobilePayReferenceAsync(string mobilePayReference, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(mobilePayReference))
        {
            return null;
        }

        return await _context.Transactions
            .AsNoTracking()
            .Where(t => t.MobilePayReference == mobilePayReference)
            .Select(EntityToDtoMapper.TransactionToDto)
            .SingleOrDefaultAsync(ct);
    }

    public async Task<TransactionDto?> GetByIdAsync(Guid transactionId, CancellationToken ct = default)
    {
        return await _context.Transactions
            .AsNoTracking()
            .Where(t => t.Id == transactionId)
            .Select(EntityToDtoMapper.TransactionToDto)
            .SingleOrDefaultAsync(ct);
    }
}