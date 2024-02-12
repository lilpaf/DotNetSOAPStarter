using Serilog;
using Serilog.Core;
using Serilog.Configuration;

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
   

    // Add services to the container.

    //builder.Services.AddControllers(options =>
    //{
    //    options.ModelBinderProviders.Insert(0, new QueryStringNullOrEmptyModelBinderProvider());
    //})
    //.AddXmlSerializerFormatters(); 
    
    builder.Services.AddControllers()
    .AddXmlSerializerFormatters();
    //builder.Services.AddEndpointsApiExplorer();
    //builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        //app.UseSwagger();
        //app.UseSwaggerUI();
    }

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


