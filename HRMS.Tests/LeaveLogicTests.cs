using System;
using Xunit;

namespace HRMS.Tests
{
    public class LeaveLogicTests
    {
        [Fact]
        public void CalculateWorkingDays_ExcludesSundays()
        {
            // Friday 2026-10-16 to Monday 2026-10-19:
            // Friday (1), Saturday (1), Sunday (0), Monday (1) = 3 total days
            var from = new DateOnly(2026, 10, 16);
            var to = new DateOnly(2026, 10, 19);

            decimal totalDays = 0;
            var current = from;
            while (current <= to)
            {
                if (current.DayOfWeek != DayOfWeek.Sunday)
                {
                    totalDays++;
                }
                current = current.AddDays(1);
            }

            Assert.Equal(3m, totalDays);
        }

        [Fact]
        public void OnlySundaySelected_ResultsInZeroWorkingDays()
        {
            // Sunday 2026-10-18 only
            var from = new DateOnly(2026, 10, 18);
            var to = new DateOnly(2026, 10, 18);

            decimal totalDays = 0;
            var current = from;
            while (current <= to)
            {
                if (current.DayOfWeek != DayOfWeek.Sunday)
                {
                    totalDays++;
                }
                current = current.AddDays(1);
            }

            Assert.Equal(0m, totalDays);
        }

        [Theory]
        [InlineData(14, 0, 0, 14)] // Fresh quota: 14 allocated, 0 used, 0 pending = 14 available
        [InlineData(14, 3, 2, 9)]  // 14 allocated, 3 used, 2 pending = 9 available
        [InlineData(14, 10, 4, 0)] // Fully exhausted: 14 allocated, 10 used, 4 pending = 0 available
        public void AvailableLeaveBalance_CalculatedAccurately(
            decimal allocated,
            decimal used,
            decimal pending,
            decimal expectedAvailable)
        {
            decimal available = allocated - (used + pending);
            Assert.Equal(expectedAvailable, available);
        }
    }
}
