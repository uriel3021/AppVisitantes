using ZXing.Net.Maui;
using KYCApp.Services;

namespace KYCApp.Views
{
    public partial class QRScanPage : ContentPage
    {
        private string _currentQRCode = string.Empty;
           private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private void OnToggleFlashClicked(object sender, EventArgs e)
        {
            try
            {
                CameraView.IsTorchOn = !CameraView.IsTorchOn;
                var button = sender as Button;
                if (button != null)
                {
                    button.Text = CameraView.IsTorchOn ? "🔆 Flash ON" : "💡 Flash";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Flash] Error al cambiar flash: {ex.Message}");
            }
        }
        
        public QRScanPage()
        {
            InitializeComponent();
        }

        private async void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[QR] *** INICIO OnBarcodesDetected ***");
                
                var first = e.Results?.FirstOrDefault();
                if (first is not null)
                {
                    var qrCode = first.Value;
                    System.Diagnostics.Debug.WriteLine($"[QR] QR detectado: {qrCode}");
                    
                    // Stop detection
                    CameraView.IsDetecting = false;
                    System.Diagnostics.Debug.WriteLine("[QR] Escáner detenido");

                    // Mostrar alert en UI thread
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        try
                        {
                            System.Diagnostics.Debug.WriteLine("[QR] Mostrando alert inicial...");
                            
                            // Mostrar overlay de validación personalizado
                            ShowValidationOverlay("Validando QR", "Verificando código en el sistema...", "⏳");
                            
                            System.Diagnostics.Debug.WriteLine("[QR] Overlay mostrado, iniciando validación...");
                            
                            // Validar con la API REST
                            await ValidateQRWithAPI(qrCode);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[QR] Error en MainThread: {ex.Message}");
                            await DisplayAlert("Error UI", $"Error en interfaz: {ex.Message}", "OK");
                            CameraView.IsDetecting = true;
                        }
                    });
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[QR] No se detectó ningún código QR válido");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[QR] Error general en OnBarcodesDetected: {ex.Message}");
                CameraView.IsDetecting = true;
            }
        }

        private async Task ValidateQRWithAPI(string qrCode)
        {
            try
            {
                //System.Diagnostics.Debug.WriteLine($"[QR] *** INICIO ValidateQRWithAPI para: {qrCode} ***");
                
                //await DisplayAlert("🔄 Validando", "Conectando al servicio...", "OK");
                
                // Validar que sea un GUID válido
                if (!Guid.TryParse(qrCode, out Guid qrGuid))
                {
                    // Mostrar error de formato con overlay personalizado
                    ShowValidationResult(false, "El QR no tiene formato de GUID válido", "", "", null, qrCode);
                    return;
                }
                
                System.Diagnostics.Debug.WriteLine($"[QR] GUID válido: {qrGuid}");
                
                // Llamar al servicio REST
                var qrService = new QRValidationService();
                
               // await DisplayAlert("⏳ Ejecutando", "Consultando código...", "OK");
                
                var result = await qrService.ValidateQRCodeAsync(qrCode);
                
               // System.Diagnostics.Debug.WriteLine($"[QR] Resultado API: IsValid={result.IsValid}, Message={result.Message}");
                
                if (result.IsValid)
                {
                    // Mostrar resultado exitoso con overlay personalizado
                    ShowValidationResult(true, result.Message, result.VisitanteName, result.VisitanteEmail, result.FechaVisita, qrCode);
                    System.Diagnostics.Debug.WriteLine("[QR] ✅ Resultado válido mostrado");
                }
                else
                {
                    // Mostrar error con overlay personalizado
                    ShowValidationResult(false, result.Message, "", "", null, qrCode);
                    System.Diagnostics.Debug.WriteLine("[QR] ❌ QR no válido mostrado");
                }
                
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[QR] ❌ EXCEPCIÓN: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[QR] StackTrace: {ex.StackTrace}");
                
                // Mostrar error de conexión con overlay personalizado
                ShowValidationResult(false, $"Error de conexión: {ex.Message}", "", "", null, "");
                System.Diagnostics.Debug.WriteLine("[QR] ❌ Error de conexión mostrado");
            }
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            CameraView.IsDetecting = false;
        }

        // Métodos para overlay personalizado
        private void ShowValidationOverlay(string title, string message, string icon)
        {
            ValidationTitle.Text = title;
            ValidationMessage.Text = message;
            ValidationIcon.Text = icon;
            ValidationButton.IsVisible = false;
            VisitanteInfo.IsVisible = false;
            ValidationOverlay.IsVisible = true;
        }

        private void ShowValidationResult(bool isValid, string message, string nombre, string email, DateTime? fecha, string qrCode)
        {
            _currentQRCode = qrCode; // Guardar QR code para navegación
            
            if (isValid)
            {
                ValidationIcon.Text = "✅";
                ValidationIcon.TextColor = Color.FromArgb("#8B3A3A");
                ValidationTitle.Text = "QR Válido";
                ValidationMessage.Text = message;

                // Mostrar información del visitante
                VisitanteNombre.Text = $"👤 {nombre}";
                VisitanteEmail.Text = $"📧 {email}";
                VisitanteFecha.Text = $"📅 {fecha:dd/MM/yyyy}";
                VisitanteInfo.IsVisible = true;

                ValidationButton.Text = "Continuar";
                ValidationButton.BackgroundColor = Color.FromArgb("#8B3A3A");
                ValidationButton.IsVisible = true;

                // Navegar automáticamente después de 2 segundos
                Device.StartTimer(TimeSpan.FromSeconds(2), () =>
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        ValidationOverlay.IsVisible = false;
                        await Navigation.PushAsync(new DocumentCapturePage(qrCode));
                    });
                    return false;
                });
            }
            else
            {
                ValidationIcon.Text = "❌";
                ValidationIcon.TextColor = Color.FromArgb("#EF4444");
                ValidationTitle.Text = "QR No Válido";
                ValidationMessage.Text = message;
                VisitanteInfo.IsVisible = false;

                ValidationButton.Text = "Reintentar";
                ValidationButton.BackgroundColor = Color.FromArgb("#EF4444");
                ValidationButton.IsVisible = true;
            }
        }

        private async void OnValidationButtonClicked(object sender, EventArgs e)
        {
            ValidationOverlay.IsVisible = false;
            
            if (ValidationButton.Text == "Reintentar")
            {
                // QR no válido - reactivar escáner
                CameraView.IsDetecting = true;
            }
            else if (ValidationButton.Text == "Continuar")
            {
                // QR válido - navegar a captura de documentos
                System.Diagnostics.Debug.WriteLine("[QR] ✅ Navegando a DocumentCapturePage...");
                await Navigation.PushAsync(new DocumentCapturePage(_currentQRCode));
                System.Diagnostics.Debug.WriteLine("[QR] ✅ Navegación completada");
            }
        }
    }
}
