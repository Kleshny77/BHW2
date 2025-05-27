using ApiGateway.Services;

namespace ApiGateway;

public class Startup
{
    public IConfiguration Configuration { get; }
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddHttpClient<FileStorageClient>(c =>
        {
            c.BaseAddress = new Uri(Configuration["FileStorageServiceUrl"]);
        });
        services.AddHttpClient<FileAnalysisClient>(c =>
        {
            c.BaseAddress = new Uri(Configuration["FileAnalysisServiceUrl"]);
        });
    }
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
} 