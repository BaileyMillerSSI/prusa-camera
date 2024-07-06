using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PrusaCamera.Configuration;

namespace PrusaCamera.Services
{
    public class TimedConnectUploadBackgroundService : BackgroundService
    {
        private readonly IPrusaConnectService _prusaConnectService;
        private readonly ICameraService _cameraService;
        private readonly IOptionsMonitor<PrusaConnectSettings> _prusaConnectSettingsMonitor;
        private readonly ILogger<TimedConnectUploadBackgroundService> _logger;

        public TimedConnectUploadBackgroundService(
            IPrusaConnectService prusaConnectService,
            ICameraService cameraService,
            IOptionsMonitor<PrusaConnectSettings> prusaConnectSettings,
            ILogger<TimedConnectUploadBackgroundService> logger)
        {
            _prusaConnectService = prusaConnectService;
            _cameraService = cameraService;
            _prusaConnectSettingsMonitor = prusaConnectSettings;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger
                .LogInformation("Starting to send timed uploads to PrusaConnect. Interval: {Time} seconds.", _prusaConnectSettingsMonitor.CurrentValue.UploadIntervalInSeconds);

            await UploadScreenshot();

            using (var timer = new PeriodicTimer(TimeSpan.FromSeconds(_prusaConnectSettingsMonitor.CurrentValue.UploadIntervalInSeconds)))
            {
                using (_prusaConnectSettingsMonitor.OnChange((settings) =>
                {
                    _logger.LogInformation("Changing interval to {Time} seconds", settings.UploadIntervalInSeconds);
                    timer.Period = TimeSpan.FromSeconds(settings.UploadIntervalInSeconds);
                }))
                {
                    while (await timer.WaitForNextTickAsync(stoppingToken))
                    {
                        await UploadScreenshot();
                    }
                }
            }
        }

        private async Task UploadScreenshot()
        {
            using (var imageStream = await _cameraService.CaptureFrame())
            {
                if (imageStream != null)
                {
                    _logger.LogInformation("Starting to send image to PrusaConnect");
                    await _prusaConnectService.UploadScreenshot(imageStream);
                    _logger.LogInformation("Finished sending image to PrusaConnect");
                }
                else
                {
                    _logger.LogDebug("Captured image was empty/null and no upload was sent");
                }
            }
        }
    }
}