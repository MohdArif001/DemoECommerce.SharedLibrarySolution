

using eCommerce.SharedLibrary.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace eCommerce.SharedLibrary.DepedencyInjection
{
    public static class SharedServiceContainer
    {
        public static IServiceCollection AddSharedService<TContext> 
            (this IServiceCollection services, IConfiguration config , string fileName)
            where TContext: DbContext
        {
            //ADD GENERIC DATABASE  CONTEXT
            services.AddDbContext<TContext>(option => option.UseSqlServer(
                config
                .GetConnectionString("eCommerceConnection"), sqlserverOption =>
                sqlserverOption.EnableRetryOnFailure()));

            // CONFIGURE SERILOG LOGGING
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
               .WriteTo.Debug()
               .WriteTo.Console()
               .WriteTo.File(path: $"{fileName}- .text",
               restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
               outputTemplate: "{Timestamp:yyyy-mm-dd hh:mm:ss:ff zzz} [{Level:u3}]{message:lj}{NewLine}{Exception}",
               rollingInterval: RollingInterval.Day)
               .CreateLogger();

            // ADD JWT AUTHENTICATION SCHEME 
            JWTAuthenticationScheme.AddJWTAuthenticationScheme(services, config);
            return services;    
        }

        public static IApplicationBuilder UseSharedPolicies(this IApplicationBuilder app)
        {
            // USE GLOBAL EXCEPTION 
            app.UseMiddleware<GlobalException>();

            //REGISTER MIDDLEWARE TO BLOCK OUTSIDER API CALLS
            app.UseMiddleware<ListenToOnlyApiGateway>();
            return app;
        }
    }
}
