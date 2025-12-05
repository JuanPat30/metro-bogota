using DomainLayer.Dtos;
using DomainLayer.Models;

namespace ServiceLayer.IService
{
    public interface IReportService
    {
        Result GetReportExcel(PaginatedResponseDto<ConversationsUserDto> reportDto);
        Task<Result> GeneratePdf(string userId, string conversationId);
     
    }
}
