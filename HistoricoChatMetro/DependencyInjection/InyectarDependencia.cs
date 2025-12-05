using Commun.Helpers;
using Commun.Logger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using RepositoryLayer.Data;
using RepositoryLayer.IRepository;
using RepositoryLayer.Repository;
using ServiceLayer.IService;
using ServiceLayer.Service;
using System.Text;

namespace DependencyInjection
{
    public static class InyectarDependencia
    {
        

        public static void ConexionFireBase(this IServiceCollection services)
        {
            services.AddSingleton<FirestoreDbContext>();
        }

        public static void ConnectionCloudStorage(this IServiceCollection services)
        {
            services.AddSingleton<CloudStorageClient>();
        }

        public static void AutenticacionJwt(this IServiceCollection services, IConfiguration Configuration)
        {
            /*
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = true;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.Zero,
                        ValidIssuer = JwtSettings.Jwt_Issuer,
                        ValidAudience = JwtSettings.Jwt_Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSettings.Jwt_SecretPassword))
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            if (context.Request.Cookies.TryGetValue(JwtSettings.Jwt_AuthCookieName, out var token))
                            {
                                context.Token = token;
                            }

                            return Task.CompletedTask;
                        }
                    };
                });
            */
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = true;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.Zero,

                        ValidIssuer = JwtSettings.Jwt_Issuer,
                        ValidAudience = JwtSettings.Jwt_Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(JwtSettings.Jwt_SecretPassword)
                        )
                    };
                });
        }

        public static void InyeccionServicios(this IServiceCollection services, IConfiguration Configuration)
        {
            services.AddScoped(typeof(ICreateLogger), typeof(CreateLogger));


            services.AddScoped(typeof(IRegisterRepository), typeof(RegisterRepository));
            services.AddScoped(typeof(IChatRepository), typeof(ChatRepository));

            services.AddScoped(typeof(IRegisterService), typeof(RegisterService));
            services.AddScoped(typeof(IChatService), typeof(ChatService));
            services.AddScoped(typeof(IEmailService), typeof(EmailService));
            services.AddScoped(typeof(IReportService), typeof(ReportService));
            services.AddScoped(typeof(IFileWrapper), typeof(FileWrapper));
            services.AddScoped(typeof(IFileStoreService), typeof(FileStoreService));

            services.AddScoped<Commun.ReportExcel>();
            services.AddScoped<Commun.PDFGenerator>();

            services.AddTransient<IAuthToken, AuthToken>();

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddSingleton<RsaKeyService>(
                provider => new RsaKeyService(Configuration)
            );
        }
    }
}