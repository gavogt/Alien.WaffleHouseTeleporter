namespace Alien.WaffleHouseTeleporter.Models
{
    public sealed class CoordinateResult
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string FormattedAddress { get; set; } = string.Empty;
    }
}
