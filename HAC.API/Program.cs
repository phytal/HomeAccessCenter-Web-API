using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace HAC.API {
    public static class Program {
        public static void Main(string[] args) {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseSentry("https://6c329f07393240b1bd91712ebbccf71e@o337002.ingest.sentry.io/5339698");
                    webBuilder.UseStartup<Startup>();
                });
        }
    }
}