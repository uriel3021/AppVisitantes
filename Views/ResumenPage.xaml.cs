using KYCApp.Data;
using KYCApp.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace KYCApp.Views
{
    public partial class ResumenPage : ContentPage
    {
        private string qrCode;
        private string documentPhotoPath;
        private string placasPhotoPath;
        private Visitante? visitanteData;

        public ResumenPage(string qrCode, string documentPath, string placasPath)
        {
            InitializeComponent();
            this.qrCode = qrCode;
            this.documentPhotoPath = documentPath;
            this.placasPhotoPath = placasPath;
            
            LoadData();
        }

        private async void LoadData()
        {
            await LoadVisitanteData();
            LoadPhotos();
            LoadQRInfo();
        }

        private async Task LoadVisitanteData()
        {
            try
            {
                // Crear contexto directamente desde appsettings.json
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                using var stream = assembly.GetManifestResourceStream("KYCApp.appsettings.json");
                
                if (stream == null)
                {
                    ShowErrorMessage("No se encontró el archivo de configuración");
                    return;
                }
                
                var config = new ConfigurationBuilder()
                    .AddJsonStream(stream)
                    .Build();
                
                var connectionString = config.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    ShowErrorMessage("No se encontró la cadena de conexión");
                    return;
                }
                
                using var context = new KYCDbContext(new DbContextOptionsBuilder<KYCDbContext>()
                    .UseSqlServer(connectionString)
                    .Options);

                // El qrCode es un GUID (Id), no un código string
                if (Guid.TryParse(qrCode, out Guid qrGuid))
                {
                    // Buscar directamente por Id (optimizado)
                    var codigoQRRegistro = await context.CodigoQrs
                        .AsNoTracking()
                        .Where(c => c.Id == qrGuid)
                        .FirstOrDefaultAsync();
                    
                    if (codigoQRRegistro != null)
                    {
                        // Buscar visitante (optimizado)
                        visitanteData = await context.Visitantes
                            .AsNoTracking()
                            .Where(v => v.Id == codigoQRRegistro.VisitanteId)
                            .FirstOrDefaultAsync();
                        
                        if (visitanteData != null)
                        {
                            VisitanteNombreLabel.Text = $"Nombre: {visitanteData.Nombre} {visitanteData.ApellidoPaterno} {visitanteData.ApellidoMaterno}";
                            VisitanteEmailLabel.Text = $"Email: {visitanteData.CorreoElectronico}";
                            VisitanteTelefonoLabel.Text = $"Teléfono: {visitanteData.Telefono}";
                            VisitanteFechaLabel.Text = $"Fecha de Visita: {visitanteData.FechaVisita:dd/MM/yyyy}";
                        }
                        else
                        {
                            ShowErrorMessage("No se encontraron datos del visitante");
                        }
                    }
                    else
                    {
                        ShowErrorMessage("No se encontró el código QR en la base de datos");
                    }
                }
                else
                {
                    ShowErrorMessage("El código QR no tiene formato válido de GUID");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error cargando datos: {ex.Message}");
            }
        }

        private void LoadPhotos()
        {
            try
            {
                if (!string.IsNullOrEmpty(documentPhotoPath) && File.Exists(documentPhotoPath))
                {
                    DocumentImage.Source = ImageSource.FromFile(documentPhotoPath);
                }

                if (!string.IsNullOrEmpty(placasPhotoPath) && File.Exists(placasPhotoPath))
                {
                    PlacasImage.Source = ImageSource.FromFile(placasPhotoPath);
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error cargando fotos: {ex.Message}");
            }
        }

        private void LoadQRInfo()
        {
            QRCodeLabel.Text = $"Código: {qrCode}";
        }

        private void ShowErrorMessage(string message)
        {
            VisitanteNombreLabel.Text = $"Error: {message}";
            VisitanteEmailLabel.Text = "";
            VisitanteTelefonoLabel.Text = "";
            VisitanteFechaLabel.Text = "";
        }

        private async void OnConfirmClicked(object sender, EventArgs e)
        {
            if (visitanteData == null)
            {
                await DisplayAlert("Error", "No se pudieron cargar los datos del visitante", "OK");
                return;
            }

            await DisplayAlert("✅ Visita Confirmada", 
                $"La visita de {visitanteData.Nombre} {visitanteData.ApellidoPaterno} ha sido registrada exitosamente.\n\n" +
                $"Código QR: {qrCode}\n" +
                $"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}", 
                "OK");

            // Return to main page
            await Navigation.PopToRootAsync();
        }

        private async void OnEditClicked(object sender, EventArgs e)
        {
            var action = await DisplayActionSheet("¿Qué desea editar?", "Cancelar", null, 
                "Tomar nueva foto de documento", "Tomar nueva foto de placas", "Volver al escáner QR");

            switch (action)
            {
                case "Tomar nueva foto de documento":
                    await Navigation.PopAsync(); // Go back to placas
                    await Navigation.PopAsync(); // Go back to document
                    break;
                case "Tomar nueva foto de placas":
                    await Navigation.PopAsync(); // Go back to placas
                    break;
                case "Volver al escáner QR":
                    await Navigation.PopToRootAsync();
                    break;
            }
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            var confirm = await DisplayAlert("Cancelar", 
                "¿Está seguro de que desea cancelar el proceso de verificación?", 
                "Sí, cancelar", "No");

            if (confirm)
            {
                // Clean up photo files
                CleanupPhotoFiles();
                await Navigation.PopToRootAsync();
            }
        }

        private void CleanupPhotoFiles()
        {
            try
            {
                if (!string.IsNullOrEmpty(documentPhotoPath) && File.Exists(documentPhotoPath))
                {
                    File.Delete(documentPhotoPath);
                }

                if (!string.IsNullOrEmpty(placasPhotoPath) && File.Exists(placasPhotoPath))
                {
                    File.Delete(placasPhotoPath);
                }
            }
            catch { /* Ignore cleanup errors */ }
        }
    }
}
