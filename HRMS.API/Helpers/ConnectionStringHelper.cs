using System;
using Microsoft.Extensions.Configuration;

namespace HRMS.API.Helpers
{
    public static class ConnectionStringHelper
    {
        /// <summary>
        /// Resolves and normalizes PostgreSQL / Supabase connection strings from configuration or environment variables.
        /// Supports standard connection strings, DATABASE_URL, SUPABASE_CONNECTION_STRING, and postgres:// URIs.
        /// </summary>
        public static string ResolveConnectionString(IConfiguration configuration)
        {
            // 1. Check explicit cloud environment variables FIRST (Render, Supabase, Railway)
            var connStr = configuration["DATABASE_URL"] ?? configuration["SUPABASE_CONNECTION_STRING"];

            // 2. Fallback to ConnectionStrings:DefaultConnection
            if (string.IsNullOrWhiteSpace(connStr))
            {
                connStr = configuration.GetConnectionString("DefaultConnection");
            }

            if (string.IsNullOrWhiteSpace(connStr))
            {
                throw new InvalidOperationException(
                    "Database connection string is not configured. Set 'DATABASE_URL' or 'ConnectionStrings__DefaultConnection'.");
            }

            return NormalizeConnectionString(connStr);
        }

        /// <summary>
        /// Normalizes a connection string. If formatted as a URI (postgres:// or postgresql://), converts it to Npgsql key-value format.
        /// Ensures SSL Mode=Require for remote/cloud hosts.
        /// </summary>
        public static string NormalizeConnectionString(string rawConnectionString)
        {
            if (string.IsNullOrWhiteSpace(rawConnectionString))
            {
                throw new ArgumentException("Connection string cannot be null or empty.", nameof(rawConnectionString));
            }

            var connStr = rawConnectionString.Trim();

            // Convert postgres:// or postgresql:// URL to Npgsql key-value format if provided
            if (connStr.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
                connStr.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
            {
                var uri = new Uri(connStr);
                var userInfo = uri.UserInfo.Split(':');
                var username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : "";
                var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
                var database = uri.AbsolutePath.TrimStart('/');
                var port = uri.Port > 0 ? uri.Port : 5432;

                connStr = $"Host={uri.Host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true;";
            }
            else if (!connStr.Contains("SSL Mode", StringComparison.OrdinalIgnoreCase) &&
                     !connStr.Contains("localhost", StringComparison.OrdinalIgnoreCase) &&
                     !connStr.Contains("127.0.0.1", StringComparison.OrdinalIgnoreCase))
            {
                connStr = connStr.TrimEnd(';') + ";SSL Mode=Require;Trust Server Certificate=true;";
            }

            return connStr;
        }

        /// <summary>
        /// Sanitizes a connection string for safe diagnostic logging (masks passwords).
        /// </summary>
        public static string SanitizeForLogging(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) return string.Empty;

            var parts = connectionString.Split(';');
            var sanitizedParts = new System.Collections.Generic.List<string>();

            foreach (var part in parts)
            {
                if (string.IsNullOrWhiteSpace(part)) continue;
                var eqIdx = part.IndexOf('=');
                if (eqIdx > 0)
                {
                    var key = part.Substring(0, eqIdx).Trim();
                    if (key.Equals("Password", StringComparison.OrdinalIgnoreCase) ||
                        key.Equals("Pwd", StringComparison.OrdinalIgnoreCase))
                    {
                        sanitizedParts.Add($"{key}=***REDACTED***");
                    }
                    else
                    {
                        sanitizedParts.Add(part.Trim());
                    }
                }
                else
                {
                    sanitizedParts.Add(part.Trim());
                }
            }

            return string.Join("; ", sanitizedParts);
        }
    }
}
