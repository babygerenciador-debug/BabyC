namespace FleetOS.Domain.Fleet.Vehicles;

public sealed class VehicleDocument
{
    private VehicleDocument() { } // EF Core

    internal VehicleDocument(Guid id, Guid vehicleId, string name, DateTime expirationDate, string fileUrl)
    {
        Id = id;
        VehicleId = vehicleId;
        Name = name;
        ExpirationDate = expirationDate.Date;
        FileUrl = fileUrl;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid VehicleId { get; private set; }
    public string Name { get; private set; } = default!;
    public DateTime ExpirationDate { get; private set; }
    public string FileUrl { get; private set; } = default!;
    public DateTimeOffset CreatedAt { get; private set; }

    public bool IsExpired() => ExpirationDate.Date < DateTime.UtcNow.Date;
}
