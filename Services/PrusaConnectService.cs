using Microsoft.Extensions.Options;
using PrusaCamera.Configuration;
using System.Net.Http.Json;

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
                
                await _uploadClient
                    .PutAsync("/c/snapshot", new ByteArrayContent(inMemoryImageStream.ToArray()));
            }
        }
    }
}