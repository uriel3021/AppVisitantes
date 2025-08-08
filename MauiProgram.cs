using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using ZXing.Net.Maui.Controls;
using KYCApp.Views;
using KYCApp.Data;
using KYCApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace KYCApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            // COMENTADO TEMPORALMENTE PARA DIAGNOSTICAR
            // .UseMauiCommunityToolkit()
            // .UseBarcodeReader()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // COMENTADO TEMPORALMENTE
        /*
        builder.Services.AddLogging(logging =>
        {
            logging.AddDebug();
        });
        */

        // Configurar appsettings.json correctamente para MAUI
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("KYCApp.appsettings.json");
        
        if (stream == null)
        {
            throw new InvalidOperationException("No se pudo encontrar el archivo appsettings.json embebido.");
        }

        var config = new ConfigurationBuilder()
            .AddJsonStream(stream)
            .Build();
        
        builder.Configuration.AddConfiguration(config);
        
        // Configurar Entity Framework usando ÚNICAMENTE appsettings.json
        builder.Services.AddDbContext<KYCDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

        // Registrar servicios
        builder.Services.AddScoped<VisitanteService>();

        return builder.Build();
    }
}
