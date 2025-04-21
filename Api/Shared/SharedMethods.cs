using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Api.Shared
{
    public static class SharedMethods
    {
        public static string? GetAndParseEnvironmentVariable(string key, string jsonKey)
        {
            var value = Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            try
            {
                using JsonDocument doc = JsonDocument.Parse(value);
                if (doc.RootElement.TryGetProperty(jsonKey, out JsonElement jsonValue))
                {
                    return jsonValue.GetString();
                }
            }
            catch (JsonException)
            {
                // If parsing fails, return the original value
                // This handles cases where the value is not JSON
                return value;
            }

            return value;
        }
    }
}