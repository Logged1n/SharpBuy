using System.Globalization;
using System.Text;
using Application.Abstractions.Authentication;
using Application.Abstractions.BackgroundJobs;
using Application.Abstractions.Caching;
using Application.Abstractions.Data;
using Application.Abstractions.Emails;
using Application.Abstractions.Payments;
using Application.Abstractions.Reporting;
using Application.Abstractions.Storage;
using Infrastructure.Authentication;
using Infrastructure.BackgroundJobs;
using Infrastructure.Caching;
using Infrastructure.Database;
using Infrastructure.DomainEvents;
using Infrastructure.Payments;
using Infrastructure.Reporting;
using Infrastructure.Storage;
using Infrastructure.Time;
using Infrastructure.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SharedKernel;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services
            .AddServices()
            .AddDatabase(configuration)
            .AddCaching(configuration)
            .AddBackgroundJobs()
            .AddAuthenticationInternal(configuration)
            .AddAuthorizationInternal();

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddOptions();
        services.AddOptions<EmailOptions>()
                  .Configure<IConfiguration>((configSection, configuration) =>
                    configuration.GetSection("EmailOptions").Bind(configSection));
        services.AddScoped<IEmailVerificationLinkFactory, EmailVerificationLinkFactory>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<IPaymentService, StripePaymentService>();
        services.AddScoped<IPdfGenerator, RazorPdfGenerator>();
        services.AddScoped<ICachedPdfGenerator, CachedPdfGeneratorService>();

        services.AddTransient<IDomainEventsDispatcher, DomainEventsDispatcher>();

        services.AddCors(options => options.AddDefaultPolicy(policy => policy
        .AllowAnyHeader()
        .AllowAnyMethod()
        .WithOrigins("http://localhost:3000")));

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("SharpBuy");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Default);
                npgsqlOptions.UseRelationalNulls(false);
            }));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        return services;
    }

    private static IServiceCollection AddAuthenticationInternal(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddIdentityCore<ApplicationUser>(options => {
            options.User.RequireUniqueEmail = true;
            options.Password.RequireDigit = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 0;
            options.Password.RequireLowercase = false;
            options.Password.RequiredUniqueChars = 0;
            options.Password.RequireUppercase = false;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        string? googleClientId = configuration["Authentication:Google:ClientId"];
        string? googleClientSecret = configuration["Authentication:Google:ClientSecret"];
        string? jwtSecret = configuration["Jwt:Secret"];

        AuthenticationBuilder authBuilder = services.AddAuthentication(options => {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddCookie();

        // Only add Google auth if credentials are configured
        if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
        {
            authBuilder.AddGoogle(options =>
            {
                options.ClientId = googleClientId;
                options.ClientSecret = googleClientSecret;
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });
        }

        if (!string.IsNullOrEmpty(jwtSecret))
        {
            authBuilder.AddJwtBearer(o =>
            {
                o.RequireHttpsMetadata = false;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    ClockSkew = TimeSpan.Zero
                };
            });
        }

        services.AddHttpContextAccessor();
        services.AddScoped<ITokenProvider, TokenProvider>();

        IConfigurationSection emailOptions = configuration.GetSection("EmailOptions");
        string? smtpConn = configuration.GetConnectionString("papercut");

        // Only configure FluentEmail if SMTP connection is available (not during design-time/migrations)
        if (emailOptions["SmtpServer"] != null && emailOptions["SmtpPort"] != null)
        {
            string host = emailOptions["SmtpServer"];
            int port = int.Parse(emailOptions["SmtpPort"]!, CultureInfo.CurrentCulture);
            services.AddFluentEmail(emailOptions["FromAddress"], emailOptions["FromAddress"])
               .AddSmtpSender(host, port);
            services.AddScoped<IEmailService, EmailService>();
        }
        else if (!string.IsNullOrEmpty(smtpConn))
        {
            string uriString = smtpConn.Replace("Endpoint=", "");
            var uri = new Uri(uriString);
            string host = uri.Host;
            int port = uri.Port;
            services.AddFluentEmail(emailOptions["FromAddress"], emailOptions["FromAddress"])
               .AddSmtpSender(host, port);
            services.AddScoped<IEmailService, EmailService>();
        }
        else
        {
            // Use no-op email service during design-time (EF migrations)
            services.AddScoped<IEmailService, NoOpEmailService>();
        }

        return services;
    }

    private static IServiceCollection AddAuthorizationInternal(this IServiceCollection services)
    {
        services.AddAuthorization();

        return services;
    }

    private static IServiceCollection AddCaching(this IServiceCollection services, IConfiguration configuration)
    {
        string? redisConnection = configuration.GetConnectionString("redis");

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
            options.InstanceName = "SharpBuy:";
        });

        // Register IConnectionMultiplexer for cache invalidation
        services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(sp => StackExchange.Redis.ConnectionMultiplexer.Connect(redisConnection!));

        services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<ICacheInvalidator, CacheInvalidator>();

        return services;
    }

    private static IServiceCollection AddBackgroundJobs(this IServiceCollection services)
    {
        services.AddSingleton<IBackgroundJobService, BackgroundJobService>();
        services.AddHostedService<BackgroundJobWorker>();

        return services;
    }
}
