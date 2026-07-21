using FleetOS.Application.Common.Interfaces;
using FleetOS.Application.Inventory;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Inventory;
using FleetOS.Shared.Pagination;
using Microsoft.EntityFrameworkCore;

namespace FleetOS.Infrastructure.Persistence.Repositories;

internal sealed class ProductCategoryRepository : IProductCategoryRepository
{
    private readonly FleetOsDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public ProductCategoryRepository(FleetOsDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task AddAsync(ProductCategory entity, CancellationToken cancellationToken = default)
        => await _dbContext.Set<ProductCategory>().AddAsync(entity, cancellationToken);

    public void Update(ProductCategory entity) => _dbContext.Set<ProductCategory>().Update(entity);
    public void Remove(ProductCategory entity) => _dbContext.Set<ProductCategory>().Remove(entity);

    public async Task<IReadOnlyList<ProductCategory>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Set<ProductCategory>().Where(c => c.TenantId == _tenantContext.TenantId).ToListAsync(cancellationToken);

    public async Task<ProductCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbContext.Set<ProductCategory>().FirstOrDefaultAsync(c => c.TenantId == _tenantContext.TenantId && c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<ProductCategoryDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<ProductCategory>()
            .Where(c => c.TenantId == _tenantContext.TenantId)
            .Select(c => new ProductCategoryDto(c.Id, c.Name, c.Description, c.CreatedAt, c.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}

internal sealed class ProductRepository : IProductRepository
{
    private readonly FleetOsDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public ProductRepository(FleetOsDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task AddAsync(Product entity, CancellationToken cancellationToken = default)
        => await _dbContext.Set<Product>().AddAsync(entity, cancellationToken);

    public void Update(Product entity) => _dbContext.Set<Product>().Update(entity);
    public void Remove(Product entity) => _dbContext.Set<Product>().Remove(entity);

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Set<Product>().Where(p => p.TenantId == _tenantContext.TenantId).ToListAsync(cancellationToken);

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbContext.Set<Product>().FirstOrDefaultAsync(p => p.TenantId == _tenantContext.TenantId && p.Id == id, cancellationToken);

    public async Task<ProductDto?> GetProductByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Product>()
            .Where(p => p.TenantId == _tenantContext.TenantId && p.Id == id)
            .Join(_dbContext.Set<ProductCategory>(), p => p.CategoryId, c => c.Id, (p, c) => new ProductDto(
                p.Id, p.CategoryId, c.Name, p.Name, p.SKU, p.Description, p.AverageUnitPrice, p.CreatedAt, p.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<ProductDto>> GetPaginatedProductsAsync(int page, int pageSize, string? searchTerm, Guid? categoryId, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<Product>().Where(p => p.TenantId == _tenantContext.TenantId);

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(p => p.Name.Contains(searchTerm) || (p.SKU != null && p.SKU.Contains(searchTerm)));

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Join(_dbContext.Set<ProductCategory>(), p => p.CategoryId, c => c.Id, (p, c) => new ProductDto(
                p.Id, p.CategoryId, c.Name, p.Name, p.SKU, p.Description, p.AverageUnitPrice, p.CreatedAt, p.UpdatedAt))
            .ToListAsync(cancellationToken);

        return PagedResult<ProductDto>.Create(items, totalCount, page, pageSize);
    }
}

internal sealed class StockBalanceRepository : IStockBalanceRepository
{
    private readonly FleetOsDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public StockBalanceRepository(FleetOsDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task AddAsync(StockBalance entity, CancellationToken cancellationToken = default)
        => await _dbContext.Set<StockBalance>().AddAsync(entity, cancellationToken);

    public void Update(StockBalance entity) => _dbContext.Set<StockBalance>().Update(entity);
    public void Remove(StockBalance entity) => _dbContext.Set<StockBalance>().Remove(entity);

    public async Task<IReadOnlyList<StockBalance>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Set<StockBalance>().Where(s => s.TenantId == _tenantContext.TenantId).ToListAsync(cancellationToken);

    public async Task<StockBalance?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbContext.Set<StockBalance>().FirstOrDefaultAsync(s => s.TenantId == _tenantContext.TenantId && s.Id == id, cancellationToken);

    public async Task<StockBalance?> GetStockBalanceAsync(Guid productId, LocationType locationType, Guid? vehicleId, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<StockBalance>()
            .Where(s => s.TenantId == _tenantContext.TenantId && s.ProductId == productId && s.LocationType == locationType);
            
        if (locationType == LocationType.Vehicle)
            query = query.Where(s => s.VehicleId == vehicleId);
            
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<StockBalanceDto>> GetMainStockAsync(int page, int pageSize, string? searchTerm, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<StockBalance>()
            .Where(s => s.TenantId == _tenantContext.TenantId && s.LocationType == LocationType.Main);

        var joinQuery = query.Join(_dbContext.Set<Product>(), s => s.ProductId, p => p.Id, (s, p) => new { s, p });

        if (!string.IsNullOrWhiteSpace(searchTerm))
            joinQuery = joinQuery.Where(x => x.p.Name.Contains(searchTerm) || (x.p.SKU != null && x.p.SKU.Contains(searchTerm)));

        var totalCount = await joinQuery.CountAsync(cancellationToken);

        var items = await joinQuery
            .OrderBy(x => x.p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new StockBalanceDto(
                x.s.Id, x.s.ProductId, x.p.Name, x.s.LocationType, x.s.VehicleId, null, x.s.Quantity, x.s.MinimumStockLevel, x.s.IsBelowMinimum, x.s.CreatedAt, x.s.UpdatedAt))
            .ToListAsync(cancellationToken);

        return PagedResult<StockBalanceDto>.Create(items, totalCount, page, pageSize);
    }

    public async Task<PagedResult<StockBalanceDto>> GetVehicleStockAsync(Guid vehicleId, int page, int pageSize, string? searchTerm, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<StockBalance>()
            .Where(s => s.TenantId == _tenantContext.TenantId && s.LocationType == LocationType.Vehicle && s.VehicleId == vehicleId);

        var joinQuery = query.Join(_dbContext.Set<Product>(), s => s.ProductId, p => p.Id, (s, p) => new { s, p })
                             .Join(_dbContext.Set<FleetOS.Domain.Fleet.Vehicles.Vehicle>(), x => x.s.VehicleId, v => v.Id, (x, v) => new { x.s, x.p, v });

        if (!string.IsNullOrWhiteSpace(searchTerm))
            joinQuery = joinQuery.Where(x => x.p.Name.Contains(searchTerm) || (x.p.SKU != null && x.p.SKU.Contains(searchTerm)));

        var totalCount = await joinQuery.CountAsync(cancellationToken);

        var items = await joinQuery
            .OrderBy(x => x.p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new StockBalanceDto(
                x.s.Id, x.s.ProductId, x.p.Name, x.s.LocationType, x.s.VehicleId, x.v.LicensePlate, x.s.Quantity, x.s.MinimumStockLevel, x.s.IsBelowMinimum, x.s.CreatedAt, x.s.UpdatedAt))
            .ToListAsync(cancellationToken);

        return PagedResult<StockBalanceDto>.Create(items, totalCount, page, pageSize);
    }

    public async Task<IReadOnlyList<StockBalanceDto>> GetStockAlertsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<StockBalance>()
            .Where(s => s.TenantId == _tenantContext.TenantId && s.Quantity <= s.MinimumStockLevel)
            .Join(_dbContext.Set<Product>(), s => s.ProductId, p => p.Id, (s, p) => new { s, p })
            .Select(x => new StockBalanceDto(
                x.s.Id, x.s.ProductId, x.p.Name, x.s.LocationType, x.s.VehicleId, null, x.s.Quantity, x.s.MinimumStockLevel, x.s.IsBelowMinimum, x.s.CreatedAt, x.s.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}

internal sealed class InventoryMovementRepository : IInventoryMovementRepository
{
    private readonly FleetOsDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public InventoryMovementRepository(FleetOsDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task AddAsync(InventoryMovement entity, CancellationToken cancellationToken = default)
        => await _dbContext.Set<InventoryMovement>().AddAsync(entity, cancellationToken);

    public void Update(InventoryMovement entity) => _dbContext.Set<InventoryMovement>().Update(entity);
    public void Remove(InventoryMovement entity) => _dbContext.Set<InventoryMovement>().Remove(entity);

    public async Task<IReadOnlyList<InventoryMovement>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Set<InventoryMovement>().Where(m => m.TenantId == _tenantContext.TenantId).ToListAsync(cancellationToken);

    public async Task<InventoryMovement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbContext.Set<InventoryMovement>().FirstOrDefaultAsync(m => m.TenantId == _tenantContext.TenantId && m.Id == id, cancellationToken);

    public async Task<PagedResult<InventoryMovementDto>> GetMovementsByProductAsync(Guid productId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<InventoryMovement>()
            .Where(m => m.TenantId == _tenantContext.TenantId && m.ProductId == productId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(m => m.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Join(_dbContext.Set<Product>(), m => m.ProductId, p => p.Id, (m, p) => new InventoryMovementDto(
                m.Id, m.ProductId, p.Name, m.Type, m.FromLocationType, m.FromVehicleId, m.ToLocationType, m.ToVehicleId, m.Quantity, m.Date, m.Notes, m.ReferenceId, m.CreatedAt))
            .ToListAsync(cancellationToken);

        return PagedResult<InventoryMovementDto>.Create(items, totalCount, page, pageSize);
    }
}
