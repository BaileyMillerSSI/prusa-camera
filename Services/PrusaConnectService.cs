using Microsoft.Extensions.Options;
using PrusaCamera.Configuration;

namespace PrusaCamera.Services
{
    public interface IPrusaConnectService
    {
        Task UploadScreenshot(Stream image);
    }

    public class PrusaConnectService : IPrusaConnectService
    {
        private readonly HttpClient _uploadClient;

        public PrusaConnectService(
            HttpClient uploadClient)
        {
            _uploadClient = uploadClient;
        }

        public async Task UploadScreenshot(Stream image)
        {
            // Upload photo to connect service
            image.Position = 0;

            using(var inMemoryImageStream = new MemoryStream())
            {
                await image
                    .CopyToAsync(inMemoryImageStream);

                inMemoryImageStream.Position = 0;
                
                var response = await _uploadClient
                    .PutAsync("/c/snapshot", new ByteArrayContent(inMemoryImageStream.ToArray()));
            }
        }
    }
}