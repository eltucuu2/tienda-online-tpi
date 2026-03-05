using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Dsw2025Tpi.Api.Helpers
{
    public static class AuthenticationAndAuthorizationExtensions
    {
        public static IServiceCollection AddAuthenticationAndAuthorization(this IServiceCollection services, IConfiguration config)
        {
            var jwtConfig = config.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtConfig["Key"] ?? throw new ArgumentNullException("JWT Key"));

            services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Password = new PasswordOptions { RequiredLength = 8 };
            })
            .AddEntityFrameworkStores<AuthenticateContext>()
            .AddDefaultTokenProviders();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtConfig["Issuer"],
                    ValidAudience = jwtConfig["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });

            services.AddAuthorization();
            services.AddSingleton<JwtTokenService>();

            return services;
        }
    }
}
