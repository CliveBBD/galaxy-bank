using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Auth;

namespace Api.Shared
{
    public class JwtDecoder
    {
        public static async Task<GoogleJsonWebSignature.Payload?> Decode(HttpContext httpContext) {
            var authHeader = httpContext.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var jwt = authHeader.Substring("Bearer ".Length).Trim();
                return await GoogleJsonWebSignature.ValidateAsync(jwt);
            }
            else
            {
                return null;
            }
        }

        public static async Task<GoogleJsonWebSignature.Payload?> Decode(string jwt) {
            return await GoogleJsonWebSignature.ValidateAsync(jwt);
        }
    }
}