using SharpAppSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prusa_camera.Configuration
{
    [AppSetting("Camera")]
    public class CameraSettings
    {
        public string? DeviceName { get; init; }
    }
}
