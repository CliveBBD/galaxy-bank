using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Models;

namespace Api.Shared
{
    public static class HttpContextUserExtensions
    {
        
        public static void SetCurrentUser(this HttpContext context, User? user)
        {
            context.Items["CurrentUser"] = user;
        }

        public static User? GetCurrentUser(this HttpContext context)
        {
            return context.Items.TryGetValue("CurrentUser", out var value) ? value as User: null;
        }
    }
}