using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Shared
{
    public static class Constants
    {
        public static string ConnectionString = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION_STRING") ?? "Host=localhost,Port=5432;Database=galaxy_bank;Username=postgres;Password=password;";

    }
}