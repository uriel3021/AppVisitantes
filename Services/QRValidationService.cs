using System.Text.Json;

namespace KYCApp.Services
{
    public class QRValidationService
    {
        private readonly HttpClient _httpClient;
        private const string BASE_URL = "https://alfaresidentqr-efbadfazcqaybtcy.canadacentral-01.azurewebsites.net";

        public QRValidationService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(10); // Timeout de 10 segundos
        }

        public async Task<QRValidationResult> ValidateQRCodeAsync(string codigoGuid)
        {
            try
            {
                var url = $"{BASE_URL}/api/CodigoQR/ValidarCodigo?codigo={codigoGuid}";
                
                System.Diagnostics.Debug.WriteLine($"[API] Llamando a: {url}");
                
                var response = await _httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[API] Respuesta RAW: {jsonContent}");
                    
                    // La API solo devuelve "true" o "false" como string
                    string cleanResponse = jsonContent.Trim().Trim('"').ToLower();
                    bool isValid = cleanResponse == "true";
                    
                    return new QRValidationResult 
                    { 
                        IsValid = isValid, 
                        Message = isValid ? "✅ Código QR válido" : "❌ Código QR no válido o no registrado",
                        VisitanteName = isValid ? "Visitante Autorizado" : "No encontrado",
                        VisitanteEmail = isValid ? "visitante@ejemplo.com" : "",
                        FechaVisita = isValid ? DateTime.Now : null
                    };
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[API] Error HTTP: {response.StatusCode}");
                    return new QRValidationResult 
                    { 
                        IsValid = false, 
                        Message = $"Error del servidor: {response.StatusCode}" 
                    };
                }
            }
            catch (TaskCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("[API] Timeout en la llamada");
                return new QRValidationResult 
                { 
                    IsValid = false, 
                    Message = "Timeout: El servidor tardó demasiado en responder" 
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API] Excepción: {ex.Message}");
                return new QRValidationResult 
                { 
                    IsValid = false, 
                    Message = $"Error de conexión: {ex.Message}" 
                };
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    public class QRValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public string VisitanteName { get; set; } = string.Empty;
        public string VisitanteEmail { get; set; } = string.Empty;
        public DateTime? FechaVisita { get; set; }
    }
}
