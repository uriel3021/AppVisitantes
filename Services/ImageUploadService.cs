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

        public async Task<ImageUploadResult> UploadImageAsync(string imagePath, string fileName)
        {
            try
            {
                if (!File.Exists(imagePath))
                {
                    return new ImageUploadResult 
                    { 
                        IsSuccess = false, 
                        Message = "El archivo de imagen no existe" 
                    };
                }

                var url = $"{BASE_URL}/api/CodigoQR/SubirImagen";
                
                System.Diagnostics.Debug.WriteLine($"[UPLOAD] Subiendo imagen: {imagePath} como {fileName}");
                
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
                form.Add(streamContent, "archivo", fileName);
                
                var response = await _httpClient.PostAsync(url, form);
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[UPLOAD] Respuesta exitosa: {jsonContent}");
                    
                    // Parse de la respuesta JSON
                    var result = System.Text.Json.JsonSerializer.Deserialize<ImageUploadResponse>(jsonContent, new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    return new ImageUploadResult 
                    { 
                        IsSuccess = true, 
                        Message = result?.Mensaje ?? "Imagen subida correctamente",
                        FileName = result?.NombreArchivo ?? fileName,
                        Url = result?.Url ?? ""
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
    }

    public class ImageUploadResponse
    {
        public string Mensaje { get; set; } = string.Empty;
        public string NombreArchivo { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}
