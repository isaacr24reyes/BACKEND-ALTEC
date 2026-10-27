using System.Reflection;
using AltecSystem.Application.Handlers.Products;
using AltecSystem.Application.Interfaces;
using AltecSystem.Domain.Persistence;
using AltecSystem.Infrastructure.Services;
using AltecSystem.Infrastructure.Repositories;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using AltecSystem.Application.Queries.Sales;
using AltecSystem.Application.Services.Sri;
using AltecSystem.Infrastructure.Sri;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<CloudinaryService>();
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    cfg.RegisterServicesFromAssembly(typeof(CreateProductHandler).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(GetSalesGroupedByInvoiceNumberQuery).Assembly);
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",
                "http://192.168.100.10:4200",
                "https://isaacr24reyes.github.io",
                "https://altecmec.com"
            )
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// ✅ Configuración de la base de datos
builder.Services.AddDbContext<AltecSystemDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// ✅ Registrar interfaces y servicios
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ISalesRepository, SalesRepository>();
builder.Services.AddTransient<IQuotationRepository, QuotationRepository>();
builder.Services.AddScoped<IImportedQuotationRepository, ImportedQuotationRepository>();
builder.Services.AddSingleton<IQuotationExcelReader, QuotationExcelReader>();

// Facturación electrónica SRI (configuración en la sección "Sri")
builder.Services.AddSingleton<SriConfiguration>();
builder.Services.AddSingleton<ISriConfiguration>(sp => sp.GetRequiredService<SriConfiguration>());
builder.Services.AddSingleton<ISriInvoiceXmlBuilder, SriInvoiceXmlBuilder>();
builder.Services.AddSingleton<ISriXmlSigner, SriXadesSigner>();
builder.Services.AddHttpClient<ISriWebServiceClient, SriWebServiceClient>();
builder.Services.AddScoped<IElectronicInvoiceRepository, ElectronicInvoiceRepository>();
builder.Services.AddScoped<ElectronicInvoiceProcessor>();
builder.Services.AddSingleton<IInvoicePdfGenerator, RideInvoicePdfGenerator>();
builder.Services.AddSingleton<IInvoiceEmailSender, SmtpInvoiceEmailSender>();
builder.Services.AddHostedService<AltecSystem.Infrastructure.BackgroundJobs.PendingInvoiceRetryService>();

// ✅ Configuración de controladores y Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ALTEC-SYSTEM API", Version = "v1" });
});
// ⬇️ Agregado para Render
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");
var app = builder.Build();

app.UseCors("AllowAngularApp");


    app.UseSwagger();
    app.UseSwaggerUI(c =>
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ALTEC-SYSTEM API v1")
    );
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
