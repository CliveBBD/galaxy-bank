using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Shared
{
    public static class Constants
    {
        public static string ConnectionString = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION_STRING") ?? "Host=localhost,Port=5432;Database=gbank;Username=postgres;Password=root;";
        public static string AdminRoleName = Environment.GetEnvironmentVariable("ADMIN_ROLE_NAME") ?? "admin";
        public static int DisputeAcceptedId = int.TryParse(Environment.GetEnvironmentVariable("ACCEPTED_STATUS_ID"), out var result) ? result : 3;

    }
}