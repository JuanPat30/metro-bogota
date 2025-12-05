using Commun.Helpers;
using DependencyInjection;
using HistoricoChatMetro.Config;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        builder =>
        {
            builder
                .WithOrigins(
                    "https://einer.workstation.loc",
                    "http://localhost:3000",
                    "https://iadev.metrodebogota.gov.co",
                    "https://ia.metrodebogota.gov.co"
                )
                .AllowCredentials()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

builder.Services.AddControllers(options =>
{
    options.Conventions.Insert(0, new GlobalRoutePrefixConvention(new RouteAttribute("chat-services")));
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition(JwtSettings.Jwt_AuthCookieName, new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Scheme = "",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Cookie,
        Description = "Enter your valid token.\n\nExample: `abc123`"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

#region Connection String  

builder.Services.ConexionFireBase();

#endregion

#region Connection Cloud Storage

builder.Services.ConnectionCloudStorage();

#endregion

#region Services Autentication  

builder.Services.AutenticacionJwt(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#endregion

#region Services Injected  

builder.Services.InyeccionServicios(builder.Configuration);

#endregion

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    // Production error handling
    app.UseExceptionHandler("/Error");
    app.UseHsts(); // Enforces HTTPS
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseStaticFiles();

app.MapControllers();

app.UseCors("AllowFrontend");

app.Run();
