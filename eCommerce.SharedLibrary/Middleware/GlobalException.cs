using eCommerce.SharedLibrary.Logs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace eCommerce.SharedLibrary.Middleware
{
    public class GlobalException(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            //DECLARE  DEFAULT VARIABLE
            string message = "sorry , internal server error occurred . kindly try again";
            int statusCode = (int)HttpStatusCode.InternalServerError;
            string title = "Error";
            try
            {
                await next(context);

                // Check if response too many request // 429 Status Code
                if (context.Response.StatusCode == StatusCodes.Status429TooManyRequests)
                {
                    title = "Warning";
                    message = "Too many request made";
                    statusCode = StatusCodes.Status429TooManyRequests;
                    await ModifyHeader(context, title, message, statusCode);
                }

                //IF RESPONSE IS UNAUTHORISED // 401 STATUS CODE
                if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
                {
                    title = " Alert";
                    message = " You are not authorized to access.";
                    statusCode = StatusCodes.Status401Unauthorized;
                    await ModifyHeader(context, title, message, statusCode);
                }
                // IF RESPONSE IS FORBIDDEN // 403 STAUS CODE
                if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
                {
                    title = "  out of service";
                    message = " You are not Allow to access.";
                    statusCode = StatusCodes.Status403Forbidden;
                    await ModifyHeader(context, title, message, statusCode);
                }
            }
            catch (Exception ex)
            {
                // LOG ORIGINAL EXCEPTION / FILE , DEBUGGER , CONSOLE
                LogException.LogExceptions(ex);

                // CHECK  IF EXCEPTION IS TIMEOUT // 408 REQUEST TIMEOUT
                if (ex is TaskCanceledException || ex is TimeoutException )
                {
                    title = "Out of time";
                    message = "Request time out , try again";
                    statusCode = StatusCodes.Status408RequestTimeout;

                }
                // IF EXCEPION IS CAUGHT.
                //IF NONE OF HE EXCEPTIONS THEN DO THE DEFAULT
                await ModifyHeader(context , title, message,statusCode);
            }
        }

        private async Task ModifyHeader(HttpContext context, string title, string message, int statusCode)
        {
           // display scary -free message to clint
           context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new ProblemDetails()
            {
                Detail = message,
                Status = statusCode,
                Title = title,

            }),CancellationToken.None);
            return;
        }
    }
}
