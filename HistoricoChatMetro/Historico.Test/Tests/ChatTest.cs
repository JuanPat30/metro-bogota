using Moq;
using ServiceLayer.Service;
using RepositoryLayer.IRepository;
using Commun;
using Google.Cloud.Firestore;
using DomainLayer.Dtos;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using System.Globalization;
using iText.Kernel.Geom;
using AutoMapper;
using DomainLayer.Models;
using NuGet.Frameworks;
using Commun.Logger;
using ServiceLayer.IService;

public class ChatTest
{
    private (Mock<IChatRepository>, Mock<IMapper>) CreateMocks()
    {
        return (new Mock<IChatRepository>(), new Mock<IMapper>());
    }
    #region Pruebas método GetConversationByUser()
    [Fact]
    public async Task GetConversationByUserList()
    {
        var mockRepository = new Mock<IChatRepository>();
        var mockMapper = new Mock<IMapper>();

        var conversationDto = new ConversationDto
        {
            UuidConversation = "ce55348c-61a1-4264-99c4-1d9d67974ab9",
            Name = "Primera Conversación",
            Date = DateTime.UtcNow,
            Estado = true,
            Messages = new List<MessageChatBotDto>()
        };

        var conversationsEntity = new List<ConversationDto> { conversationDto };

        mockRepository.Setup(repo => repo.GetConversationByUser("prueba@gmail.com", null, null, null))
            .ReturnsAsync(conversationsEntity);

        var service = new ChatService(mockRepository.Object, mockMapper.Object);

        var datePreviousUpdate = conversationDto.Date.Value;
        var result = await service.GetConversationByUser("prueba@gmail.com");

        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs200, result.MessageHttp);
        Assert.NotNull(result.Data);

        var response = (List<ConversationDto>)result.Data;
        Assert.Equal(1, response.Count);
        Assert.Equal(conversationDto.UuidConversation, response[0].UuidConversation);
        Assert.Equal(conversationDto.Name, response[0].Name);

        TimeZoneInfo zonaHoraria = TimeZoneInfo.FindSystemTimeZoneById("America/Bogota");
        var expectedDate = TimeZoneInfo.ConvertTimeFromUtc(datePreviousUpdate, zonaHoraria);
        Assert.Equal(expectedDate, response[0].Date);
    }

    [Fact]
    public async Task GetConversationByUserEmptyConversations()
    {
        var mockRepository = new Mock<IChatRepository>();
        var mockMapper = new Mock<IMapper>();

        mockRepository.Setup(repo => repo.GetConversationByUser("prueba@gmail.com", null, null, null))
            .ReturnsAsync(new List<ConversationDto>());

        var service = new ChatService(mockRepository.Object, mockMapper.Object);

        var result = await service.GetConversationByUser("prueba@gmail.com");

        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs200, result.MessageHttp);
        Assert.NotNull(result.Data);

        var conversations = (List<ConversationDto>)result.Data;
        Assert.Empty(conversations);
    }

    [Fact]
    public async Task GetConversationByUserReturnsNull()
    {
        var mockRepository = new Mock<IChatRepository>();
        var mockMapper = new Mock<IMapper>();

        mockRepository.Setup(repo => repo.GetConversationByUser("prueba@gmail.com", null, null, null)).ReturnsAsync((List<ConversationDto>)null);

        var service = new ChatService(mockRepository.Object, mockMapper.Object);

        var result = await service.GetConversationByUser("prueba@gmail.com");

        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs200, result.MessageHttp);
        Assert.Equal(Constants.CONV_NO_EXIST, result.Data);
    }

    [Fact]
    public async Task GetConversationByUserFilterName()
    {
        var mockRepository = new Mock<IChatRepository>();
        var mockMapper = new Mock<IMapper>();

        var conversationDto = new ConversationDto
        {
            UuidConversation = "ce55348c-61a1-4264-99c4-1d9d67974ab9",
            Name = "Primera Conversación",
            Date = DateTime.UtcNow,
            Estado = true,
            Messages = new List<MessageChatBotDto>()
        };
        var conversationDto2 = new ConversationDto
        {
            UuidConversation = "de55348c-61a1-4264-99c4-1d9d67974ab1",
            Name = "Segunda Conversación",
            Date = DateTime.UtcNow.AddDays(3),
            Estado = false,
            Messages = new List<MessageChatBotDto>()
        };

        var conversationDto3 = new ConversationDto
        {
            UuidConversation = "fe55348c-61a1-4264-99c4-1d9d67974ab2",
            Name = "Nuevo Estado",
            Date = DateTime.UtcNow.AddDays(1),
            Estado = true,
            Messages = new List<MessageChatBotDto>()
        };

        var conversationsEntity = new List<ConversationDto> { conversationDto, conversationDto2, conversationDto3 };

        var filter = conversationsEntity.Where(c => c.Name.Contains("Conversa")).ToList();
        mockRepository.Setup(repo => repo.GetConversationByUser("prueba@gmail.com", "Conversa", null, null))
            .ReturnsAsync(filter);

        var service = new ChatService(mockRepository.Object, mockMapper.Object);

        var result = await service.GetConversationByUser("prueba@gmail.com", "Conversa");

        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs200, result.MessageHttp);
        Assert.NotNull(result.Data);

        var response = (List<ConversationDto>)result.Data;
        Assert.Equal(filter.Count, response.Count);
        Assert.Equal(filter, response);
        Assert.Contains(conversationDto, response);
        Assert.Contains(conversationDto2, response);
        Assert.DoesNotContain(conversationDto3, response);
    }

    [Fact]
    public async Task GetConversationByUserFilterDates()
    {
        var mockRepository = new Mock<IChatRepository>();
        var mockMapper = new Mock<IMapper>();

        var conversationDto = new ConversationDto
        {
            UuidConversation = "ce55348c-61a1-4264-99c4-1d9d67974ab9",
            Name = "Primera Conversación",
            Date = DateTime.UtcNow,
            Estado = true,
            Messages = new List<MessageChatBotDto>()
        };
        var conversationDto2 = new ConversationDto
        {
            UuidConversation = "de55348c-61a1-4264-99c4-1d9d67974ab1",
            Name = "Segunda Conversación",
            Date = DateTime.UtcNow.AddDays(3),
            Estado = false,
            Messages = new List<MessageChatBotDto>()
        };

        var conversationDto3 = new ConversationDto
        {
            UuidConversation = "fe55348c-61a1-4264-99c4-1d9d67974ab2",
            Name = "Nuevo Estado",
            Date = DateTime.UtcNow.AddDays(1),
            Estado = true,
            Messages = new List<MessageChatBotDto>()
        };

        var conversationsEntity = new List<ConversationDto> { conversationDto, conversationDto2, conversationDto3 };

        var fromDate = DateTime.UtcNow.AddDays(-1);
        var toDate = DateTime.UtcNow.AddDays(1);

        var filter = conversationsEntity.FindAll(c => c.Date >= fromDate && c.Date <= toDate);
        mockRepository.Setup(repo => repo.GetConversationByUser("prueba@gmail.com", null, fromDate, toDate))
            .ReturnsAsync(filter);

        var service = new ChatService(mockRepository.Object, mockMapper.Object);

        var result = await service.GetConversationByUser("prueba@gmail.com", from: fromDate, to: toDate);

        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs200, result.MessageHttp);
        Assert.NotNull(result.Data);

        var response = (List<ConversationDto>)result.Data;
        TimeZoneInfo zonaHoraria = TimeZoneInfo.FindSystemTimeZoneById("America/Bogota");

        Assert.Equal(filter.Count, response.Count);
        Assert.Equal(filter, response);
        Assert.Equal(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow.Date,zonaHoraria), TimeZoneInfo.ConvertTimeFromUtc(response[0].Date.Value.Date, zonaHoraria));
        Assert.Contains(conversationDto, response);
        Assert.Contains(conversationDto3, response);
        Assert.DoesNotContain(conversationDto2, response);

    }

    [Fact]
    public async Task GetConversationByUserFilterAll()
    {
        var mockRepository = new Mock<IChatRepository>();
        var mockMapper = new Mock<IMapper>();

        var conversationDto = new ConversationDto
        {
            UuidConversation = "ce55348c-61a1-4264-99c4-1d9d67974ab9",
            Name = "Primera Conversación",
            Date = DateTime.UtcNow,
            Estado = true,
            Messages = new List<MessageChatBotDto>()
        };
        var conversationDto2 = new ConversationDto
        {
            UuidConversation = "de55348c-61a1-4264-99c4-1d9d67974ab1",
            Name = "Segunda Conversación",
            Date = DateTime.UtcNow.AddDays(3),
            Estado = false,
            Messages = new List<MessageChatBotDto>()
        };

        var conversationDto3 = new ConversationDto
        {
            UuidConversation = "fe55348c-61a1-4264-99c4-1d9d67974ab2",
            Name = "Nuevo Estado",
            Date = DateTime.UtcNow.AddDays(1),
            Estado = true,
            Messages = new List<MessageChatBotDto>()
        };

        var conversationsEntity = new List<ConversationDto> { conversationDto, conversationDto2, conversationDto3 };

        var fromDate = DateTime.UtcNow.AddDays(-1);
        var toDate = DateTime.UtcNow.AddDays(1);
        var name = "Conversa";

        var filter = conversationsEntity.FindAll(c => c.Name.Contains(name) && c.Date >= fromDate && c.Date <= toDate);
        mockRepository.Setup(repo => repo.GetConversationByUser("prueba@gmail.com", name, fromDate, toDate))
            .ReturnsAsync(filter);

        var service = new ChatService(mockRepository.Object, mockMapper.Object);

        var result = await service.GetConversationByUser("prueba@gmail.com", name: name, from: fromDate, to: toDate);

        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs200, result.MessageHttp);
        Assert.NotNull(result.Data);

        TimeZoneInfo zonaHoraria = TimeZoneInfo.FindSystemTimeZoneById("America/Bogota");
        var response = (List<ConversationDto>)result.Data;

        Assert.Equal(filter.Count, response.Count);
        Assert.Single(response);
        Assert.Equal(filter, response);
        Assert.Equal(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow.Date, zonaHoraria), TimeZoneInfo.ConvertTimeFromUtc(response[0].Date.Value.Date, zonaHoraria));
        Assert.Contains(conversationDto, response);
        Assert.DoesNotContain(conversationDto3, response);
        Assert.DoesNotContain(conversationDto2, response);
    }

    [Fact]
    public async Task GetConversationByUserEmptyConversationsWithFilter()
    {
        var mockRepository = new Mock<IChatRepository>();
        var mockMapper = new Mock<IMapper>();

        mockRepository.Setup(repo => repo.GetConversationByUser("prueba@gmail.com", "Conversa", null, null))
            .ReturnsAsync(new List<ConversationDto>());

        var service = new ChatService(mockRepository.Object, mockMapper.Object);

        var result = await service.GetConversationByUser("prueba@gmail.com", name: "Conversa");

        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs200, result.MessageHttp);
        Assert.NotNull(result.Data);

        var conversations = (List<ConversationDto>)result.Data;
        Assert.Empty(conversations);
    }

    [Fact]
    public async Task GetConversationByUserThrowException()
    {

        var mockRepository = new Mock<IChatRepository>();
        var mockMapper = new Mock<IMapper>();

        mockRepository.Setup(repo => repo.GetConversationByUser("prueba@gmail.com", null, null, null))
            .ThrowsAsync(new System.Exception("Repository error"));

        var service = new ChatService(mockRepository.Object, mockMapper.Object);

        await Assert.ThrowsAsync<Exception>(() => service.GetConversationByUser("prueba@gmail.com"));
    }
    #endregion

    #region Pruebas método GetConversationById()

    [Fact]
    public async Task GetConversationByIdConte()
    {
        var mockRepository = new Mock<IChatRepository>();
        var mockMapper = new Mock<IMapper>();

        var conversationDto = new ConversationDto
        {
            UuidConversation = "ce55348c-61a1-4264-99c4-1d9d67974ab9",
            Name = "Primera Conversación",
            Date = DateTime.UtcNow,
            Estado = true,
            Messages = new List<MessageChatBotDto> { new MessageChatBotDto { Fecha = DateTime.UtcNow } }
        };
        var conversationDto2 = new ConversationDto
        {
            UuidConversation = "de55348c-61a1-4264-99c4-1d9d67974ab1",
            Name = "Segunda Conversación",
            Date = DateTime.UtcNow.AddDays(3),
            Estado = false,
            Messages = new List<MessageChatBotDto> { new MessageChatBotDto { Fecha = DateTime.UtcNow.AddDays(3) } }
        };

        var conversationDto3 = new ConversationDto
        {
            UuidConversation = "fe55348c-61a1-4264-99c4-1d9d67974ab2",
            Name = "Nuevo Estado",
            Date = DateTime.UtcNow.AddDays(1),
            Estado = true,
            Messages = new List<MessageChatBotDto> { new MessageChatBotDto { Fecha = DateTime.UtcNow.AddDays(1) } }
        };

        var conversationsEntity = new List<ConversationDto> { conversationDto, conversationDto2, conversationDto3 };

        string userId = "prueba@gmail.com";
        string id = "ce55348c-61a1-4264-99c4-1d9d67974ab9";

        ConversationDto filter = conversationsEntity.Where(c => c.UuidConversation == id).FirstOrDefault();

        var datePreviousUpdate = conversationDto.Date.Value;
        var dateMessagePreviousUpdate = conversationDto.Messages.FirstOrDefault().Fecha.Value;
        mockRepository.Setup(repo => repo.GetConversationById(userId, id))
            .ReturnsAsync(filter);

        var service = new ChatService(mockRepository.Object, mockMapper.Object);

        var result = await service.GetConversationById(userId:userId, conversationId:id);

        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs200, result.MessageHttp);
        Assert.NotNull(result.Data);

        var response = (ConversationDto)result.Data;
        Assert.Equal(filter, response);
        Assert.Equal(id, response.UuidConversation);

        TimeZoneInfo zonaHoraria = TimeZoneInfo.FindSystemTimeZoneById("America/Bogota");
        var expectedDate = TimeZoneInfo.ConvertTimeFromUtc(datePreviousUpdate, zonaHoraria);
        var expectedDateMessage = TimeZoneInfo.ConvertTimeFromUtc(dateMessagePreviousUpdate, zonaHoraria);
        Assert.Equal(expectedDate, response.Date);
        Assert.Equal(expectedDateMessage, response.Messages.FirstOrDefault().Fecha);
    }

    [Fact]
    public async Task GetConversationByIdReturnsNull()
    {
        var mockRepository = new Mock<IChatRepository>();
        var mockMapper = new Mock<IMapper>();

        mockRepository.Setup(repo => repo.GetConversationById("prueba@gmail.com", "ce55348c-61a1-4264-99c4-1d9d67974ab9")).ReturnsAsync((ConversationDto)null);

        var service = new ChatService(mockRepository.Object, mockMapper.Object);

        var result = await service.GetConversationById("prueba@gmail.com", "ce55348c-61a1-4264-99c4-1d9d67974ab9");

        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs204, result.MessageHttp);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetConversationByIdWithOutUserId()
    {
        var mockRepository = new Mock<IChatRepository>();
        var mockMapper = new Mock<IMapper>();


        var service = new ChatService(mockRepository.Object, mockMapper.Object);

        var result = await service.GetConversationById(null, "ce55348c-61a1-4264-99c4-1d9d67974ab9");

        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs200, result.MessageHttp);
        Assert.Equal($"{Constants.PARAMS_REQUIRED} userId, conversationId",result.Data);
    }

    [Fact]
    public async Task GetConversationByIdWithOutConversationId()
    {
        var mockRepository = new Mock<IChatRepository>();
        var mockMapper = new Mock<IMapper>();


        var service = new ChatService(mockRepository.Object, mockMapper.Object);

        var result = await service.GetConversationById("prueba@gmail.com", null);

        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs200, result.MessageHttp);
        Assert.Equal($"{Constants.PARAMS_REQUIRED} userId, conversationId", result.Data);
    }
    [Fact]
    public async Task GetConversationByIdWithOutParams()
    {
        var mockRepository = new Mock<IChatRepository>();
        var mockMapper = new Mock<IMapper>();


        var service = new ChatService(mockRepository.Object, mockMapper.Object);

        var result = await service.GetConversationById(null, null);

        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs200, result.MessageHttp);
        Assert.Equal($"{Constants.PARAMS_REQUIRED} userId, conversationId", result.Data);
    }
    [Fact]
    public async Task GetConversationByIdThrowException()
    {

        var mockRepository = new Mock<IChatRepository>();
        var mockMapper = new Mock<IMapper>();

        mockRepository.Setup(repo => repo.GetConversationById("prueba@gmail.com", "ce55348c-61a1-4264-99c4-1d9d67974ab9"))
            .ThrowsAsync(new System.Exception("Repository error"));

        var service = new ChatService(mockRepository.Object, mockMapper.Object);

        await Assert.ThrowsAsync<Exception>(() => service.GetConversationById("prueba@gmail.com", "ce55348c-61a1-4264-99c4-1d9d67974ab9"));
    }
    #endregion

    #region Pruebas método SaveConversation()
    [Fact]
    public async Task SaveConversationWhenConversationNull()
    {
        var (mockChatRepository, mockMapper) = CreateMocks();

        var service = new ChatService(mockChatRepository.Object, mockMapper.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.SaveConversation("userId", null));
    }

    [Fact]
    public async Task SaveConversationWhenConversationDoesNotExist()
    {
        var (mockChatRepository, mockMapper) = CreateMocks();
        var mockDocumentReference = new Mock<IDocumentReferenceWrapper>();
        var mockDocumentSnapshot = new Mock<IDocumentSnapshotWrapper>();

        var conversationDto = new ConversationDto
        {
            UuidConversation = "ce55348c-61a1-4264-99c4-1d9d67974ab9",
            Name = "Primera Conversación",
            Date = DateTime.UtcNow,
            Estado = true
        };

        mockChatRepository.Setup(r => r.GetConversationByIdReference("prueba@yopmail.com", "ce55348c-61a1-4264-99c4-1d9d67974ab9"))
            .Returns(mockDocumentReference.Object);

        mockDocumentReference.Setup(r => r.GetSnapshotAsync())
            .ReturnsAsync(mockDocumentSnapshot.Object);

        mockDocumentSnapshot.Setup(s => s.ConvertTo<Conversation>())
            .Returns((Conversation)null);
        
        var service = new ChatService(mockChatRepository.Object, mockMapper.Object);

        var result = await service.SaveConversation("prueba@yopmail.com", conversationDto);

        mockChatRepository.Verify(r => r.Insert("prueba@yopmail.com", It.IsAny<ConversationDto>()), Times.Once);
        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs200, result.MessageHttp);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task SaveConversationWhenConversationExist()
    {
        var (mockChatRepository, mockMapper) = CreateMocks();
        var mockDocumentReference = new Mock<IDocumentReferenceWrapper>();
        var mockDocumentSnapshot = new Mock<IDocumentSnapshotWrapper>();

        var conversationDto = new ConversationDto
        {
            UuidConversation = "ce55348c-61a1-4264-99c4-1d9d67974ab9",
            Name = "Primera Conversación",
            Date = DateTime.UtcNow,
            Estado = true,
            Messages = new List<MessageChatBotDto> { new MessageChatBotDto { Fecha = DateTime.UtcNow } }
        };

        var conversation = new Conversation
        {
            UuidConversation = "ce55348c-61a1-4264-99c4-1d9d67974ab9",
            Name = "Primera Conversación",
            Date = DateTime.UtcNow,
            Estado = true,
            Messages = new List<MessageChatBot> { new MessageChatBot { Fecha = DateTime.UtcNow } }
        };

        var existConversationDto = new ConversationDto
        {
            UuidConversation = "ce55348c-61a1-4264-99c4-1d9d67974ab9",
            Name = "Primera Conversación",
            Date = DateTime.UtcNow,
            Estado = true,
            Messages = new List<MessageChatBotDto> { new MessageChatBotDto { Fecha = DateTime.UtcNow } }
        };

        mockChatRepository.Setup(r => r.GetConversationByIdReference("prueba@yopmail.com", "ce55348c-61a1-4264-99c4-1d9d67974ab9"))
            .Returns(mockDocumentReference.Object);

        mockDocumentSnapshot.Setup(s => s.ConvertTo<Conversation>())
            .Returns(conversation);

        mockMapper.Setup(m => m.Map<ConversationDto>(conversation))
        .Returns(existConversationDto);

        mockDocumentReference.Setup(r => r.GetSnapshotAsync())
            .ReturnsAsync(mockDocumentSnapshot.Object);
              

        var service = new ChatService(mockChatRepository.Object, mockMapper.Object);

        var result = await service.SaveConversation("prueba@yopmail.com", conversationDto);

        mockChatRepository.Verify(r => r.Update("prueba@yopmail.com", "ce55348c-61a1-4264-99c4-1d9d67974ab9", mockDocumentReference.Object, It.IsAny<ConversationDto>()), Times.Once);
        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs200, result.MessageHttp);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task SaveConversationWhenRepositoryThrowsException()
    {
        var (mockChatRepository, mockMapper) = CreateMocks();
        var mockDocumentReference = new Mock<IDocumentReferenceWrapper>();
        var mockDocumentSnapshot = new Mock<IDocumentSnapshotWrapper>();


        var conversationDto = new ConversationDto
        {
            UuidConversation = "ce55348c-61a1-4264-99c4-1d9d67974ab9",
            Name = "Primera Conversación",
            Date = DateTime.UtcNow,
            Estado = true,
            Messages = new List<MessageChatBotDto> { new MessageChatBotDto { Fecha = DateTime.UtcNow } }
        };

        mockChatRepository.Setup(r => r.GetConversationByIdReference("prueba@yopmail.com", "ce55348c-61a1-4264-99c4-1d9d67974ab9"))
            .Returns(mockDocumentReference.Object);

        mockDocumentReference.Setup(r => r.GetSnapshotAsync())
            .ThrowsAsync(new Exception("Repository Error"));

        var service = new ChatService(mockChatRepository.Object, mockMapper.Object);

        await Assert.ThrowsAsync<Exception>(() => service.SaveConversation("prueba@yopmail.com", conversationDto));
    }

    #endregion

    #region Pruebas método UpdateMessages()
    [Fact]
    public async Task UpdateMessagesWhenConversationDoesNotExist()
    {
        var (mockChatRepository, mockMapper) = CreateMocks();
        var mockDocumentReference = new Mock<IDocumentReferenceWrapper>();
        var mockDocumentSnapshot = new Mock<IDocumentSnapshotWrapper>();        

        mockChatRepository.Setup(r => r.GetConversationByIdReference("prueba@yopmail.com", "ce55348c-61a1-4264-99c4-1d9d67974ab9"))
            .Returns(mockDocumentReference.Object);

        mockDocumentSnapshot.Setup(s => s.Exists).Returns(false);

        mockDocumentReference.Setup(r => r.GetSnapshotAsync())
            .ReturnsAsync(mockDocumentSnapshot.Object);

        var service = new ChatService(mockChatRepository.Object, mockMapper.Object);

        var result = await service.UpdateMessages("prueba@yopmail.com", "ce55348c-61a1-4264-99c4-1d9d67974ab9");

        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs200, result.MessageHttp);
        Assert.Equal(Constants.CONV_NO_EXIST, result.Data);
    }

    [Fact]
    public async Task UpdateMessagesWhenConversationHasNoMessages()
    {
        var (mockChatRepository, mockMapper) = CreateMocks();
        var mockDocumentReference = new Mock<IDocumentReferenceWrapper>();
        var mockDocumentSnapshot = new Mock<IDocumentSnapshotWrapper>();

        var conversationDto = new ConversationDto
        {
            UuidConversation = "ce55348c-61a1-4264-99c4-1d9d67974ab9",
            Name = "Primera Conversación",
            Date = DateTime.UtcNow,
            Estado = true
        };

        var conversation = new Conversation
        {
            UuidConversation = "ce55348c-61a1-4264-99c4-1d9d67974ab9",
            Name = "Primera Conversación",
            Date = DateTime.UtcNow,
            Estado = true
        };

        var existConversationDto = new ConversationDto
        {
            UuidConversation = "ce55348c-61a1-4264-99c4-1d9d67974ab9",
            Name = "Primera Conversación",
            Date = DateTime.UtcNow,
            Estado = true
        };

        mockChatRepository.Setup(r => r.GetConversationByIdReference("prueba@yopmail.com", "ce55348c-61a1-4264-99c4-1d9d67974ab9"))
            .Returns(mockDocumentReference.Object);

        mockDocumentSnapshot.Setup(s => s.Exists).Returns(true);
        mockDocumentSnapshot.Setup(s => s.ConvertTo<Conversation>())
            .Returns(conversation);

        mockMapper.Setup(m => m.Map<ConversationDto>(conversation))
        .Returns(existConversationDto);

        mockDocumentReference.Setup(r => r.GetSnapshotAsync())
            .ReturnsAsync(mockDocumentSnapshot.Object);


        var service = new ChatService(mockChatRepository.Object, mockMapper.Object);

        var result = await service.UpdateMessages("prueba@yopmail.com", "ce55348c-61a1-4264-99c4-1d9d67974ab9");

        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs204, result.MessageHttp);
        Assert.Equal(Constants.MSJ_NO_EXIST, result.Data);
    }

    [Fact]
    public async Task UpdateMessagesWhenConversationHasMessages()
    {
        var (mockChatRepository, mockMapper) = CreateMocks();
        var mockDocumentReference = new Mock<IDocumentReferenceWrapper>();
        var mockDocumentSnapshot = new Mock<IDocumentSnapshotWrapper>();

        var conversationDto = new ConversationDto
        {
            UuidConversation = "ce55348c-61a1-4264-99c4-1d9d67974ab9",
            Name = "Primera Conversación",
            Date = DateTime.UtcNow,
            Estado = true,
            Messages = new List<MessageChatBotDto> { new MessageChatBotDto { Uuid = Guid.NewGuid().ToString() } }
        };

        var conversation = new Conversation
        {
            UuidConversation = "ce55348c-61a1-4264-99c4-1d9d67974ab9",
            Name = "Primera Conversación",
            Date = DateTime.UtcNow,
            Estado = true,
            Messages = new List<MessageChatBot> { new MessageChatBot { Uuid = Guid.NewGuid().ToString() } }
        };

        var updatedMessages = new ConversationDto
        {
            UuidConversation = "ce55348c-61a1-4264-99c4-1d9d67974ab9",
            Name = "Primera Conversación",
            Date = DateTime.UtcNow,
            Estado = true,
            Messages = new List<MessageChatBotDto>()
        };

        mockChatRepository.Setup(r => r.GetConversationByIdReference("prueba@yopmail.com", "ce55348c-61a1-4264-99c4-1d9d67974ab9"))
            .Returns(mockDocumentReference.Object);

        mockDocumentSnapshot.Setup(s => s.Exists).Returns(true);
        mockDocumentSnapshot.Setup(s => s.ConvertTo<Conversation>()).Returns(conversation);
        mockDocumentReference.Setup(r => r.GetSnapshotAsync()).ReturnsAsync(mockDocumentSnapshot.Object);
        mockMapper.Setup(m => m.Map<ConversationDto>(conversation)).Returns(conversationDto);

        mockChatRepository.Setup(r => r.UpdateMessages(mockDocumentReference.Object, It.IsAny<List<MessageChatBotDto>>()))
            .ReturnsAsync(updatedMessages);

        var service = new ChatService(mockChatRepository.Object, mockMapper.Object);

        var result = await service.UpdateMessages("prueba@yopmail.com", "ce55348c-61a1-4264-99c4-1d9d67974ab9");

        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs200, result.MessageHttp);
        Assert.Equal(updatedMessages, result.Data);
    }

    [Fact]
    public async Task UpdateMessagesWhenRepositoryThrowsException()
    {
        var (mockChatRepository, mockMapper) = CreateMocks();
        var mockDocumentReference = new Mock<IDocumentReferenceWrapper>();
        var mockDocumentSnapshot = new Mock<IDocumentSnapshotWrapper>();

        mockChatRepository.Setup(r => r.GetConversationByIdReference("prueba@yopmail.com", "ce55348c-61a1-4264-99c4-1d9d67974ab9"))
            .Returns(mockDocumentReference.Object);

        mockDocumentReference.Setup(r => r.GetSnapshotAsync())
            .ThrowsAsync(new Exception("Repository Error"));

        var service = new ChatService(mockChatRepository.Object, mockMapper.Object);

        await Assert.ThrowsAsync<Exception>(() => service.UpdateMessages("prueba@yopmail.com", "ce55348c-61a1-4264-99c4-1d9d67974ab9"));
    }
    #endregion

    #region Pruebas método DeleteConversation()
    [Fact]
    public async Task DeleteConversationWhenConversationExists()
    {
        var (mockChatRepository, mockMapper) = CreateMocks();

        var conversationDto = new ConversationDto
        {
            UuidConversation = "ce55348c-61a1-4264-99c4-1d9d67974ab9",
            Name = "Primera Conversación",
            Date = DateTime.UtcNow,
            Estado = true,
            Messages = new List<MessageChatBotDto> { new MessageChatBotDto { Uuid = Guid.NewGuid().ToString() } }
        };

        mockChatRepository.Setup(r => r.GetConversationById("prueba@yopmail.com", "ce55348c-61a1-4264-99c4-1d9d67974ab9"))
            .ReturnsAsync(conversationDto);
        mockChatRepository.Setup(r => r.DeleteConversation("prueba@yopmail.com", "ce55348c-61a1-4264-99c4-1d9d67974ab9", false))
            .ReturnsAsync(true);

        var service = new ChatService(mockChatRepository.Object, mockMapper.Object);

        var result = await service.DeleteConversation("prueba@yopmail.com", "ce55348c-61a1-4264-99c4-1d9d67974ab9", false);

        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs200, result.MessageHttp);
        Assert.True((bool)result.Data);
    }

    [Fact]
    public async Task DeleteConversationWhenConversationDoesNotExists()
    {
        var (mockChatRepository, mockMapper) = CreateMocks();
        
        mockChatRepository.Setup(r => r.GetConversationById("prueba@yopmail.com", "ce55348c-61a1-4264-99c4-1d9d67974ab9"))
            .ReturnsAsync((ConversationDto)null);
    
        var service = new ChatService(mockChatRepository.Object, mockMapper.Object);

        var result = await service.DeleteConversation("prueba@yopmail.com", "ce55348c-61a1-4264-99c4-1d9d67974ab9", false);

        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs204, result.MessageHttp);
        Assert.Equal(Constants.CONV_NO_EXIST, result.Data);
    }

    [Fact]
    public async Task DeleteConversationThrowsException()
    {
        var (mockChatRepository, mockMapper) = CreateMocks();

        var conversationDto = new ConversationDto
        {
            UuidConversation = "ce55348c-61a1-4264-99c4-1d9d67974ab9",
            Name = "Primera Conversación",
            Date = DateTime.UtcNow,
            Estado = true,
            Messages = new List<MessageChatBotDto> { new MessageChatBotDto { Uuid = Guid.NewGuid().ToString() } }
        };

        mockChatRepository.Setup(r => r.GetConversationById("prueba@yopmail.com", "ce55348c-61a1-4264-99c4-1d9d67974ab9"))
            .ReturnsAsync(conversationDto);

        mockChatRepository.Setup(r => r.DeleteConversation("prueba@yopmail.com", "ce55348c-61a1-4264-99c4-1d9d67974ab9", false))
            .ThrowsAsync(new Exception("Repository Error"));

        var service = new ChatService(mockChatRepository.Object, mockMapper.Object);

        var result = await service.DeleteConversation("prueba@yopmail.com", "ce55348c-61a1-4264-99c4-1d9d67974ab9", true);

        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs200, result.MessageHttp);
        Assert.False((bool)result.Data);
    }

    [Fact]
    public async Task DeleteConversationWhenGetConversationByIdThrowsException()
    {
        var (mockChatRepository, mockMapper) = CreateMocks();

        mockChatRepository.Setup(r => r.GetConversationById("prueba@yopmail.com", "ce55348c-61a1-4264-99c4-1d9d67974ab9"))
            .ThrowsAsync(new Exception("Repository Error"));

        var service = new ChatService(mockChatRepository.Object, mockMapper.Object);

        await Assert.ThrowsAsync<Exception>(() => service.DeleteConversation("prueba@yopmail.com", "ce55348c-61a1-4264-99c4-1d9d67974ab9", true));
    }
    #endregion

    #region Pruebas método UpdateFieldMessages
    [Fact]
    public async Task UpdateFieldMessagesConversationDoesNotExist()
    {
        var (mockChatRepository, mockMapper) = CreateMocks();
        var mockDocumentReference = new Mock<IDocumentReferenceWrapper>();
        var mockDocumentSnapshot = new Mock<IDocumentSnapshotWrapper>();

        mockDocumentSnapshot.Setup(s => s.Exists).Returns(false);

        mockDocumentReference.Setup(r => r.GetSnapshotAsync())
            .ReturnsAsync(mockDocumentSnapshot.Object);

        mockChatRepository.Setup(r => r.GetConversationByIdReference("prueba@yopmail.com", "ce55348c-61a1-4264-99c4-1d9d67974ab9"))
            .Returns(mockDocumentReference.Object);

        var service = new ChatService(mockChatRepository.Object, mockMapper.Object);

        var result = await service.UpdateFieldMessages("prueba@yopmail.com", "ce55348c-61a1-4264-99c4-1d9d67974ab9", new MessageChatBotDto());

        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs204, result.MessageHttp);
        Assert.Equal(Constants.CONV_NO_EXIST, result.Data);
    }

    [Fact]
    public async Task UpdateFieldMessagesConversationHasNoMessages()
    {
        var (mockChatRepository, mockMapper) = CreateMocks();
        var mockDocumentReference = new Mock<IDocumentReferenceWrapper>();
        var mockDocumentSnapshot = new Mock<IDocumentSnapshotWrapper>();

        var conversationDto = new ConversationDto
        {
            UuidConversation = "ce55348c-61a1-4264-99c4-1d9d67974ab9",
            Name = "Primera Conversación",
            Date = DateTime.UtcNow,
            Estado = true
        };

        var conversation = new Conversation
        {
            UuidConversation = "ce55348c-61a1-4264-99c4-1d9d67974ab9",
            Name = "Primera Conversación",
            Date = DateTime.UtcNow,
            Estado = true
        };

        var existConversationDto = new ConversationDto
        {
            UuidConversation = "ce55348c-61a1-4264-99c4-1d9d67974ab9",
            Name = "Primera Conversación",
            Date = DateTime.UtcNow,
            Estado = true
        };

        mockChatRepository.Setup(r => r.GetConversationByIdReference("prueba@yopmail.com", "ce55348c-61a1-4264-99c4-1d9d67974ab9"))
            .Returns(mockDocumentReference.Object);

        mockDocumentSnapshot.Setup(s => s.Exists).Returns(true);
        mockDocumentSnapshot.Setup(s => s.ConvertTo<Conversation>())
            .Returns(conversation);

        mockMapper.Setup(m => m.Map<ConversationDto>(conversation))
        .Returns(existConversationDto);

        mockDocumentReference.Setup(r => r.GetSnapshotAsync())
            .ReturnsAsync(mockDocumentSnapshot.Object);


        var service = new ChatService(mockChatRepository.Object, mockMapper.Object);

        var result = await service.UpdateFieldMessages("prueba@yopmail.com", "ce55348c-61a1-4264-99c4-1d9d67974ab9", new MessageChatBotDto());

        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs204, result.MessageHttp);
        Assert.Equal(Constants.MSJ_NO_EXIST, result.Data);
    }

    [Fact]
    public async Task UpdateFieldMessagesMessageNotFound()
    {
        var (mockChatRepository, mockMapper) = CreateMocks();
        var mockDocumentReference = new Mock<IDocumentReferenceWrapper>();
        var mockDocumentSnapshot = new Mock<IDocumentSnapshotWrapper>();

        var conversationDto = new ConversationDto
        {
            UuidConversation = "ce55348c-61a1-4264-99c4-1d9d67974ab9",
            Name = "Primera Conversación",
            Date = DateTime.UtcNow,
            Estado = true,
            Messages = new List<MessageChatBotDto> { new MessageChatBotDto { Uuid = "rt55348c-61a1-4264-99c4-1d9d67974ab9" } }
        };

        var conversation = new Conversation
        {
            UuidConversation = "ce55348c-61a1-4264-99c4-1d9d67974ab9",
            Name = "Primera Conversación",
            Date = DateTime.UtcNow,
            Estado = true,
            Messages = new List<MessageChatBot> { new MessageChatBot { Uuid = "rt55348c-61a1-4264-99c4-1d9d67974ab9" } }
        };

        var messagesDto = new MessageChatBotDto { Uuid = "yt55348c-61a1-4264-99c4-1d9d67974ab9" };

        mockChatRepository.Setup(r => r.GetConversationByIdReference("prueba@yopmail.com","ce55348c-61a1-4264-99c4-1d9d67974ab9"))
            .Returns(mockDocumentReference.Object);

        mockDocumentSnapshot.Setup(s => s.Exists).Returns(true);
        mockDocumentSnapshot.Setup(s => s.ConvertTo<Conversation>()).Returns(conversation);
        mockDocumentReference.Setup(r => r.GetSnapshotAsync()).ReturnsAsync(mockDocumentSnapshot.Object);
        mockMapper.Setup(m => m.Map<List<MessageChatBotDto>>(conversation.Messages)).Returns(new List<MessageChatBotDto> { new MessageChatBotDto { Uuid = "je55348c-61a1-4264-99c4-1d9d67974ab9" } });

        var service = new ChatService(mockChatRepository.Object, mockMapper.Object);

        // Act
        var result = await service.UpdateFieldMessages("prueba@yopmail.com", "ce55348c-61a1-4264-99c4-1d9d67974ab9", messagesDto);

        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs204, result.MessageHttp);
        Assert.Equal(Constants.MSJ_NO_EXIST_UPDATE, result.Data);
    }

    [Fact]
    public async Task UpdateFieldMessagesMessageFound()
    {
        var (mockChatRepository, mockMapper) = CreateMocks();
        var mockDocumentReference = new Mock<IDocumentReferenceWrapper>();
        var mockDocumentSnapshot = new Mock<IDocumentSnapshotWrapper>();

        var conversationDto = new ConversationDto
        {
            UuidConversation = "ce55348c-61a1-4264-99c4-1d9d67974ab9",
            Name = "Primera Conversación",
            Date = DateTime.UtcNow,
            Estado = true,
            Messages = new List<MessageChatBotDto> { new MessageChatBotDto { Uuid = "rt55348c-61a1-4264-99c4-1d9d67974ab9", IsFavorite = false } }
        };

        var conversation = new Conversation
        {
            UuidConversation = "ce55348c-61a1-4264-99c4-1d9d67974ab9",
            Name = "Primera Conversación",
            Date = DateTime.UtcNow,
            Estado = true,
            Messages = new List<MessageChatBot> { new MessageChatBot { Uuid = "rt55348c-61a1-4264-99c4-1d9d67974ab9", IsFavorite= false } }
        };

        var messagesDto = new MessageChatBotDto { Uuid = "rt55348c-61a1-4264-99c4-1d9d67974ab9", IsFavorite = true };
        var messageToUpdateDto = new MessageChatBotDto { Uuid = "rt55348c-61a1-4264-99c4-1d9d67974ab9", IsFavorite = false};

        mockChatRepository.Setup(r => r.GetConversationByIdReference("prueba@yopmail.com", "ce55348c-61a1-4264-99c4-1d9d67974ab9"))
            .Returns(mockDocumentReference.Object);

        mockDocumentSnapshot.Setup(s => s.Exists).Returns(true);
        mockDocumentSnapshot.Setup(s => s.ConvertTo<Conversation>()).Returns(conversation);
        mockDocumentReference.Setup(r => r.GetSnapshotAsync()).ReturnsAsync(mockDocumentSnapshot.Object);

        mockMapper.Setup(m => m.Map<List<MessageChatBotDto>>(conversation.Messages)).Returns(new List<MessageChatBotDto> { messageToUpdateDto });
        mockChatRepository.Setup(r => r.UpdateFieldMessages(mockDocumentReference.Object, It.IsAny<List<MessageChatBotDto>>())).ReturnsAsync(true);

        var service = new ChatService(mockChatRepository.Object, mockMapper.Object);

        var result = await service.UpdateFieldMessages("prueba@yopmail.com", "ce55348c-61a1-4264-99c4-1d9d67974ab9", messagesDto);

        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs200, result.MessageHttp);
        Assert.Equal(messagesDto.Message, messageToUpdateDto.Message);
    }

    [Fact]
    public async Task UpdateFieldMessagesThrowsException()
    {
        var (mockChatRepository, mockMapper) = CreateMocks();
        var mockDocumentReference = new Mock<IDocumentReferenceWrapper>();
        var mockDocumentSnapshot = new Mock<IDocumentSnapshotWrapper>();

        mockChatRepository.Setup(r => r.GetConversationByIdReference("prueba@yopmail.com", "ce55348c-61a1-4264-99c4-1d9d67974ab9"))
            .Returns(mockDocumentReference.Object);

        mockDocumentReference.Setup(r => r.GetSnapshotAsync())
            .ThrowsAsync(new Exception("Repository Error"));

        var service = new ChatService(mockChatRepository.Object, mockMapper.Object);

        await Assert.ThrowsAsync<Exception>(() => service.UpdateFieldMessages("prueba@yopmail.com", "ce55348c-61a1-4264-99c4-1d9d67974ab9", new MessageChatBotDto()));
    }
    #endregion
}





