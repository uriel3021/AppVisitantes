using KYCApp.Services;

namespace KYCApp.Views
{
    public partial class ResumenPage : ContentPage
    {
        private string qrCode;
        private string documentPhotoPath;
        private string placasPhotoPath;
        private QRValidationResult? validationResult;

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
                // Validar que sea un GUID válido
                if (!Guid.TryParse(qrCode, out Guid qrGuid))
                {
                    ShowErrorMessage("El código QR no tiene formato válido de GUID");
                    return;
                }
                
                // Llamar al servicio REST para obtener datos del visitante
                var qrService = new QRValidationService();
                var result = await qrService.ValidateQRCodeAsync(qrCode);
                
                if (result.IsValid)
                {
                    validationResult = result; // Guardar el resultado para usar en la confirmación
                    VisitanteNombreLabel.Text = $"Nombre: {result.VisitanteName}";
                    VisitanteEmailLabel.Text = $"Email: {result.VisitanteEmail}";
                    VisitanteTelefonoLabel.Text = $"Teléfono: (No disponible desde API)";
                    VisitanteFechaLabel.Text = $"Fecha de Visita: {result.FechaVisita:dd/MM/yyyy}";
                }
                else
                {
                    ShowErrorMessage($"No se encontraron datos del visitante: {result.Message}");
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
            if (validationResult == null || !validationResult.IsValid)
            {
                await DisplayAlert("Error", "No se pudieron cargar los datos del visitante", "OK");
                return;
            }

            // Mostrar indicador de progreso
            var loadingAlert = DisplayAlert("📤 Subiendo imágenes", "Por favor espere mientras se suben las fotos...", "OK");

            try
            {
                var uploadService = new ImageUploadService();
                
                string documentUrl = "";
                string placasUrl = "";

                // Subir foto del documento
                if (!string.IsNullOrEmpty(documentPhotoPath) && File.Exists(documentPhotoPath))
                {
                    System.Diagnostics.Debug.WriteLine($"[RESUMEN] Subiendo documento: {documentPhotoPath}");
                    var documentResult = await uploadService.UploadImageAsync(documentPhotoPath, $"documento_{qrCode}_{DateTime.Now:yyyyMMdd_HHmmss}.jpg");
                    
                    if (documentResult.IsSuccess)
                    {
                        documentUrl = documentResult.Url;
                        System.Diagnostics.Debug.WriteLine($"[RESUMEN] Documento subido: {documentUrl}");
                    }
                    else
                    {
                        await DisplayAlert("❌ Error", $"Error subiendo foto del documento: {documentResult.Message}", "OK");
                        return;
                    }
                }

                // Subir foto de placas
                if (!string.IsNullOrEmpty(placasPhotoPath) && File.Exists(placasPhotoPath))
                {
                    System.Diagnostics.Debug.WriteLine($"[RESUMEN] Subiendo placas: {placasPhotoPath}");
                    var placasResult = await uploadService.UploadImageAsync(placasPhotoPath, $"placas_{qrCode}_{DateTime.Now:yyyyMMdd_HHmmss}.jpg");
                    
                    if (placasResult.IsSuccess)
                    {
                        placasUrl = placasResult.Url;
                        System.Diagnostics.Debug.WriteLine($"[RESUMEN] Placas subidas: {placasUrl}");
                    }
                    else
                    {
                        await DisplayAlert("❌ Error", $"Error subiendo foto de placas: {placasResult.Message}", "OK");
                        return;
                    }
                }

                // Cancelar el alert de carga
                // loadingAlert is not awaitable here, so we'll show success instead
                
                await DisplayAlert("✅ Visita Confirmada", 
                    $"La visita de {validationResult.VisitanteName} ha sido registrada exitosamente.\n\n" +
                    $"Código QR: {qrCode}\n" +
                    $"Documento: {(string.IsNullOrEmpty(documentUrl) ? "No subido" : "✅ Subido")}\n" +
                    $"Placas: {(string.IsNullOrEmpty(placasUrl) ? "No subido" : "✅ Subido")}\n" +
                    $"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}", 
                    "OK");

                System.Diagnostics.Debug.WriteLine($"[RESUMEN] Visita registrada - Documento: {documentUrl}, Placas: {placasUrl}");

                // Return to main page
                await Navigation.PopToRootAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RESUMEN] Error en confirmación: {ex.Message}");
                await DisplayAlert("❌ Error", $"Error durante el proceso de confirmación: {ex.Message}", "OK");
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
