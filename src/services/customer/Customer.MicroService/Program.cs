using Customer.MicroService.Data;
using Customer.MicroService.Repositories;
using Customer.MicroService.Services;
using Customer.MicroService.Services.Async;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

var builder = WebApplication.CreateBuilder(args);
var isDevelopment = builder.Environment.IsDevelopment();

ConfigureLogging(builder);

var loggerFactory = LoggerFactory.Create(builder => builder.AddSerilog());

var logger = loggerFactory.CreateLogger<Program>();
logger.LogInformation($"Environment: {builder.Environment.EnvironmentName}");

ConfigureServices(builder);

ConfigureDatabase(builder);

var app = builder.Build();

ConfigureApp(app);

app.Run();

void ConfigureLogging(WebApplicationBuilder builder)
{
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .WriteTo.Console(theme: AnsiConsoleTheme.Literate)
        .WriteTo.File("logs/customer-microservice.log", rollingInterval: RollingInterval.Day)
        .WriteTo.Seq(builder.Configuration.GetSection("Logging:Seq:ServerUrl").Value)
        .CreateLogger();

    builder.Host.UseSerilog();

    // Log Order Service URL
    var orderServiceUrl = builder.Configuration.GetSection("OrderServiceUrl").Value;
    Log.Information($"Order Service URL: {orderServiceUrl}");

    // Log Rabbit MQ URL
    var rabbitMqUrl = builder.Configuration.GetSection("RABBITMQ_HOST").Value + ":" + builder.Configuration.GetSection("RABBITMQ_PORT").Value;
    Log.Information($"RabbitMQ URL: {rabbitMqUrl}");
}

void ConfigureDatabase(WebApplicationBuilder builder)
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnectionString");

    if (isDevelopment)
    {
        logger.LogInformation($"Using development database");
        builder.Services.AddDbContext<CustomersDbContext>(options =>
        options.UseSqlServer(connectionString));

    }
    else
    {
        logger.LogInformation($"Using production SQL Server database");

        var dbServer = Environment.GetEnvironmentVariable("DB_HOST");
        if (!string.IsNullOrEmpty(dbServer))
        {
            connectionString = connectionString.Replace("${DB_HOST}", dbServer);
        }

        var dbName = Environment.GetEnvironmentVariable("DB_NAME");
        if (!string.IsNullOrEmpty(dbName))
        {
            connectionString = connectionString.Replace("${DB_NAME}", dbName);
        }

        var dbPassword = Environment.GetEnvironmentVariable("SA_PASSWORD");
        if (!string.IsNullOrEmpty(dbPassword))
        {
            connectionString = connectionString.Replace("${SA_PASSWORD}", dbPassword);
        }

        logger.LogInformation($"Database Connection string: {connectionString}");

        builder.Services.AddDbContext<CustomersDbContext>(options =>
        {
            options.UseSqlServer(connectionString); 
        });
    }
}

void ConfigureServices(WebApplicationBuilder builder)
{
    builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
    builder.Services.AddScoped<ICustomerService, CustomerService>();
    builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
    builder.Services.AddHttpClient<IOrderService, OrderService>();
    builder.Services.AddSingleton<IMessageBusClient, MessageBusClient>();

    builder.Services.AddControllers(options =>
    {
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
        options.ReturnHttpNotAcceptable = true; // Return appropriate HTTP status code if client requested format is not supported by the service
    }).AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    })
    .AddXmlDataContractSerializerFormatters(); // Add XML support

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddHealthChecks();
}

void ConfigureApp(WebApplication app)
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();
    app.UseHealthChecks("/api/health");
}
