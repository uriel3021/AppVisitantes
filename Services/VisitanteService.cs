using KYCApp.Data;
using KYCApp.Models;
using Microsoft.EntityFrameworkCore;

namespace KYCApp.Services
{
    public class VisitanteService
    {
        private readonly KYCDbContext _context;

        public VisitanteService(KYCDbContext context)
        {
            _context = context;
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                await _context.Database.CanConnectAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Visitante>> GetAllVisitantesAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== Iniciando consulta de Visitantes ===");
                
                // Log de la query SQL generada
                var query = _context.Visitantes.AsQueryable();
                var sql = query.ToQueryString();
                System.Diagnostics.Debug.WriteLine($"SQL Query: {sql}");
                
                var result = await query.ToListAsync();
                System.Diagnostics.Debug.WriteLine($"Resultados encontrados: {result.Count}");
                
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al obtener visitantes: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return new List<Visitante>();
            }
        }

        public async Task<List<CodigoQr>> GetAllCodigosQRAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== Iniciando consulta de Códigos QR ===");
                
                // Log de la query SQL generada
                var query = _context.CodigoQrs.Include(c => c.Visitante).AsQueryable();
                var sql = query.ToQueryString();
                System.Diagnostics.Debug.WriteLine($"SQL Query: {sql}");
                
                var result = await query.ToListAsync();
                System.Diagnostics.Debug.WriteLine($"Resultados encontrados: {result.Count}");
                
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al obtener códigos QR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return new List<CodigoQr>();
            }
        }

        public async Task<Visitante?> GetVisitanteByIdAsync(Guid id)
        {
            try
            {
                return await _context.Visitantes.FindAsync(id);
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> ValidateQRCodeAsync(string qrCode)
        {
            try
            {
                var codigo = await _context.CodigoQrs
                    .Include(c => c.Visitante)
                    .FirstOrDefaultAsync(c => c.Codigo == qrCode);
                
                return codigo != null;
            }
            catch
            {
                return false;
            }
        }

        // Método adicional para diagnosticar problemas de esquema
        public async Task<string> DiagnoseSchemaAsync()
        {
            try
            {
                var result = "";
                
                // Verificar si las tablas existen en el esquema por defecto
                var tableExistsQuery = @"
                    SELECT TABLE_SCHEMA, TABLE_NAME 
                    FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_NAME IN ('Visitante', 'CodigoQR')
                    ORDER BY TABLE_SCHEMA, TABLE_NAME";
                
                System.Diagnostics.Debug.WriteLine($"Ejecutando diagnóstico de esquema...");
                
                using var command = _context.Database.GetDbConnection().CreateCommand();
                command.CommandText = tableExistsQuery;
                
                await _context.Database.OpenConnectionAsync();
                using var reader = await command.ExecuteReaderAsync();
                
                result += "=== Tablas encontradas ===\n";
                while (await reader.ReadAsync())
                {
                    var schema = reader["TABLE_SCHEMA"].ToString();
                    var tableName = reader["TABLE_NAME"].ToString();
                    result += $"Schema: {schema}, Tabla: {tableName}\n";
                    System.Diagnostics.Debug.WriteLine($"Tabla encontrada - Schema: {schema}, Tabla: {tableName}");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                var error = $"Error en diagnóstico: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(error);
                return error;
            }
        }
    }
}
