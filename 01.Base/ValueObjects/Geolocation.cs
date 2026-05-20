using Smlp.Base.Common;

namespace Smlp.Base.ValueObjects
{
    public class Geolocation : ValueObject
    {
        public double Latitude { get; }
        public double Longitude { get; }
        public double? Accuracy { get; }
        public string? City { get; }
        public string? Country { get; }

        public Geolocation(double latitude, double longitude, double? accuracy = null, string? city = null, string? country = null)
        {
            Latitude = latitude;
            Longitude = longitude;
            Accuracy = accuracy;
            City = city;
            Country = country;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Latitude;
            yield return Longitude;
        }

        public override string ToString()
            => $"{Latitude},{Longitude}";
    }
}
