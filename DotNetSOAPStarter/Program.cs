using DotNetSOAPStarter.SOAP.Authentication.Handlers;
using DotNetSOAPStarter.SOAP.MVC_Customisations.Binders;
using DotNetSOAPStarter.SOAP.MVC_Customisations.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Web Host");

    builder.Host.UseSerilog((hostContext, service, configuration) =>
    {
        configuration.ReadFrom.Configuration(builder.Configuration);
    });
   
    builder.Services.AddControllers(options =>
    {
        options.ModelBinderProviders.Insert(0, new SOAPRequestEnvelopeModelBinderProvider());
        options.ModelBinderProviders.Insert(0, new QueryStringNullOrEmptyModelBinderProvider());
        
    })
    .AddXmlSerializerFormatters();

    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        options.InvalidModelStateResponseFactory = ApiBehaviorOptionsExtensions.InvalidModelStateResponseFactory;
    });

    builder.Services.AddAuthentication().AddSOAP_FileDataStore();

    var app = builder.Build();

    //app.UseHttpsRedirection();

    //app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}


