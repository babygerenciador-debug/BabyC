using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Inventory;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Inventory.Commands;

internal sealed class CreateProductCategoryCommandHandler : IRequestHandler<CreateProductCategoryCommand, Result<Guid>>
{
    private readonly IProductCategoryRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CreateProductCategoryCommandHandler(IProductCategoryRepository repository, IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(CreateProductCategoryCommand request, CancellationToken cancellationToken)
    {
        var categoryResult = ProductCategory.Create(
            _tenantContext.TenantId,
            _tenantContext.OrganizationId,
            _tenantContext.BusinessUnitId,
            request.Name,
            request.Description);

        if (categoryResult.IsFailure)
            return Result.Failure<Guid>(categoryResult.Error);

        await _repository.AddAsync(categoryResult.Value!, cancellationToken);
        await _unitOfWork.CommitAsync(_tenantContext.TenantId, cancellationToken);

        return Result.Success(categoryResult.Value!.Id);
    }
}

internal sealed class DeleteProductCategoryCommandHandler : IRequestHandler<DeleteProductCategoryCommand, Result>
{
    private readonly IProductCategoryRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public DeleteProductCategoryCommandHandler(IProductCategoryRepository repository, IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(DeleteProductCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (category is null) return Result.Failure(Error.NotFound("Category.NotFound", "Category not found."));

        _repository.Remove(category);
        await _unitOfWork.CommitAsync(_tenantContext.TenantId, cancellationToken);
        return Result.Success();
    }
}

internal sealed class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result>
{
    private readonly IProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public DeleteProductCommandHandler(IProductRepository repository, IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null) return Result.Failure(Error.NotFound("Product.NotFound", "Product not found."));

        _repository.Remove(product);
        await _unitOfWork.CommitAsync(_tenantContext.TenantId, cancellationToken);
        return Result.Success();
    }
}

internal sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    private readonly IProductRepository _repository;
    private readonly IProductCategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CreateProductCommandHandler(IProductRepository repository, IProductCategoryRepository categoryRepository, IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _repository = repository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null)
            return Result.Failure<Guid>(Error.NotFound("Category.NotFound", "Category not found."));

        var productResult = Product.Create(
            _tenantContext.TenantId,
            _tenantContext.OrganizationId,
            _tenantContext.BusinessUnitId,
            request.CategoryId,
            request.Name,
            request.SKU,
            request.Description,
            request.AverageUnitPrice);

        if (productResult.IsFailure)
            return Result.Failure<Guid>(productResult.Error);

        await _repository.AddAsync(productResult.Value!, cancellationToken);
        await _unitOfWork.CommitAsync(_tenantContext.TenantId, cancellationToken);

        return Result.Success(productResult.Value!.Id);
    }
}
