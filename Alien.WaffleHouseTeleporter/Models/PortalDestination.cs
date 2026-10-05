namespace Alien.WaffleHouseTeleporter.Models;

public sealed record PortalDestination(string Id, string Name, string Address,
    double Latitude, double Longitude);

public sealed record LocationCatalog(string Source, string License, string RetrievedAt,
    List<PortalDestination> Locations);
