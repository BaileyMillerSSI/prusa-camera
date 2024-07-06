using SharpAppSettings;

namespace PrusaCamera.Configuration
{
    [AppSetting("PrusaConnect")]
    public class PrusaConnectSettings
    {
        public string Url { get; init; }

        public string CameraToken { get; init; }

        public short UploadIntervalInSeconds { get; init; }
    }
}