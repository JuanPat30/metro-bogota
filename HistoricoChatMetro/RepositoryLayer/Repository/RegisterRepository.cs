using AutoMapper;
using Commun.Logger;
using DomainLayer.Dtos;
using DomainLayer.Models;
using Google.Cloud.Firestore;
using RepositoryLayer.Data;
using RepositoryLayer.IRepository;
using System.Globalization;

namespace RepositoryLayer.Repository
{
    /// <summary>
    /// Gabriela Muñoz
    /// Clase del repositorio de bitacora
    /// </summary>
    public class RegisterRepository : IRegisterRepository
    {
        private readonly FirestoreDb _db;
        private readonly ICreateLogger _createLogger;
        private readonly IMapper _mapper;

        private readonly string collectionChats = "UsersChats";
        private readonly string collectionConv = "Conversations";

        /// <summary>
        /// Gabriela Muñoz
        /// Constructor de la clase
        /// </summary>
        /// <param name="objContext"></param>
        /// <param name="mapper"></param>
        /// <param name="createLogger"></param>
        public RegisterRepository(FirestoreDbContext objContext, IMapper mapper, ICreateLogger createLogger)
        {
            _db = objContext.GetDb();
            _mapper = mapper;
            _createLogger = createLogger;
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método que devuelve la lista de usuarios
        /// </summary>
        /// <returns>Lista de usuarios</returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<string>> GetUsers()
        {
            try
            {
                var query = _db.CollectionGroup(collectionConv);
                var snapshot = await query.GetSnapshotAsync();

                var tasks = snapshot.Documents.Select(d =>
                {
                    return d.Reference.Parent.Parent.Id;

                }).Distinct().ToList();

                return tasks;
            }
            catch (Exception ex)
            {
                _createLogger.LogWriteExcepcion(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método para obtener todas las conversaciones de todos los usuarios
        /// con filtros de fecha, nombre, estado y ordenamiento por fechas
        /// </summary>
        /// <param name="name"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="status"></param>
        /// <param name="isDescending"></param>
        /// <returns>lista de conversaciones</returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<ConversationsUserDto>> GetAllConversations(string? name = null, DateTime? from = null, DateTime? to = null, bool? status = null, bool? isDescending = null)
        {
            try
            {

                Query query = !string.IsNullOrEmpty(name) ? _db.Collection(collectionChats).Document(name).Collection(collectionConv): _db.CollectionGroup(collectionConv);

                if (from.HasValue)
                {
                    query = query.WhereGreaterThanOrEqualTo("Date", Timestamp.FromDateTime(from.Value.Date.ToUniversalTime()));
                }
                if (to.HasValue)
                {
                    query = query.WhereLessThanOrEqualTo("Date", Timestamp.FromDateTime(to.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime()));
                }
                if (status.HasValue)
                {
                    query = query.WhereEqualTo("Estado", status.Value);
                }
                query = isDescending.GetValueOrDefault(true) ? query.OrderByDescending("Date") : query.OrderBy("Date");

                var snapshot = await query.GetSnapshotAsync();
                var conversations = await Task.WhenAll(snapshot.Documents.Select(async d =>
                {
                    await Task.Yield();
                    var conversation = d.ConvertTo<Conversation>();
                    var userId = d.Reference.Parent.Parent.Id;
                    TimeZoneInfo zonaHoraria = TimeZoneInfo.FindSystemTimeZoneById("America/Bogota");
                    return new ConversationsUserDto
                    {
                        ConversationId = conversation.UuidConversation,
                        UserName = userId,
                        Name = conversation.Name,
                        Date = TimeZoneInfo.ConvertTimeFromUtc(conversation.Date!.Value, zonaHoraria).ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture),
                        Estado = conversation.Estado ? "Activo" : "Inactivo"
                    };
                }));

                return conversations.ToList();
            }
            catch (Exception ex)
            {
                _createLogger.LogWriteExcepcion(ex.Message);
                throw new Exception(ex.Message);
            }

        }

    }
}