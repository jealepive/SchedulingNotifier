using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PuppeteerSharp;
using SchedulingNotifier.Core;
using System;
using System.IO;
using System.Net.Http;

[assembly: FunctionsStartup(typeof(SchedulingNotifier.Api.Startup))]

namespace SchedulingNotifier.Api;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddOptions<AppSettings>()
        .Configure<IConfiguration>((settings, configuration) =>
        {
            configuration.GetSection("AppSettings").Bind(settings);
        });

        builder.Services.AddSingleton((services) =>
        {
            return services.GetRequiredService<IOptions<AppSettings>>().Value;
        });

        builder.Services.AddSingleton<IEmailService>((services) =>
        {
            return new EmailService(services.GetRequiredService<AppSettings>());
        });

        builder.Services.AddSingleton(_ =>
        {
            using var browserFetcher = new BrowserFetcher(new BrowserFetcherOptions { Path = Path.GetTempPath() });
            browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision).GetAwaiter().GetResult();
            return new BrowserSettings { ExecutablePath = browserFetcher.GetExecutablePath(BrowserFetcher.DefaultChromiumRevision) };
        });

        builder.Services.AddSingleton<IAppointmentService, AppointmentService>();

        builder.Services.AddSingleton(sp =>
        {
            var settings = sp.GetService<AppSettings>();
            var client = new HttpClient()
            {
                BaseAddress = new Uri($"{settings.MainUrl}"),
            };
            return client;
        });
    }
}