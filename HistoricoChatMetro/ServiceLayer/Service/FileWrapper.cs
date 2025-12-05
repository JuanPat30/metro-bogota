using ServiceLayer.IService;

namespace ServiceLayer.Service
{
    /// <summary>
    /// Clase de Wrapper para archivos
    /// </summary>
    public class FileWrapper : IFileWrapper
    {
        /// <summary>
        /// Gabriela Muñoz
        /// Metodo para eliminar un archivo
        /// </summary>
        /// <param name="path"></param>
        public void Delete(string path)
        {
            File.Delete(path);
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método para leer el archivo
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }
    }
}
