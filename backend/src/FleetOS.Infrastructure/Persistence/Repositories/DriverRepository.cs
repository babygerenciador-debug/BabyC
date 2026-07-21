using FleetOS.Application.Common.Interfaces;
using FleetOS.Application.Operations.Drivers;
using FleetOS.Domain.Operations.Drivers;
using FleetOS.Shared.Pagination;
using Microsoft.EntityFrameworkCore;

namespace FleetOS.Infrastructure.Persistence.Repositories;

internal sealed class DriverRepository : BaseRepository<Driver>, IDriverRepository
{
    public DriverRepository(FleetOsDbContext context) : base(context)
    {
    }

    public async Task<Driver?> GetByCnhAsync(string cnhNumber, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(d => d.CnhNumber == cnhNumber, cancellationToken);
    }

    public async Task<Driver?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);
    }

    public async Task<DriverDto?> GetDriverByIdWithUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var query = from d in DbSet
                    join u in DbContext.Users on d.UserId equals u.Id
                    where d.Id == id
                    select new DriverDto(
                        d.Id,
                        d.UserId,
                        u.Name,
                        u.EmailAddress,
                        u.CpfLast4 ?? "",
                        d.CnhNumber,
                        d.CnhCategory,
                        d.CnhExpirationDate,
                        d.CnhExpirationDate < DateTime.UtcNow,
                        d.Status.ToString(),
                        d.Phone,
                        d.PhotoUrl,
                        d.IsAvailable,
                        d.CreatedAt
                    );

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<DriverDto>> GetPaginatedDriversAsync(
        int page, int pageSize, string? searchTerm, string? status, CancellationToken cancellationToken = default)
    {
        var query = from d in DbSet
                    join u in DbContext.Users on d.UserId equals u.Id
                    select new { Driver = d, User = u };

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearch = searchTerm.ToLower();
            query = query.Where(x => 
                x.User.Name.ToLower().Contains(lowerSearch) || 
                x.Driver.CnhNumber.Contains(searchTerm) || 
                x.User.CpfLast4.Contains(searchTerm));
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<DriverStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(x => x.Driver.Status == parsedStatus);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.Driver.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new DriverDto(
                x.Driver.Id,
                x.Driver.UserId,
                x.User.Name,
                x.User.EmailAddress,
                x.User.CpfLast4 ?? "",
                x.Driver.CnhNumber,
                x.Driver.CnhCategory,
                x.Driver.CnhExpirationDate,
                x.Driver.CnhExpirationDate < DateTime.UtcNow,
                x.Driver.Status.ToString(),
                x.Driver.Phone,
                x.Driver.PhotoUrl,
                x.Driver.IsAvailable,
                x.Driver.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return PagedResult<DriverDto>.Create(items, totalCount, page, pageSize);
    }
}
