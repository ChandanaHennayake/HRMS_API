namespace HRMS.API.Helpers
{
    public static class GeoLocationHelper
    {
        private const double EarthRadiusMeters = 6371000;

        public static double CalculateDistance(
            double latitude1,
            double longitude1,
            double latitude2,
            double longitude2)
        {
            var lat1 = ToRadians(latitude1);
            var lat2 = ToRadians(latitude2);

            var deltaLatitude =
                ToRadians(latitude2 - latitude1);

            var deltaLongitude =
                ToRadians(longitude2 - longitude1);

            var a =
                Math.Sin(deltaLatitude / 2) *
                Math.Sin(deltaLatitude / 2) +
                Math.Cos(lat1) *
                Math.Cos(lat2) *
                Math.Sin(deltaLongitude / 2) *
                Math.Sin(deltaLongitude / 2);

            var c = 2 * Math.Atan2(
                Math.Sqrt(a),
                Math.Sqrt(1 - a));

            return EarthRadiusMeters * c;
        }

        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
    }
}