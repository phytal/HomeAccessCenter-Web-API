using System;
using System.Net;
using System.Net.Http;
using HAC.API.Controllers;
using HAC.API.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;

namespace HAC.API {
    public class Startup {
        private static readonly CookieContainer CookieContainer = new CookieContainer();

        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            var timeout = Policy.TimeoutAsync<HttpResponseMessage>(
                TimeSpan.FromSeconds(40));
            var handler = new SocketsHttpHandler {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                CookieContainer = CookieContainer,
                PooledConnectionLifetime = TimeSpan.FromMinutes(9),
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5),
                MaxConnectionsPerServer = 10
            };
            services.AddControllers();
            services.AddMvc();
            services.AddHttpClient<IAttendance, Attendance>().ConfigurePrimaryHttpMessageHandler(() => handler)
                .AddPolicyHandler(request => timeout);
            services.AddHttpClient<ICourses, Courses>().ConfigurePrimaryHttpMessageHandler(() => handler)
                .AddPolicyHandler(request => timeout);
            services.AddHttpClient<IIpr, Ipr>().ConfigurePrimaryHttpMessageHandler(() => handler)
                .AddPolicyHandler(request => timeout);
            services.AddHttpClient<IReportCard, ReportCard>().ConfigurePrimaryHttpMessageHandler(() => handler)
                .AddPolicyHandler(request => timeout);
            services.AddHttpClient<IStudentInfo, StudentInfo>().ConfigurePrimaryHttpMessageHandler(() => handler)
                .AddPolicyHandler(request => timeout);
            services.AddHttpClient<ITranscript, Transcript>().ConfigurePrimaryHttpMessageHandler(() => handler)
                .AddPolicyHandler(request => timeout);
            services.AddHttpClient<ILogin, Login>().ConfigurePrimaryHttpMessageHandler(() => handler)
                .SetHandlerLifetime(TimeSpan.FromMinutes(9)).AddPolicyHandler(request => timeout);
            services.AddScoped<IHac, Hac>();
            services.AddScoped<IControllerUtils, ControllerUtils>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();


            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}