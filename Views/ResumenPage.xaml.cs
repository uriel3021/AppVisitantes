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
                    NombreLabel.Text = result.VisitanteName;
                    EmpresaLabel.Text = result.VisitanteEmail; // Usando email como empresa
                    AreaLabel.Text = "Área de visita";
                    QRCodeLabel.Text = qrCode;
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
                    DocumentoImagen.Source = ImageSource.FromFile(documentPhotoPath);
                }

                if (!string.IsNullOrEmpty(placasPhotoPath) && File.Exists(placasPhotoPath))
                {
                    PlacaImagen.Source = ImageSource.FromFile(placasPhotoPath);
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
            NombreLabel.Text = $"Error: {message}";
            EmpresaLabel.Text = "";
            AreaLabel.Text = "";
        }

        private async void OnConfirmClicked(object sender, EventArgs e)
        {
            /*
            if (validationResult == null || !validationResult.IsValid)
            {
                await DisplayAlert("Error", "No se pudieron cargar los datos del visitante", "OK");
                return;
            }
            */
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
                        ShowErrorOverlay("Error de Subida", $"Error subiendo foto del documento: {documentResult.Message}");
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
                        ShowErrorOverlay("Error de Subida", $"Error subiendo foto de placas: {placasResult.Message}");
                        return;
                    }
                }

                // Mostrar confirmación con overlay personalizado
                ShowConfirmationOverlay(
                    "Visita Confirmada",
                    "La visita ha sido registrada exitosamente.",
                    $"Código QR: {qrCode}",
                    $"Documento: {(string.IsNullOrEmpty(documentUrl) ? "No subido" : "✅ Subido")}",
                    $"Placas: {(string.IsNullOrEmpty(placasUrl) ? "No subido" : "✅ Subido")}",
                    $"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}"
                );

                System.Diagnostics.Debug.WriteLine($"[RESUMEN] Visita registrada - Documento: {documentUrl}, Placas: {placasUrl}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RESUMEN] Error en confirmación: {ex.Message}");
                ShowErrorOverlay("Error de Confirmación", $"Error durante el proceso de confirmación: {ex.Message}");
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

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        // Métodos para overlay personalizado
        private void ShowConfirmationOverlay(string title, string message, string qrInfo, string docInfo, string placasInfo, string fechaInfo)
        {
            ConfirmationIcon.Text = "✅";
            ConfirmationIcon.TextColor = Color.FromArgb("#10B981");
            ConfirmationTitle.Text = title;
            ConfirmationMessage.Text = message;
            QRInfo.Text = qrInfo;
            DocumentoInfo.Text = docInfo;
            PlacasInfo.Text = placasInfo;
            FechaInfo.Text = fechaInfo;
            
            RegistroInfo.IsVisible = true;
            ConfirmationButton.Text = "Continuar";
            ConfirmationOverlay.IsVisible = true;
        }

        private void ShowErrorOverlay(string title, string message)
        {
            ConfirmationIcon.Text = "❌";
            ConfirmationIcon.TextColor = Color.FromArgb("#EF4444");
            ConfirmationTitle.Text = title;
            ConfirmationMessage.Text = message;
            
            RegistroInfo.IsVisible = false;
            ConfirmationButton.Text = "Reintentar";
            ConfirmationOverlay.IsVisible = true;
        }

        private async void OnConfirmationButtonClicked(object sender, EventArgs e)
        {
            ConfirmationOverlay.IsVisible = false;
            
            if (ConfirmationButton.Text == "Continuar")
            {
                // Éxito - regresar al menú principal
                await Navigation.PopToRootAsync();
            }
            else
            {
                // Error - no hacer nada, solo cerrar overlay
            }
        }
    }
}
