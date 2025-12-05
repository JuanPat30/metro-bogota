using AutoMapper;
using DomainLayer.Dtos;
using DomainLayer.Models;

namespace DomainLayer.EntityMapper
{
    public class AutoMapper : Profile
    {
        public AutoMapper()
        {
            CreateMap<Conversation, ConversationDto>().ReverseMap();
            CreateMap<ConversationDto, Conversation>();

            CreateMap<MessageChatBot, MessageChatBotDto>().ReverseMap();
            CreateMap<MessageChatBotDto, MessageChatBot>();
        }
    }
}
