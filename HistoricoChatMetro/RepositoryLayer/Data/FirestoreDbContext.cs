using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;

namespace RepositoryLayer.Data
{
    public class FirestoreDbContext
    {
        private readonly FirestoreDb _db;

        /// <summary>
        /// Gabriela Muñoz
        /// Configuración conexión a Base de datos
        /// </summary>
        /// <param name="configuration"></param>
        public FirestoreDbContext(IConfiguration configuration)
        {
            FirestoreDbBuilder builder = new FirestoreDbBuilder
            {
                ProjectId = configuration["GoogleCloud:ProjectId"],
                DatabaseId = configuration["GoogleCloud:DatabaseId"]
            };

            _db = builder.Build();
        }

        /// <summary>
        /// Retorno de conexion
        /// </summary>
        /// <returns></returns>
        public FirestoreDb GetDb() => _db;
    }
}

