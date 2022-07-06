using Microsoft.Extensions.Logging;
using PuppeteerSharp;

namespace SchedulingNotifier.Core;

public interface IAppointmentService
{
    Task Check();
}

public class AppointmentService : IAppointmentService
{
    private readonly IEmailService _emailService;
    private readonly AppSettings _settings;
    private readonly BrowserSettings _browserSettings;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(IEmailService emailService, AppSettings settings, ILogger<AppointmentService> logger, BrowserSettings browserSettings)
    {
        _emailService = emailService;
        _settings = settings;
        _logger = logger;
        _browserSettings = browserSettings;
    }

    public async Task Check()
    {
        try
        {
            var officesToCheck = new[] { AppointmentOffice.Calle100, AppointmentOffice.Calle53, AppointmentOffice.Centro };
            var tasks = new List<Task>();
            foreach (var office in officesToCheck)
            {
                tasks.Add(CheckOfficeAsync(office));
            }

            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
        }
    }

    private async Task CheckOfficeAsync(AppointmentOffice office)
    {
        await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
            ExecutablePath = _browserSettings.ExecutablePath,
        });
        await using var page = await browser.NewPageAsync();

        await page.GoToAsync($"{_settings.MainUrl}{_settings.AppointmentUrlPath}");

        var jsCommands = new List<string>{
            "document.getElementById('buttonPreSolicitar').click()",
            $"document.getElementById('inputPais:select:entrada').selectedIndex = 1",
            "document.getElementById('inputPais:select:entrada').onchange()",
            $"document.getElementById('inputOficina:select:entrada').selectedIndex = {(int)office}",
            "document.getElementById('inputOficina:select:entrada').onchange()",
            "document.getElementById('inputTramite:select:entrada').selectedIndex = 1",
            "document.getElementById('inputTramite:select:entrada').onchange()",
            "document.getElementById('inputServicio:select:entrada').selectedIndex = 1",
            "document.getElementById('inputServicio:select:entrada').onchange()"
        };

        foreach (var command in jsCommands)
        {
            await page.EvaluateExpressionAsync(command);
            await Task.Delay(1000);
        }

        var currentPageContent = await page.GetContentAsync();

        if (!currentPageContent.Contains("no hay citas disponibles", StringComparison.OrdinalIgnoreCase))
        {
            var emailSubject = "Actualizacion de Estado - Citas de Pasaporte";
            var emailContent = $"La Oficina {office} ya tiene CITAS DISPONIBLES. Puede ingresar a { _settings.MainUrl}{ _settings.AppointmentUrlPath}";
            await _emailService.Send(_settings.SmtpUser, "annatorresaranis@gmail.com", "jealepive@outlook.com", emailSubject, emailContent).ConfigureAwait(false);
            _logger.LogInformation($"Email sent - Found available appointments for Office {office}!");
        }
        else
        {
            _logger.LogInformation($"No available appointments were found for {office}");
        }
    }
}