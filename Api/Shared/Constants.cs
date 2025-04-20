using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Shared
{
    public static class Constants
    {
        public static string ConnectionString = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION_STRING") ?? "Host=localhost,Port=5432;Database=gbank;Username=postgres;Password=root;";
        public static string DisputeOfficerRoleName = Environment.GetEnvironmentVariable("DISPUTE_OFFICER_ROLE_NAME") ?? "dispute_officer";
        public static string SystemAdminRoleName = Environment.GetEnvironmentVariable("SYSTEM_ADMIN_ROLE_NAME") ?? "system_admin";
        public static string DefaultRoleName = Environment.GetEnvironmentVariable("DEFAULT_ROLE_NAME") ?? "customer";
        public static int DisputeAcceptedId = int.TryParse(Environment.GetEnvironmentVariable("ACCEPTED_STATUS_ID"), out var result) ? result : 3;

    }
}