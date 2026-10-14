using System;
using System.Collections.Generic;
using HRMS.API.Helpers;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace HRMS.Tests
{
    public class ConnectionStringHelperTests
    {
        [Fact]
        public void ResolveConnectionString_UsesDefaultConnection_WhenPresent()
        {
            var configData = new Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", "Host=db.supabase.co;Database=postgres;Username=postgres;Password=Secret123" }
            };
            var config = new ConfigurationBuilder().AddInMemoryCollection(configData).Build();

            var result = ConnectionStringHelper.ResolveConnectionString(config);

            Assert.Contains("Host=db.supabase.co", result);
            Assert.Contains("SSL Mode=Require", result);
        }

        [Fact]
        public void ResolveConnectionString_FallsBackToDatabaseUrl_WhenDefaultConnectionMissing()
        {
            var configData = new Dictionary<string, string?>
            {
                { "DATABASE_URL", "postgresql://postgres:SecretPassword@aws-0-ap-southeast-1.pooler.supabase.com:5432/postgres" }
            };
            var config = new ConfigurationBuilder().AddInMemoryCollection(configData).Build();

            var result = ConnectionStringHelper.ResolveConnectionString(config);

            Assert.Contains("Host=aws-0-ap-southeast-1.pooler.supabase.com", result);
            Assert.Contains("Port=5432", result);
            Assert.Contains("Database=postgres", result);
            Assert.Contains("Username=postgres", result);
            Assert.Contains("Password=SecretPassword", result);
            Assert.Contains("SSL Mode=Require", result);
        }

        [Fact]
        public void ResolveConnectionString_FallsBackToSupabaseConnectionString_WhenOthersMissing()
        {
            var configData = new Dictionary<string, string?>
            {
                { "SUPABASE_CONNECTION_STRING", "Host=db.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=Pass" }
            };
            var config = new ConfigurationBuilder().AddInMemoryCollection(configData).Build();

            var result = ConnectionStringHelper.ResolveConnectionString(config);

            Assert.Contains("Host=db.supabase.co", result);
            Assert.Contains("SSL Mode=Require", result);
        }

        [Fact]
        public void ResolveConnectionString_ThrowsInvalidOperationException_WhenNoConfiguredConnection()
        {
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();

            Assert.Throws<InvalidOperationException>(() => ConnectionStringHelper.ResolveConnectionString(config));
        }

        [Fact]
        public void NormalizeConnectionString_PreservesLocalhostWithoutForcingSSL()
        {
            var raw = "Host=localhost;Port=5432;Database=ABANS_BAN;Username=postgres;Password=test";
            var result = ConnectionStringHelper.NormalizeConnectionString(raw);

            Assert.DoesNotContain("SSL Mode=Require", result);
        }

        [Fact]
        public void SanitizeForLogging_RedactsPasswordProperly()
        {
            var raw = "Host=db.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=SuperSecretPassword123;SSL Mode=Require";
            var sanitized = ConnectionStringHelper.SanitizeForLogging(raw);

            Assert.DoesNotContain("SuperSecretPassword123", sanitized);
            Assert.Contains("Password=***REDACTED***", sanitized);
            Assert.Contains("Host=db.supabase.co", sanitized);
        }
    }
}
