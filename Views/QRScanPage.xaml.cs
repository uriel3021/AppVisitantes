using ZXing.Net.Maui;
using KYCApp.Services;

namespace KYCApp.Views
{
    public partial class QRScanPage : ContentPage
    {
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
        } public QRScanPage()
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
                            
                            // Mostrar SIEMPRE el valor escaneado primero
                            await DisplayAlert("🔍 QR Detectado", 
                                $"Código escaneado: {qrCode}\n" +
                                $"Longitud: {qrCode.Length} caracteres\n" +
                                $"Hora: {DateTime.Now:HH:mm:ss}\n\n" +
                                $"Presiona OK para validar", 
                                "OK");
                            
                            System.Diagnostics.Debug.WriteLine("[QR] Alert mostrado, iniciando validación...");
                            
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
                    await DisplayAlert("❌ Formato QR", "El QR no tiene formato de GUID válido", "OK");
                    CameraView.IsDetecting = true;
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
                    await DisplayAlert("✅ QR VÁLIDO", 
                        $"Código registrado en el sistema\n\n" +
                        $"� QR: {qrCode}\n" +
                        $"👤 Visitante: {result.VisitanteName}\n" +
                        $"📧 Email: {result.VisitanteEmail}\n" +
                        $"� Fecha: {result.FechaVisita:dd/MM/yyyy}\n\n" +
                        $"¡Continuando con documentos!", 
                        "Continuar");
                    
                    System.Diagnostics.Debug.WriteLine("[QR] ✅ Navegando a DocumentCapturePage...");
                    await Navigation.PushAsync(new DocumentCapturePage(qrCode));
                    System.Diagnostics.Debug.WriteLine("[QR] ✅ Navegación completada");
                }
                else
                {
                    await DisplayAlert("❌ QR NO VÁLIDO", 
                        $"Código no registrado\n\n" +
                        $"🔍 Escaneado: {qrCode}\n" +
                        $"📝 Mensaje: {result.Message}", 
                        "Reintentar");
                    
                    System.Diagnostics.Debug.WriteLine("[QR] ❌ QR no válido, reactivando escáner");
                    CameraView.IsDetecting = true;
                }
                
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[QR] ❌ EXCEPCIÓN: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[QR] StackTrace: {ex.StackTrace}");
                
                await DisplayAlert("❌ ERROR", 
                    $"Error validando QR:\n{ex.Message}\n\n" +
                    $"Tipo: {ex.GetType().Name}", 
                    "OK");
                
                CameraView.IsDetecting = true;
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
    }
}
