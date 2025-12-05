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
using Commun.Logger;
using DomainLayer.Models;
using ServiceLayer.IService;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;
using MailKit.Security;



public class SendEmailTest
{
    private (Mock<IChatService>, Mock<IMapper>, Mock<IConfiguration>, Mock<ICreateLogger>, Mock<IFileWrapper>) CreateMocks()
    {
        return (new Mock<IChatService>(), new Mock<IMapper>(), new Mock<IConfiguration>(), new Mock<ICreateLogger>(), new Mock<IFileWrapper>());
    }

    #region Pruebas método SendEmail()
    [Fact]
    public async Task SendEmailWhenConversationExists()
    {
        var (mockChatService, mockMapper, mockConfig, mockCreateLogger, mockFileWrapper) = CreateMocks();

        var conversationDto = new ConversationDto
        {
            Date = DateTime.UtcNow,
            Messages = new List<MessageChatBotDto> { new MessageChatBotDto { IdPersona = "prueba@yopmail.com", Message = "test" } },
            Estado = true
        };
        var resultConversation = new Result { Data = conversationDto };

        mockChatService.Setup(s => s.GetConversationById("prueba@yopmail.com", "conversationId"))
            .ReturnsAsync(resultConversation);

        mockMapper.Setup(m => m.Map<ConversationDto>(conversationDto))
            .Returns(conversationDto);

        mockConfig.Setup(c => c.GetSection("Email:Host").Value).Returns("smtp.gmail.com");
        mockConfig.Setup(c => c.GetSection("Email:Port").Value).Returns("587");
        mockConfig.Setup(c => c.GetSection("Email:UserName").Value).Returns("katary.pruebas@gmail.com");
        mockConfig.Setup(c => c.GetSection("Email:PassWord").Value).Returns("ftnvbpmglwveayml");

        mockFileWrapper.Setup(f => f.ReadAllText(It.IsAny<string>())).Returns("<html><body>{{messages}}</body></html>");

        var service = new EmailService(mockChatService.Object,mockConfig.Object, mockCreateLogger.Object, mockMapper.Object, mockFileWrapper.Object);

        var result = await service.SendEmail("prueba@yopmail.com", "conversationId");

        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs200, result.MessageHttp);
    }

    [Fact]
    public async Task SendEmailWhenConversationIsNull()
    {
        var (mockChatService, mockMapper, mockConfig, mockCreateLogger, mockFileWrapper) = CreateMocks();

        mockChatService.Setup(s => s.GetConversationById("prueba@yopmail.com", "123"))
            .ReturnsAsync(new Result { Data = null });

        var service = new EmailService(mockChatService.Object, mockConfig.Object, mockCreateLogger.Object, mockMapper.Object, mockFileWrapper.Object);

        var result = await service.SendEmail("prueba@yopmail.com", "123");

        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs200, result.MessageHttp);
        Assert.Equal(Constants.CONV_NO_EXIST, result.Data);
    }
    [Fact]
    public async Task SendEmailFileReadException()
    {
        var (mockChatService, mockMapper, mockConfig, mockCreateLogger, mockFileWrapper) = CreateMocks();

        mockChatService.Setup(s => s.GetConversationById(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new Result { Data = new ConversationDto() });

        mockMapper.Setup(m => m.Map<ConversationDto>(It.IsAny<ConversationDto>()))
            .Returns(new ConversationDto());

        mockFileWrapper.Setup(f => f.ReadAllText(It.IsAny<string>()))
            .Throws(new FileNotFoundException());

        var service = new EmailService(mockChatService.Object, mockConfig.Object, mockCreateLogger.Object, mockMapper.Object, mockFileWrapper.Object);

        var result = await service.SendEmail("userId", "conversationId");

        mockCreateLogger.Verify(l => l.LogWriteExcepcion(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task SendEmailSmtpException()
    {
        var (mockChatService, mockMapper, mockConfig, mockCreateLogger, mockFileWrapper) = CreateMocks();
        var mockSmtpClient = new Mock<SmtpClient>();

        mockChatService.Setup(s => s.GetConversationById(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new Result { Data = new ConversationDto() });

        mockMapper.Setup(m => m.Map<ConversationDto>(It.IsAny<ConversationDto>()))
            .Returns(new ConversationDto());

        mockFileWrapper.Setup(f => f.ReadAllText(It.IsAny<string>()))
            .Returns("<html><body>{{messages}}</body></html>");

        mockConfig.Setup(c => c.GetSection("Email:Host").Value).Returns("smtp.gmail.com");
        mockConfig.Setup(c => c.GetSection("Email:Port").Value).Returns("587");
        mockConfig.Setup(c => c.GetSection("Email:UserName").Value).Returns("katary.pruebas@gmail.com");
        mockConfig.Setup(c => c.GetSection("Email:PassWord").Value).Returns("ftnvbpmglwveayml");

        mockSmtpClient.Setup(s => s.ConnectAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<SecureSocketOptions>(), It.IsAny<System.Threading.CancellationToken>()))
            .ThrowsAsync(new SmtpException());

        var service = new EmailService(mockChatService.Object, mockConfig.Object, mockCreateLogger.Object, mockMapper.Object, mockFileWrapper.Object);

        var result = await service.SendEmail("userId", "conversationId");

        mockCreateLogger.Verify(l => l.LogWriteExcepcion(It.IsAny<string>()), Times.Once);
    }

    #endregion

}


