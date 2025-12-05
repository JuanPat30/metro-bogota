using DomainLayer.Dtos;
using DomainLayer.Models;
using RepositoryLayer.IRepository;
using ServiceLayer.IService;

namespace ServiceLayer.Service
{
    /// <summary>
    /// Gabriela Muñoz
    /// Clase para Bitacora
    /// </summary>
    public class RegisterService : IRegisterService
    {
        private readonly IRegisterRepository _registerRepository;

        /// <summary>
        /// Gabriela Muñoz
        /// Constructor de la clase
        /// </summary>
        /// <param name="fireBaseRepository"></param>
        public RegisterService(IRegisterRepository fireBaseRepository)
        {
            _registerRepository = fireBaseRepository;
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método para obtener los usuarios de la base de datos
        /// </summary>
        /// <returns>Result</returns>
        public async Task<Result> GetUsers()
        {
            Result result = new Result();
           
            result.Success = true;
            result.MessageHttp = Commun.Constants.msjMs200;
            result.Data = await _registerRepository.GetUsers();
            return result;
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método para obtener todas las conversaciones de los usuarios
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="name"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="status"></param>
        /// <param name="isDescending"></param>
        /// <returns>Result</returns>
        public async Task<Result> GetAll(int page, int pageSize, string? name = null, DateTime? from = null, DateTime? to = null, bool? status = null, bool? isDescending = null)
        {
            Result result = new Result();

            var allConversations = await _registerRepository.GetAllConversations(name, from, to, status, isDescending);

            if(allConversations == null)
            {
                return ValidationConversations(result);
            }

            int totalItems = allConversations.Count;
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var paginatedConversations = allConversations
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var paginatedResponse = new PaginatedResponseDto<ConversationsUserDto>
            {
                Items = paginatedConversations,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            };

            result.Success = true;
            result.MessageHttp = Commun.Constants.msjMs200;
            result.Data = paginatedResponse;


            return result;
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Validación interna cuando las conversaciones no existen
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private Result ValidationConversations(Result result)
        {
            result.Success = true;
            result.MessageHttp = Commun.Constants.msjMs200;
            result.Data = null; 
            return result;
        }
    }
}