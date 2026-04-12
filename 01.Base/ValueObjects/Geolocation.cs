using System.Globalization;
using SMLP.Base.Common;

namespace SMLP.Base.ValueObjects
{

    public class Geolocation : ValueObject
    {
        private const string Separator = "||";

        public double Latitude { get; init; }
        public double Longitude { get; init; }
        public double Accuracy { get; init; }

        public Geolocation(double latitude, double longitude, double accuracy)
        {
            Latitude = latitude;
            Longitude = longitude;
            Accuracy = accuracy;
        }

        public static Geolocation From(string geolocationText)
        {
            if (geolocationText is null)
            {
                throw new ArgumentNullException(nameof(geolocationText));
            }

            var splitted = geolocationText.Split(Separator);
            var expectedSplitLength = 3;

            return splitted.Length != expectedSplitLength
                ? throw new ArgumentException($"Provided argument cannot be splitted into {expectedSplitLength} parts.", nameof(geolocationText))
                : !double.TryParse(splitted[0], out var latitude)
                ? throw new ArgumentException($"First part of the provided argument cannot be parsed into {nameof(Latitude)}.", nameof(geolocationText))
                : !double.TryParse(splitted[1], out var longitude)
                ? throw new ArgumentException($"Second part of the provided argument cannot be parsed into {nameof(Longitude)}.", nameof(geolocationText))
                : !double.TryParse(splitted[1], out var accuracy)
                ? throw new ArgumentException($"Third part of the provided argument cannot be parsed into {nameof(Accuracy)}.", nameof(geolocationText))
                : new Geolocation(latitude, longitude, accuracy);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Latitude;
            yield return Longitude;
            yield return Accuracy;
        }

        public static implicit operator string(Geolocation geolocation) => geolocation.ToString();

        public static explicit operator Geolocation(string geolocationText) => From(geolocationText);

        public override string ToString()
        {
            return $"{Latitude}{Separator}{Longitude}{Separator}{Accuracy}";
        }

        public string LocationUrl
        {
            get
            {
                var latitude = Latitude.ToString(CultureInfo.InvariantCulture);
                var longitude = Longitude.ToString(CultureInfo.InvariantCulture);

                return $"https://www.google.com/maps/@{latitude},{longitude},20z";
            }
        }

        public string LocationText => $"{nameof(Latitude)}: {LatitudeText}, {nameof(Longitude)}: {LongitudeText} ° with {nameof(Accuracy).ToLower()}: {Accuracy:N2} meters";

        public string LatitudeText
        {
            get
            {
                var direction = "N";
                var latitude = Latitude;

                if (latitude < 0)
                {
                    direction = "S";
                    latitude *= -1;
                }

                var degrees = latitude - (latitude % 1);
                var minutes = (latitude % 1 * 60.0001) - (latitude % 1 * 60.0001 % 1);
                var seconds = Math.Round(latitude % 1 * 60 % 1 * 60 * 10000) / 10000;

                if (seconds == 60)
                {
                    seconds = 0;
                }

                return $"{degrees}°{minutes}'{string.Format("{0:0.00}", seconds)}\"{direction}";
            }
        }

        public string LongitudeText
        {
            get
            {
                var direction = "E";
                var longitude = Longitude;

                if (longitude < 0)
                {
                    direction = "W";
                    longitude *= -1;
                }

                var degrees = longitude - (longitude % 1);
                var minutes = (longitude % 1 * 60.0001) - (Longitude % 1 * 60.0001 % 1);
                var seconds = Math.Round(longitude % 1 * 60 % 1 * 60 * 10000) / 10000;

                if (seconds == 60)
                {
                    seconds = 0;
                }

                return $"{degrees}°{minutes}'{string.Format("{0:0.00}", seconds)}\"{direction}";
            }
        }
    }

}
