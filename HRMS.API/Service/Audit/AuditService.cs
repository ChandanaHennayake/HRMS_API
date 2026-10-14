using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using HRMS.API.DTOs.Audit;
using HRMS.API.Entities;
using HRMS.API.Repository.Audit;
using Microsoft.Extensions.Logging;

namespace HRMS.API.Service.Audit
{
    public class AuditService : IAuditService
    {
        private readonly IAuditRepository _repository;
        private readonly ILogger<AuditService> _logger;

        public AuditService(IAuditRepository repository, ILogger<AuditService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<AuditLogDto> LogAsync(CreateAuditLogDto dto)
        {
            try
            {
                var entity = new AuditLog
                {
                    UserId = dto.UserId,
                    UserName = dto.UserName,
                    Action = dto.Action,
                    EntityName = dto.EntityName,
                    EntityId = dto.EntityId,
                    Description = dto.Description,
                    OldValues = dto.OldValues,
                    NewValues = dto.NewValues,
                    IpAddress = dto.IpAddress,
                    UserAgent = dto.UserAgent,
                    Severity = string.IsNullOrWhiteSpace(dto.Severity) ? "Info" : dto.Severity,
                    CreatedAt = DateTime.UtcNow
                };

                var created = await _repository.CreateAsync(entity);
                return MapToDto(created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist audit log for Action {Action} on Entity {Entity}", dto.Action, dto.EntityName);
                return new AuditLogDto
                {
                    Action = dto.Action,
                    EntityName = dto.EntityName,
                    Description = dto.Description,
                    Severity = dto.Severity,
                    CreatedAt = DateTime.UtcNow
                };
            }
        }

        public async Task<AuditLogDto?> GetByIdAsync(long id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return entity == null ? null : MapToDto(entity);
        }

        public async Task<PagedAuditResultDto> GetPagedAsync(AuditLogQueryDto query)
        {
            var (items, totalCount) = await _repository.GetPagedAsync(query);

            return new PagedAuditResultDto
            {
                Items = items.Select(MapToDto),
                TotalCount = totalCount,
                Page = query.Page <= 0 ? 1 : query.Page,
                PageSize = query.PageSize <= 0 ? 30 : query.PageSize
            };
        }

        public async Task<AuditStatsDto> GetStatsAsync()
        {
            return await _repository.GetStatsAsync();
        }

        public async Task<IEnumerable<string>> GetActionsAsync()
        {
            return await _repository.GetDistinctActionsAsync();
        }

        public async Task<IEnumerable<string>> GetEntitiesAsync()
        {
            return await _repository.GetDistinctEntitiesAsync();
        }

        public async Task LogSecurityEventAsync(int? userId, string? userName, string action, string description, string? ipAddress, bool isSuccess = true)
        {
            var dto = new CreateAuditLogDto
            {
                UserId = userId,
                UserName = userName,
                Action = action,
                EntityName = "Security",
                EntityId = userId?.ToString(),
                Description = description,
                IpAddress = ipAddress,
                Severity = isSuccess ? "Info" : "Security"
            };

            await LogAsync(dto);
        }

        public async Task LogEntityActionAsync(int? userId, string? userName, string action, string entityName, string entityId, string description, object? oldValues = null, object? newValues = null, string severity = "Info")
        {
            string? oldJson = null;
            string? newJson = null;

            try
            {
                if (oldValues != null)
                {
                    oldJson = oldValues is string s ? s : JsonSerializer.Serialize(oldValues);
                }
                if (newValues != null)
                {
                    newJson = newValues is string s ? s : JsonSerializer.Serialize(newValues);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to serialize audit log values for {EntityName}:{EntityId}", entityName, entityId);
            }

            var dto = new CreateAuditLogDto
            {
                UserId = userId,
                UserName = userName,
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                Description = description,
                OldValues = oldJson,
                NewValues = newJson,
                Severity = severity
            };

            await LogAsync(dto);
        }

        private static AuditLogDto MapToDto(AuditLog a)
        {
            return new AuditLogDto
            {
                Id = a.Id,
                UserId = a.UserId,
                UserName = a.UserName ?? a.User?.Username,
                Action = a.Action,
                EntityName = a.EntityName,
                EntityId = a.EntityId,
                Description = a.Description,
                OldValues = a.OldValues,
                NewValues = a.NewValues,
                IpAddress = a.IpAddress,
                UserAgent = a.UserAgent,
                Severity = a.Severity,
                CreatedAt = a.CreatedAt
            };
        }
    }
}
