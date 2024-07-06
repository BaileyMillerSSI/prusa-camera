using FlashCap;
using Microsoft.Extensions.Logging;

namespace PrusaCamera.Services
{
    public interface ICameraService
    {
        Task<Stream> CaptureFrame();
    }

    public class WebcamService : ICameraService
    {
        private readonly CaptureDevices _devices;
        private readonly ILogger<WebcamService> _logger;

        public WebcamService(ILogger<WebcamService> logger)
        {
            _devices = new CaptureDevices();
            _logger = logger;
        }

        public async Task<Stream> CaptureFrame()
        {
            await Task.CompletedTask;

            return File.OpenRead("test.jpeg");
        }
    }
}