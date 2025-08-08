using ZXing.Net.Maui;
using System;
using KYCApp.Data;
using KYCApp.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace KYCApp.Views
{
    public partial class QRScanPage : ContentPage
    {
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
                            
                            // Mostrar SIEMPRE el valor escaneado primero
                            await DisplayAlert("🔍 QR Detectado", 
                                $"Código escaneado: {qrCode}\n" +
                                $"Longitud: {qrCode.Length} caracteres\n" +
                                $"Hora: {DateTime.Now:HH:mm:ss}\n\n" +
                                $"Presiona OK para validar", 
                                "OK");
                            
                            System.Diagnostics.Debug.WriteLine("[QR] Alert mostrado, iniciando validación...");
                            
                            // Validar con la base de datos
                            await ValidateQRWithDatabase(qrCode);
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

        private async Task ValidateQRWithDatabase(string qrCode)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[QR] *** INICIO ValidateQRWithDatabase para: {qrCode} ***");
                
                await DisplayAlert("🔄 Validando", "Conectando a la base de datos...", "OK");
                
                // Crear contexto directamente desde appsettings.json
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                using var stream = assembly.GetManifestResourceStream("KYCApp.appsettings.json");
                
                if (stream == null)
                {
                    System.Diagnostics.Debug.WriteLine("[QR] ❌ ERROR: No se encontró appsettings.json");
                    await DisplayAlert("❌ Error Config", "No se encontró archivo de configuración", "OK");
                    CameraView.IsDetecting = true;
                    return;
                }
                
                System.Diagnostics.Debug.WriteLine("[QR] ✅ Archivo appsettings.json encontrado");
                
                var config = new ConfigurationBuilder()
                    .AddJsonStream(stream)
                    .Build();
                
                var connectionString = config.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    System.Diagnostics.Debug.WriteLine("[QR] ❌ ERROR: Cadena de conexión vacía");
                    await DisplayAlert("❌ Error Config", "Cadena de conexión no encontrada", "OK");
                    CameraView.IsDetecting = true;
                    return;
                }
                
                System.Diagnostics.Debug.WriteLine($"[QR] ✅ Cadena de conexión obtenida: {connectionString.Substring(0, Math.Min(50, connectionString.Length))}...");
                
                using var context = new KYCDbContext(new DbContextOptionsBuilder<KYCDbContext>()
                    .UseSqlServer(connectionString)
                    .Options);
                
                System.Diagnostics.Debug.WriteLine("[QR] ✅ Contexto EF creado");
                
                // Probar conexión
                await DisplayAlert("🔄 Conectando", "Probando conexión a base de datos...", "OK");
                
                await context.Database.OpenConnectionAsync();
                System.Diagnostics.Debug.WriteLine("[QR] ✅ Conexión a BD exitosa");
                
                // Buscar el código QR
                await DisplayAlert("🔍 Buscando", $"Buscando QR: {qrCode}", "OK");
                
                System.Diagnostics.Debug.WriteLine("[QR] ⏳ Ejecutando consulta LINQ...");
                
                var codigoQRRegistro = default(CodigoQr);
                
                try
                {
                    // Buscar directamente el código QR específico (más eficiente)
                    await DisplayAlert("⏳ Ejecutando", "Buscando código específico...", "OK");
                    
                    // Convertir string a Guid para comparar correctamente
                    if (Guid.TryParse(qrCode, out Guid qrGuid))
                    {
                        // Usar FindAsync si Id es PK (más rápido) o consulta optimizada
                        codigoQRRegistro = await context.CodigoQrs.FindAsync(qrGuid);
                        
                        // Si FindAsync no funciona (Id no es PK), usar consulta optimizada
                        if (codigoQRRegistro == null)
                        {
                            codigoQRRegistro = await context.CodigoQrs
                                .AsNoTracking()  // No tracking para lectura
                                .Where(c => c.Id == qrGuid)
                                .FirstOrDefaultAsync();
                        }
                    }
                    else
                    {
                        await DisplayAlert("❌ Formato QR", "El QR no tiene formato de GUID válido", "OK");
                        CameraView.IsDetecting = true;
                        return;
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"[QR] Resultado búsqueda: {(codigoQRRegistro != null ? "✅ ENCONTRADO" : "❌ NO ENCONTRADO")}");
                    
                    await DisplayAlert("🔍 Resultado", 
                        codigoQRRegistro != null ? "✅ QR ENCONTRADO!" : "❌ QR NO ENCONTRADO", 
                        "OK");
                }
                catch (Exception sqlEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[QR] ❌ Error en consulta SQL: {sqlEx.Message}");
                    await DisplayAlert("❌ Error SQL", $"Error en consulta:\n{sqlEx.Message}", "OK");
                    CameraView.IsDetecting = true;
                    return;
                }
                
                if (codigoQRRegistro != null)
                {
                    // QR VÁLIDO - buscar visitante (optimizado)
                    var visitante = await context.Visitantes
                        .AsNoTracking()  // No tracking para lectura
                        .Where(v => v.Id == codigoQRRegistro.VisitanteId)
                        .Select(v => new { v.Nombre, v.ApellidoPaterno })  // Solo las columnas necesarias
                        .FirstOrDefaultAsync();
                    
                    string visitanteInfo = visitante != null ? 
                        $"👤 {visitante.Nombre} {visitante.ApellidoPaterno}" : 
                        "👤 Visitante no encontrado";
                    
                    System.Diagnostics.Debug.WriteLine($"[QR] Visitante: {visitanteInfo}");
                    
                    await DisplayAlert("✅ QR VÁLIDO", 
                        $"Código registrado en el sistema\n\n" +
                        $"🔍 QR: {qrCode}\n" +
                        $"🆔 ID: {codigoQRRegistro.Id}\n" +
                        $"{visitanteInfo}\n\n" +
                        $"¡Continuando con documentos!", 
                        "Continuar");
                    
                    System.Diagnostics.Debug.WriteLine("[QR] ✅ Navegando a DocumentCapturePage...");
                    await Navigation.PushAsync(new DocumentCapturePage(qrCode));
                    System.Diagnostics.Debug.WriteLine("[QR] ✅ Navegación completada");
                }
                else
                {
                    // QR NO VÁLIDO - mostrar ejemplos (consulta optimizada)
                    var ejemplosQR = await context.CodigoQrs
                        .AsNoTracking()  // No tracking
                        .Take(3)
                        .Select(c => c.Codigo)  // Solo la columna necesaria
                        .ToListAsync();
                    
                    string ejemplos = ejemplosQR.Any() ? 
                        $"Ejemplos válidos:\n• {string.Join("\n• ", ejemplosQR)}" :
                        "No hay códigos en la BD";
                    
                    await DisplayAlert("❌ QR NO VÁLIDO", 
                        $"Código no registrado\n\n" +
                        $"🔍 Escaneado: {qrCode}\n\n" +
                        $"{ejemplos}", 
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
