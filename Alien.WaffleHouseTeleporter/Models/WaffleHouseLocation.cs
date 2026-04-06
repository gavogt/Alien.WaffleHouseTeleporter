namespace Alien.WaffleHouseTeleporter.Models
{
    public class WaffleHouseLocation
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PlaceId { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double DistanceMiles { get; set; }
    }
}
