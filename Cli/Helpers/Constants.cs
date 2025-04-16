using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Cli.Helpers
{
    public static class Constants
    {
        public static IConfigurationRoot Config { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();
        public static string ApiBaseUrl { get; } = Config["ApiBaseUrl"] ?? "";
    }
}