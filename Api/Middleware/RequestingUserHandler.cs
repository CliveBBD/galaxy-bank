using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Services;
using Api.Shared;

namespace Api.Middleware
{
    public class RequestingUserHandler(IUserService userService) : IMiddleware
    {
        private readonly IUserService _userService = userService;

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                var user = await _userService.GetCurrentUser(context);
                context.SetCurrentUser(user);
            }
            catch(Google.Apis.Auth.InvalidJwtException)
            {
                // the provided JWT was invalid, we wont be able to get the current user, let the individual controllers handle that
            }
            finally
            {
                await next(context);
            }
        }
    }
}