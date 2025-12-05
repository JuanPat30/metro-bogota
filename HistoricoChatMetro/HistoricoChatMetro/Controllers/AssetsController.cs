using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.IService;
using ServiceLayer.Service;

namespace HistoricoChatMetro.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AssetsController : ControllerBase
    {
        
        private readonly IFileStoreService _fileStoreService;
        private string ? _bucketName;

        public AssetsController(IConfiguration configuration, IFileStoreService fileStoreService)
        {
            _fileStoreService = fileStoreService;

            _bucketName = configuration["GoogleCloud:BucketAnalysisFiles"];
        }

        [HttpPost("UploadFileForAnalyze")]
        public async Task<IActionResult> UploadFileForAnalyze(IFormFile file)
        {
            if (string.IsNullOrEmpty(_bucketName))
            {
                return BadRequest("El nombre del bucket no está configurado en la aplicación. Por favor, verifica la configuración de Google Cloud Storage.");
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest("No se proporcionó ningún archivo o el archivo está vacío.");
            }

            if (file.Length > 30 * 1024 * 1024) // 30 MB
            {
                return BadRequest("El archivo no puede exceder los 30 MB.");
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (fileExtension != ".xlsx" && fileExtension != ".xls" && fileExtension != ".pdf" && fileExtension != ".docx" && fileExtension != ".doc")
            {
                return BadRequest("Solo se permiten archivos de Excel (.xlsx, .xls), PDF y Word (.docx, .doc).");
            }

            var tempFilePath = Path.GetTempFileName();

            try
            {
                string userId = User.GetEmail();
                using (var stream = System.IO.File.Create(tempFilePath))
                {
                    await file.CopyToAsync(stream);
                }

                var objectName = $"{userId.Replace("@", "_").Replace(".", "_")}-{Guid.NewGuid()}-{Path.GetFileName(file.FileName)}".Replace(" ", "_");

                string gcsUri = await _fileStoreService.UploadFileAsync(
                    _bucketName,
                    tempFilePath,
                    objectName);

                return Ok(new { Uri = gcsUri, OriginalName = file.FileName, Size = file.Length });
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"Error - Archivo no encontrado: {ex.Message}");
                return NotFound($"No se pudo encontrar el archivo temporal: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al subir el archivo para análisis: {ex.Message}");
                return StatusCode(500, $"Ocurrió un error interno al procesar el archivo: {ex.Message}");
            }
            finally
            {
                // 6. Asegurarse de eliminar el archivo temporal
                if (System.IO.File.Exists(tempFilePath))
                {
                    System.IO.File.Delete(tempFilePath);
                }
            }
        }

    }
}
