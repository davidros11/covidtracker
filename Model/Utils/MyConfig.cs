using Microsoft.Extensions.Configuration;
namespace CovidTracker
{
    public class MyConfig
    {
        public static IConfigurationRoot Configuration { get; }
        static MyConfig()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            Configuration = builder.Build();
        }
    }
}