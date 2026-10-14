using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HRMS.API.DTOs.Audit;
using HRMS.API.Entities;
using HRMS.API.Repository.Audit;
using HRMS.API.Service.Audit;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HRMS.Tests
{
    public class AuditServiceTests
    {
        private class FakeAuditRepository : IAuditRepository
        {
            public readonly List<AuditLog> Store = new();

            public Task<AuditLog> CreateAsync(AuditLog log)
            {
                log.Id = Store.Count + 1;
                Store.Add(log);
                return Task.FromResult(log);
            }

            public Task<AuditLog?> GetByIdAsync(long id)
            {
                var item = Store.FirstOrDefault(x => x.Id == id);
                return Task.FromResult(item);
            }

            public Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetPagedAsync(AuditLogQueryDto query)
            {
                var q = Store.AsQueryable();
                if (!string.IsNullOrWhiteSpace(query.Action))
                {
                    q = q.Where(x => x.Action == query.Action);
                }
                var total = q.Count();
                var items = q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToList();
                return Task.FromResult(((IEnumerable<AuditLog>)items, total));
            }

            public Task<AuditStatsDto> GetStatsAsync()
            {
                return Task.FromResult(new AuditStatsDto
                {
                    TotalLogs = Store.Count,
                    SecurityCount = Store.Count(x => x.Severity == "Security"),
                    InfoCount = Store.Count(x => x.Severity == "Info")
                });
            }

            public Task<IEnumerable<string>> GetDistinctActionsAsync()
            {
                return Task.FromResult(Store.Select(x => x.Action).Distinct());
            }

            public Task<IEnumerable<string>> GetDistinctEntitiesAsync()
            {
                return Task.FromResult(Store.Select(x => x.EntityName).Distinct());
            }
        }

        [Fact]
        public async Task LogSecurityEvent_FailedLogin_Sets_Security_Severity()
        {
            var fakeRepo = new FakeAuditRepository();
            var service = new AuditService(fakeRepo, NullLogger<AuditService>.Instance);

            await service.LogSecurityEventAsync(1, "testuser", "LoginFailed", "Invalid password", "127.0.0.1", false);

            Assert.Single(fakeRepo.Store);
            var log = fakeRepo.Store.First();
            Assert.Equal("Security", log.Severity);
            Assert.Equal("LoginFailed", log.Action);
            Assert.Equal("127.0.0.1", log.IpAddress);
            Assert.Equal("testuser", log.UserName);
        }

        [Fact]
        public async Task LogEntityAction_Serializes_Values_Correctly()
        {
            var fakeRepo = new FakeAuditRepository();
            var service = new AuditService(fakeRepo, NullLogger<AuditService>.Instance);

            var oldVal = new { Status = "Pending" };
            var newVal = new { Status = "Approved" };

            await service.LogEntityActionAsync(5, "manager", "Approve", "LeaveRequest", "101", "Approved leave", oldVal, newVal, "Info");

            Assert.Single(fakeRepo.Store);
            var log = fakeRepo.Store.First();
            Assert.Equal("Approve", log.Action);
            Assert.Equal("LeaveRequest", log.EntityName);
            Assert.Equal("101", log.EntityId);
            Assert.Contains("Pending", log.OldValues);
            Assert.Contains("Approved", log.NewValues);
        }

        [Fact]
        public async Task GetPagedAsync_Calculates_Pagination_Properly()
        {
            var fakeRepo = new FakeAuditRepository();
            var service = new AuditService(fakeRepo, NullLogger<AuditService>.Instance);

            for (int i = 1; i <= 25; i++)
            {
                await service.LogAsync(new CreateAuditLogDto
                {
                    Action = "Test",
                    EntityName = "Item",
                    Description = $"Item {i}"
                });
            }

            var result = await service.GetPagedAsync(new AuditLogQueryDto { Page = 2, PageSize = 10 });

            Assert.Equal(25, result.TotalCount);
            Assert.Equal(3, result.TotalPages);
            Assert.Equal(10, result.Items.Count());
        }
    }
}
