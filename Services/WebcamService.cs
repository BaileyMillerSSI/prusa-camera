using FlashCap;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using prusa_camera.Configuration;
using SixLabors.ImageSharp;

namespace PrusaCamera.Services
{
    public interface ICameraService
    {
        Task<Stream?> CaptureFrame();
    }

    public class WebcamService : ICameraService
    {
        private readonly CaptureDevices _devices;
        private readonly IOptionsMonitor<CameraSettings> _cameraSettingsMonitor;
        private readonly ILogger<WebcamService> _logger;

        public WebcamService(
            IOptionsMonitor<CameraSettings> cameraSettingsMonitor,
            ILogger<WebcamService> logger)
        {
            _devices = new CaptureDevices();
            _cameraSettingsMonitor = cameraSettingsMonitor;
            _logger = logger;
            foreach (var device in _devices.EnumerateDescriptors())
            {
                _logger.LogInformation("Device available: {Device}", device);
            }
        }

        public async Task<Stream?> CaptureFrame()
        {
            var captureDevice = GetCaptureDevice();

            if (captureDevice != null)
            {
                var firstResolution = captureDevice.Characteristics.First();

                var singleFrame = await captureDevice.TakeOneShotAsync(firstResolution);

                using (var image = Image.Load(singleFrame))
                {
                    var outputStream = new MemoryStream();

                    await image.SaveAsJpegAsync(outputStream);
                    outputStream.Position = 0;

                    return outputStream;
                }
            }

            return null;
        }

        private CaptureDeviceDescriptor? GetCaptureDevice() =>
            _devices.EnumerateDescriptors().FirstOrDefault(device => device.Name == _cameraSettingsMonitor.CurrentValue.DeviceName);
    }
}