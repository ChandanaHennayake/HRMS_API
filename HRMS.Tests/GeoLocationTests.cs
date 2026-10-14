using HRMS.API.Helpers;
using Xunit;

namespace HRMS.Tests
{
    public class GeoLocationTests
    {
        [Fact]
        public void CalculateDistance_SameCoordinates_ReturnsZero()
        {
            double lat = 6.9271;
            double lon = 79.8612;

            double distance = GeoLocationHelper.CalculateDistance(lat, lon, lat, lon);

            Assert.True(distance < 0.001, $"Expected zero distance but got {distance}");
        }

        [Fact]
        public void CalculateDistance_WithinGeofence_ReturnsExpectedDistance()
        {
            // Abans Head Office Colombo reference coordinates: 6.9056, 79.8512
            // Point ~50 meters away: 6.9059, 79.8514
            double branchLat = 6.9056;
            double branchLon = 79.8512;
            double employeeLat = 6.9059;
            double employeeLon = 79.8514;

            double distance = GeoLocationHelper.CalculateDistance(branchLat, branchLon, employeeLat, employeeLon);

            // Haversine should compute between 35m and 60m
            Assert.True(distance > 20 && distance < 80, $"Calculated distance {distance}m is outside expected bounds.");
        }

        [Fact]
        public void CalculateDistance_OutsideGeofence_ExceedsThreshold()
        {
            // Colombo (6.9271, 79.8612) to Kandy (7.2906, 80.6337) is approx 95-100km
            double colomboLat = 6.9271;
            double colomboLon = 79.8612;
            double kandyLat = 7.2906;
            double kandyLon = 80.6337;

            double distance = GeoLocationHelper.CalculateDistance(colomboLat, colomboLon, kandyLat, kandyLon);

            // Distance should be > 90,000 meters (90 km)
            Assert.True(distance > 90000, $"Expected >90km but got {distance / 1000}km");
        }
    }
}
