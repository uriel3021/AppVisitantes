using KYCApp.Data;
using KYCApp.Models;
using Microsoft.EntityFrameworkCore;

namespace KYCApp
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
                // Log para debugging
                System.Diagnostics.Debug.WriteLine("Ejecutando consulta de Visitantes...");
                var visitantes = await _context.Visitantes.ToListAsync();
                System.Diagnostics.Debug.WriteLine($"Visitantes encontrados: {visitantes.Count}");
                return visitantes;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en GetAllVisitantesAsync: {ex.Message}");
                return new List<Visitante>();
            }
        }

        public async Task<List<CodigoQr>> GetAllCodigosQRAsync()
        {
            try
            {
                // Log para debugging
                System.Diagnostics.Debug.WriteLine("Ejecutando consulta de Códigos QR...");
                var codigos = await _context.CodigoQrs.Include(c => c.Visitante).ToListAsync();
                System.Diagnostics.Debug.WriteLine($"Códigos QR encontrados: {codigos.Count}");
                return codigos;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en GetAllCodigosQRAsync: {ex.Message}");
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
    }
}
