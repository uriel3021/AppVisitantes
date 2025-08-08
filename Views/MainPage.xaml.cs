using KYCApp.Services;
using KYCApp.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace KYCApp.Views
{
    public partial class MainPage : ContentPage
    {
        private VisitanteService? _visitanteService;
        private KYCDbContext? _dbContext;

        public MainPage()
        {
            InitializeComponent();
        }
        
        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            try
            {
                // Obtener servicios cuando la página aparece
                _visitanteService = Handler?.MauiContext?.Services?.GetService<VisitanteService>();
                _dbContext = Handler?.MauiContext?.Services?.GetService<KYCDbContext>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al obtener servicios: {ex.Message}");
            }
        }

        private async void OnScanQRClicked(object sender, EventArgs e)
        {
            var qrPage = new QRScanPage();
            await Navigation.PushAsync(qrPage);
        }

        private async void OnTestDBClicked(object sender, EventArgs e)
        {
            try
            {
                KYCDbContext? context = _dbContext;
                
                // Si no se pudo obtener por DI, crear uno manualmente
                if (context == null)
                {
                    // Obtener la configuración desde el ServiceProvider
                    var serviceProvider = Handler?.MauiContext?.Services;
                    if (serviceProvider != null)
                    {
                        context = serviceProvider.GetService<KYCDbContext>();
                    }
                    
                    // Si aún no funciona, crear directamente desde appsettings.json
                    if (context == null)
                    {
                        try
                        {
                            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                            using var stream = assembly.GetManifestResourceStream("KYCApp.appsettings.json");
                            
                            if (stream != null)
                            {
                                var config = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                                    .AddJsonStream(stream)
                                    .Build();
                                
                                var connectionString = config.GetConnectionString("DefaultConnection");
                                
                                var optionsBuilder = new DbContextOptionsBuilder<KYCDbContext>();
                                optionsBuilder.UseSqlServer(connectionString);
                                context = new KYCDbContext(optionsBuilder.Options);
                            }
                        }
                        catch (Exception configEx)
                        {
                            await DisplayAlert("Error de Configuración", $"❌ Error leyendo appsettings.json: {configEx.Message}", "OK");
                            return;
                        }
                    }
                }

                if (context == null)
                {
                    await DisplayAlert("Error", "❌ No se pudo obtener el contexto de base de datos", "OK");
                    return;
                }

                // Contar registros usando Entity Framework
                var visitantesCount = context.Visitantes.Count();
                var codigoQRCount = context.CodigoQrs.Count();
                
                string message = $"✅ Conexión exitosa\n\n" +
                               $"📊 Resultados:\n" +
                               $"• Visitantes: {visitantesCount} registros\n" +
                               $"• CodigoQR: {codigoQRCount} registros\n\n" +
                               $"🔗 Cadena de conexión: Configurada desde appsettings.json";

                await DisplayAlert("Test Base de Datos", message, "OK");
                
                // Disponer del contexto si lo creamos manualmente
                if (context != _dbContext)
                {
                    context.Dispose();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"❌ Error: {ex.Message}", "OK");
            }
        }
    }
}
