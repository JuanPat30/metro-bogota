using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.IService
{
    public interface IFileStoreService
    {
        /// <summary>
        /// Subir un archivo a Google Cloud Storage
        /// </summary>
        /// <param name="fileName">Nombre del archivo</param>
        /// <param name="filePath">Ruta del archivo</param>
        /// <returns>URL del archivo subido</returns>
        Task<string> UploadFileAsync(string bucket, string localPath, string objectName);
    }
}
