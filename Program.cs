using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PrusaCamera.Configuration;
using PrusaCamera.Services;
using Serilog;
using SharpAppSettings;

namespace PrusaCamera
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                var configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
                    .Build();

                var hostBuilder = new HostBuilder();

                hostBuilder.UseSerilog();
                hostBuilder
                .ConfigureAppConfiguration(builder =>
                {
                    builder.Sources.Clear();
                    builder.AddConfiguration(configuration);
                })
                .ConfigureServices(services =>
                {
                    services.AddSerilog();
                    services.AddTypedSettings(configuration, typeof(Program).Assembly);
                    services.AddSingleton<ICameraService, WebcamService>();
                    services.AddHttpClient<IPrusaConnectService, PrusaConnectService>((serviceCollection, handler) =>
                    {
                        var connectSettings = serviceCollection.GetRequiredService<IOptions<PrusaConnectSettings>>().Value;

                        handler.BaseAddress = new Uri(connectSettings.Url);

                        handler
                            .DefaultRequestHeaders
                            .Add("Fingerprint", Environment.MachineName);

                        handler
                            .DefaultRequestHeaders
                            .Add("Token", connectSettings.CameraToken);
                    });

                    services.AddHostedService<TimedConnectUploadBackgroundService>();
                });


                using (var host = hostBuilder.Build())
                {
                    var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
                    var logger = host.Services.GetRequiredService<ILogger<Program>>();

                    lifetime.ApplicationStarted.Register(() =>
                    {
                        logger.LogDebug("Started");
                    });

                    lifetime.ApplicationStopping.Register(() =>
                    {
                        logger.LogDebug("Application stopping");
                    });

                    lifetime.ApplicationStopped.Register(() =>
                    {
                        logger.LogDebug("Application stopped");
                    });

                    await host.StartAsync();

                    // Listens for Ctrl+C.
                    await host.WaitForShutdownAsync();
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application failled to start");
            }
            finally
            {

                await Log.CloseAndFlushAsync();
            }
        }
    }
}