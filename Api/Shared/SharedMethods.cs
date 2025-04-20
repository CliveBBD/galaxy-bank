using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Api.Shared
{
    public static class SharedMethods
    {
        public static string? GetAndParseEnvironmentVariable(string key, string defaultValue)
        {
            return GetValueByKey(Environment.GetEnvironmentVariable(key), key) ?? default;
        }

        public static string? GetValueByKey(string? jsonStringOrTextValue, string key)
        {
            if (jsonStringOrTextValue == null)
            {
                return null;
            }
            else
            {
                try
                {
                    using JsonDocument doc = JsonDocument.Parse(jsonStringOrTextValue);
                    if (doc.RootElement.TryGetProperty(key, out JsonElement value))
                    {
                        return value.ToString();
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (System.Text.Json.JsonException)
                {
                    return jsonStringOrTextValue;
                }
            }
        }
    }
}