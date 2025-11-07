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
                    
                    // La API ahora devuelve códigos numéricos: 0, 1, 2
                    string cleanResponse = jsonContent.Trim().Trim('"');
                    
                    if (int.TryParse(cleanResponse, out int statusCode))
                    {
                        return statusCode switch
                        {
                            0 => new QRValidationResult
                            {
                                IsValid = false,
                                StatusCode = 0,
                                Message = "❌ Código no autorizado",
                                VisitanteName = "No autorizado",
                                VisitanteEmail = "",
                                FechaVisita = null
                            },
                            1 => new QRValidationResult
                            {
                                IsValid = true,
                                StatusCode = 1,
                                Message = "✅ Código autorizado",
                                VisitanteName = "Visitante Autorizado",
                                VisitanteEmail = "visitante@ejemplo.com",
                                FechaVisita = DateTime.Now
                            },
                            2 => new QRValidationResult
                            {
                                IsValid = false,
                                StatusCode = 2,
                                Message = "⏰ Código expirado (acceso ya utilizado)",
                                VisitanteName = "Acceso expirado",
                                VisitanteEmail = "",
                                FechaVisita = null
                            },
                            _ => new QRValidationResult
                            {
                                IsValid = false,
                                StatusCode = -1,
                                Message = $"❓ Código de respuesta desconocido: {statusCode}",
                                VisitanteName = "Error desconocido",
                                VisitanteEmail = "",
                                FechaVisita = null
                            }
                        };
                    }
                    else
                    {
                        return new QRValidationResult
                        {
                            IsValid = false,
                            StatusCode = -1,
                            Message = "❌ Respuesta del servidor no válida",
                            VisitanteName = "Error de formato",
                            VisitanteEmail = "",
                            FechaVisita = null
                        };
                    }
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
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string VisitanteName { get; set; } = string.Empty;
        public string VisitanteEmail { get; set; } = string.Empty;
        public DateTime? FechaVisita { get; set; }
    }
}
