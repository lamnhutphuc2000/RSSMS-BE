using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace RSSMS.API.App_Start
{
    public static class JwtBearerConfig
    {
        public static void ConfigJwtBearer(this IServiceCollection services)
        {
            //string secret = "https://securetoken.google.com/wafayu-82753";
            string secret = "this is secret key mot hai ba bon nam sau bay";
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                //.AddJwtBearer(options =>
                //{
                //    options.Authority = "https://securetoken.google.com/wafayu-82753";
                //    options.TokenValidationParameters = new TokenValidationParameters
                //    {
                //        ValidateIssuer = true,
                //        ValidIssuer = "https://securetoken.google.com/wafayu-82753",
                //        ValidateAudience = true,
                //        ValidAudience = "wafayu-82753",
                //        ValidateLifetime = true
                //    };
                //});
                .AddJwtBearer(x =>
                {
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret)),
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidIssuer = "this is issuer fht mot hai",
                        ValidAudience = "this is issuer fht mot hai"
                    };
                });
        }
    }
}
