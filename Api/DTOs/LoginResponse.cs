using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.DTOs
{
    public class LoginResponse (string authUrl, string sessionId)
    {
        public string AuthUrl { get; init; } = authUrl;
        public string SessionId { get; init; } = sessionId;
    }
}