using System.Text;

namespace KYCApp.Services
{
    public class ImageUploadService
    {
        private readonly HttpClient _httpClient;
        private const string BASE_URL = "https://alfaresidentqr-efbadfazcqaybtcy.canadacentral-01.azurewebsites.net";

        public ImageUploadService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30); // Timeout más largo para subir imágenes
        }

        public async Task<ImageUploadResult> UploadImageAsync(string imagePath, string fileName, string visitanteId, int tipo)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[UPLOAD] Iniciando subida...");
                System.Diagnostics.Debug.WriteLine($"[UPLOAD] ImagePath: '{imagePath}'");
                System.Diagnostics.Debug.WriteLine($"[UPLOAD] FileName: '{fileName}'");
                System.Diagnostics.Debug.WriteLine($"[UPLOAD] VisitanteId: '{visitanteId}'");
                System.Diagnostics.Debug.WriteLine($"[UPLOAD] Tipo: {tipo}");
                
                // Validar parámetros de entrada
                if (string.IsNullOrWhiteSpace(imagePath))
                {
                    return new ImageUploadResult 
                    { 
                        IsSuccess = false, 
                        Message = "La ruta de la imagen está vacía" 
                    };
                }
                
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return new ImageUploadResult 
                    { 
                        IsSuccess = false, 
                        Message = "El nombre del archivo está vacío" 
                    };
                }
                
                if (string.IsNullOrWhiteSpace(visitanteId))
                {
                    return new ImageUploadResult 
                    { 
                        IsSuccess = false, 
                        Message = "El visitanteId está vacío" 
                    };
                }
                
                // Validar que visitanteId sea un GUID válido (como requiere el servidor)
                if (!Guid.TryParse(visitanteId, out _))
                {
                    return new ImageUploadResult 
                    { 
                        IsSuccess = false, 
                        Message = $"El visitanteId no es un GUID válido: {visitanteId}" 
                    };
                }
                
                // Validar que el tipo sea válido (1 = placas, 2 = documento)
                if (tipo != 1 && tipo != 2)
                {
                    return new ImageUploadResult 
                    { 
                        IsSuccess = false, 
                        Message = $"Tipo inválido: {tipo}. Debe ser 1 (placas) o 2 (documento)" 
                    };
                }
                
                if (!File.Exists(imagePath))
                {
                    return new ImageUploadResult 
                    { 
                        IsSuccess = false, 
                        Message = $"El archivo de imagen no existe en la ruta: {imagePath}" 
                    };
                }

                var url = $"{BASE_URL}/api/CodigoQR/SubirImagen";
                System.Diagnostics.Debug.WriteLine($"[UPLOAD] URL del endpoint: {url}");
                
                using var form = new MultipartFormDataContent();
                using var fileStream = File.OpenRead(imagePath);
                using var streamContent = new StreamContent(fileStream);
                
                // Detectar el tipo de contenido basado en la extensión
                var extension = Path.GetExtension(fileName).ToLowerInvariant();
                var contentType = extension switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    _ => "application/octet-stream"
                };
                
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
                
                // Agregar todos los parámetros requeridos según Swagger
                form.Add(streamContent, "archivo", fileName);
                form.Add(new StringContent(visitanteId), "visitanteId");
                form.Add(new StringContent(tipo.ToString()), "tipo");
                
                System.Diagnostics.Debug.WriteLine($"[UPLOAD] Form data preparada:");
                
                var response = await _httpClient.PostAsync(url, form);
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[UPLOAD] Respuesta exitosa: {jsonContent}");
                    
                    // Si el servidor devuelve 200 OK, asumimos que la subida fue exitosa
                    // independientemente del formato JSON de respuesta
                    return new ImageUploadResult 
                    { 
                        IsSuccess = true, 
                        Message = "Imagen subida correctamente al servidor",
                        FileName = fileName,
                        Url = $"{BASE_URL}/uploads/{fileName}", // URL estimada
                        ImagenId = Guid.NewGuid().ToString(),
                        VisitanteId = visitanteId
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[UPLOAD] Error HTTP: {response.StatusCode} - {errorContent}");
                    
                    return new ImageUploadResult 
                    { 
                        IsSuccess = false, 
                        Message = $"Error del servidor: {response.StatusCode} - {errorContent}" 
                    };
                }
            }
            catch (TaskCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("[UPLOAD] Timeout en la subida");
                return new ImageUploadResult 
                { 
                    IsSuccess = false, 
                    Message = "Timeout: La subida tardó demasiado tiempo" 
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[UPLOAD] Excepción: {ex.Message}");
                return new ImageUploadResult 
                { 
                    IsSuccess = false, 
                    Message = $"Error de conexión: {ex.Message}" 
                };
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    public class ImageUploadResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string ImagenId { get; set; } = string.Empty;
        public string VisitanteId { get; set; } = string.Empty;
    }

    public class ImageUploadResponse
    {
        public string? Mensaje { get; set; }
        public string? NombreArchivo { get; set; }
        public string? Url { get; set; }
        public string? ImagenId { get; set; }
        public string? VisitanteId { get; set; }
        public string? Message { get; set; } // Alternativa en inglés
        public string? FileName { get; set; } // Alternativa en inglés
        public string? Filename { get; set; } // Otra variante
        public bool? Success { get; set; }
        public bool? Successful { get; set; }
    }
}
