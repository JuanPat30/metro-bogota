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



public class ReportTest
{
    private (Mock<IChatRepository>, Mock<IMapper>, Mock<ICreateLogger>, Mock<ReportExcel>, Mock<IReportService>, Mock<IFileWrapper>, Mock<PDFGenerator>) CreateMocks()
    {
        return (new Mock<IChatRepository>(), new Mock<IMapper>(), new Mock<ICreateLogger>(), new Mock<ReportExcel>(), new Mock<IReportService>(), new Mock<IFileWrapper>(), new Mock<PDFGenerator>());
    }

    #region Pruebas método GetReportExcel()
    [Fact]
    public void GetReportExcelGenerated()
    {
        var (mockChatRespository, mockMapper, mockCreateLogger, mockReportExcel, mockReportService, mockFileWrapper, mockPdfGenerator) = CreateMocks();

        ConversationsUserDto conv = new ConversationsUserDto
        {

            ConversationId = "889b16a9-ac1b-4200-8442-f9626e5b4eda",
            UserName = "adriana.correa@metrodebogota.gov.co",
            Name = "Nueva consulta",
            Date = "19/03/2025 11:36 AM",
            Estado = "Activo"
        };
        ConversationsUserDto conv2 = new ConversationsUserDto
        {

            ConversationId = "e5560112-05c6-4961-8687-f46f74ae5c6b",
            UserName = "adriana.correa@metrodebogota.gov.co",
            Name = "Nueva consulta",
            Date = "19/03/2025 11:28 AM",
            Estado = "Activo"
        };

        var reportDto = new PaginatedResponseDto<ConversationsUserDto>
        {
            Items = new List<ConversationsUserDto> { conv, conv2 },
            Page = 1,
            PageSize = 10,
            TotalItems = 7,
            TotalPages = 1
        };

        string base64Excel = "base64EncodedExcelData";
        mockReportExcel.Setup(x => x.GenereteExcel(reportDto.Items)).Returns(base64Excel);

        mockFileWrapper.Setup(f => f.Delete(It.IsAny<string>())).Verifiable();

        var service = new ReportService(mockChatRespository.Object, mockMapper.Object, mockCreateLogger.Object, mockFileWrapper.Object, mockReportExcel.Object, mockPdfGenerator.Object);

        var result = service.GetReportExcel(reportDto);

        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs200, result.MessageHttp);
        Assert.Equal(base64Excel.Trim(), result.Data.ToString().Trim());

        mockFileWrapper.Verify(f => f.Delete(It.IsAny<string>()), Times.Once);
    }
    [Fact]
    public void GetReportExcelEmptyList()
    {
        var (mockChatRespository, mockMapper, mockCreateLogger, mockReportExcel, mockReportService, mockFileWrapper, mockPdfGenerator) = CreateMocks();


        var reportDto = new PaginatedResponseDto<ConversationsUserDto>
        {
            Items = new List<ConversationsUserDto>(),
            Page = 1,
            PageSize = 10,
            TotalItems = 7,
            TotalPages = 1
        };

        string base64Excel = "base64EncodedExcelData";
        mockReportExcel.Setup(x => x.GenereteExcel(reportDto.Items)).Returns(base64Excel);

        mockFileWrapper.Setup(f => f.Delete(It.IsAny<string>())).Verifiable();

        var service = new ReportService(mockChatRespository.Object, mockMapper.Object, mockCreateLogger.Object, mockFileWrapper.Object, mockReportExcel.Object, mockPdfGenerator.Object);

        var result = service.GetReportExcel(reportDto);

        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs200, result.MessageHttp);
        Assert.Equal(base64Excel.Trim(), result.Data.ToString().Trim());

        mockFileWrapper.Verify(f => f.Delete(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void GetReportExcelException()
    {
        var (mockChatRespository, mockMapper, mockCreateLogger, mockReportExcel, mockReportService, mockFileWrapper, mockPdfGenerator) = CreateMocks();
        ConversationsUserDto conv = new ConversationsUserDto
        {

            ConversationId = "889b16a9-ac1b-4200-8442-f9626e5b4eda",
            UserName = "adriana.correa@metrodebogota.gov.co",
            Name = "Nueva consulta",
            Date = "19/03/2025 11:36 AM",
            Estado = "Activo"
        };
        ConversationsUserDto conv2 = new ConversationsUserDto
        {

            ConversationId = "e5560112-05c6-4961-8687-f46f74ae5c6b",
            UserName = "adriana.correa@metrodebogota.gov.co",
            Name = "Nueva consulta",
            Date = "19/03/2025 11:28 AM",
            Estado = "Activo"
        };

        var reportDto = new PaginatedResponseDto<ConversationsUserDto>
        {
            Items = new List<ConversationsUserDto> { conv, conv2 },
            Page = 1,
            PageSize = 10,
            TotalItems = 7,
            TotalPages = 1
        };

        mockReportExcel.Setup(x => x.GenereteExcel(reportDto.Items)).Throws(new Exception("Error al generar Excel"));

        var service = new ReportService(mockChatRespository.Object, mockMapper.Object, mockCreateLogger.Object, mockFileWrapper.Object, mockReportExcel.Object, mockPdfGenerator.Object);

        var exception = Assert.Throws<Exception>(() => service.GetReportExcel(reportDto));
        Assert.Equal("Error al generar Excel", exception.Message);

        mockCreateLogger.Verify(x => x.LogWriteExcepcion("Error al generar Excel"), Times.Once);
    }

    [Fact]
    public void GetReportExcelDeleteFileException()
    {
        var (mockChatRespository, mockMapper, mockCreateLogger, mockReportExcel, mockReportService, mockFileWrapper, mockPdfGenerator) = CreateMocks();

        ConversationsUserDto conv = new ConversationsUserDto
        {

            ConversationId = "889b16a9-ac1b-4200-8442-f9626e5b4eda",
            UserName = "adriana.correa@metrodebogota.gov.co",
            Name = "Nueva consulta",
            Date = "19/03/2025 11:36 AM",
            Estado = "Activo"
        };
        ConversationsUserDto conv2 = new ConversationsUserDto
        {

            ConversationId = "e5560112-05c6-4961-8687-f46f74ae5c6b",
            UserName = "adriana.correa@metrodebogota.gov.co",
            Name = "Nueva consulta",
            Date = "19/03/2025 11:28 AM",
            Estado = "Activo"
        };

        var reportDto = new PaginatedResponseDto<ConversationsUserDto>
        {
            Items = new List<ConversationsUserDto> { conv, conv2 },
            Page = 1,
            PageSize = 10,
            TotalItems = 7,
            TotalPages = 1
        };

        string base64Excel = "base64EncodedExcelData";
        mockReportExcel.Setup(x => x.GenereteExcel(reportDto.Items)).Returns(base64Excel);

        mockFileWrapper.Setup(f => f.Delete(It.IsAny<string>())).Throws(new Exception("Error al eliminar el archivo"));

        var service = new ReportService(mockChatRespository.Object, mockMapper.Object, mockCreateLogger.Object, mockFileWrapper.Object, mockReportExcel.Object, mockPdfGenerator.Object);

        var exception = Assert.Throws<Exception>(() => service.GetReportExcel(reportDto));
        Assert.Equal("Error al eliminar el archivo", exception.Message);

        mockCreateLogger.Verify(x => x.LogWriteExcepcion("Error al eliminar el archivo"), Times.Once);
    }
    #endregion

    #region Pruebas método GeneratePdf()
    [Fact]
    public async Task GeneratePdfIsGenerated()
    {
        var (mockChatRespository, mockMapper, mockCreateLogger, mockReportExcel, mockReportService, mockFileWrapper, mockPdfGenerator) = CreateMocks();

        var conversationDto = new ConversationDto
        {
            UuidConversation = "ce55348c-61a1-4264-99c4-1d9d67974ab9",
            Name = "Primera Conversación",
            Date = DateTime.UtcNow,
            Estado = true,
            Messages = new List<MessageChatBotDto> { new MessageChatBotDto { Fecha = DateTime.UtcNow } }
        };

        mockChatRespository.Setup(r => r.GetConversationById("prueba@yopmail.com", "ce55348c-61a1-4264-99c4-1d9d67974ab9"))
            .ReturnsAsync(conversationDto);

        string base64Pdf = "base64EncodedPdfData";
        mockPdfGenerator.Setup(g => g.GeneratePdf(conversationDto, "prueba@yopmail.com")).Returns(base64Pdf);

        mockFileWrapper.Setup(f => f.Delete(It.IsAny<string>())).Verifiable();

        var service = new ReportService(mockChatRespository.Object, mockMapper.Object, mockCreateLogger.Object, mockFileWrapper.Object, mockReportExcel.Object, mockPdfGenerator.Object);

        var result = await service.GeneratePdf("prueba@yopmail.com", "ce55348c-61a1-4264-99c4-1d9d67974ab9");

        Assert.True(result.Success);
        Assert.Equal("200 - Ok", result.MessageHttp);
        Assert.Equal(base64Pdf, result.Data);

        mockFileWrapper.Verify(f => f.Delete(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GeneratePdfWithConversationNull()
    {
        var (mockChatRespository, mockMapper, mockCreateLogger, mockReportExcel, mockReportService, mockFileWrapper, mockPdfGenerator) = CreateMocks();

        mockChatRespository.Setup(r => r.GetConversationById("prueba@yopmail.com", "ce55348c-61a1-4264-99c4-1d9d67974ab9"))
            .ReturnsAsync((ConversationDto)null);

        var service = new ReportService(mockChatRespository.Object, mockMapper.Object, mockCreateLogger.Object, mockFileWrapper.Object, mockReportExcel.Object, mockPdfGenerator.Object);

        var result = await service.GeneratePdf("prueba@yopmail.com", "ce55348c-61a1-4264-99c4-1d9d67974ab9");

        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs204, result.MessageHttp);
        Assert.Equal(Constants.CONV_NO_EXIST, result.Data);
    }

    [Fact]
    public async Task GeneratePdf_ShouldHandleException_WhenPdfGenerationFails()
    {
        var (mockChatRespository, mockMapper, mockCreateLogger, mockReportExcel, mockReportService, mockFileWrapper, mockPdfGenerator) = CreateMocks();

        var conversationDto = new ConversationDto
        {
            UuidConversation = "ce55348c-61a1-4264-99c4-1d9d67974ab9",
            Name = "Primera Conversación",
            Date = DateTime.UtcNow,
            Estado = true,
            Messages = new List<MessageChatBotDto> { new MessageChatBotDto { Fecha = DateTime.UtcNow } }
        };

        mockChatRespository.Setup(r => r.GetConversationById("prueba@yopmail.com", "ce55348c-61a1-4264-99c4-1d9d67974ab9"))
            .ReturnsAsync(conversationDto);

        mockPdfGenerator.Setup(g => g.GeneratePdf(conversationDto, "prueba@yopmail.com"))
            .Throws(new Exception("Error al generar PDF"));

        var service = new ReportService(mockChatRespository.Object, mockMapper.Object, mockCreateLogger.Object, mockFileWrapper.Object, mockReportExcel.Object, mockPdfGenerator.Object);

        var exception = await Assert.ThrowsAsync<Exception>(() => service.GeneratePdf("prueba@yopmail.com", "ce55348c-61a1-4264-99c4-1d9d67974ab9"));
        Assert.Equal("Error al generar PDF", exception.Message);

        mockCreateLogger.Verify(l => l.LogWriteExcepcion("Error al generar PDF"), Times.Once);
    }

    [Fact]
    public async Task GeneratePdfFileDeleteException()
    {
        var (mockChatRespository, mockMapper, mockCreateLogger, mockReportExcel, mockReportService, mockFileWrapper, mockPdfGenerator) = CreateMocks();

        var conversationDto = new ConversationDto
        {
            UuidConversation = "ce55348c-61a1-4264-99c4-1d9d67974ab9",
            Name = "Primera Conversación",
            Date = DateTime.UtcNow,
            Estado = true,
            Messages = new List<MessageChatBotDto> { new MessageChatBotDto { Fecha = DateTime.UtcNow } }
        };

        mockChatRespository.Setup(r => r.GetConversationById("prueba@yopmail.com", "ce55348c-61a1-4264-99c4-1d9d67974ab9"))
             .ReturnsAsync(conversationDto);

        string base64Pdf = "base64EncodedPdfData";
        mockPdfGenerator.Setup(g => g.GeneratePdf(conversationDto, "prueba@yopmail.com")).Returns(base64Pdf);

        mockFileWrapper.Setup(f => f.Delete(It.IsAny<string>()))
            .Throws(new Exception("Error al eliminar el archivo"));

        var service = new ReportService(mockChatRespository.Object, mockMapper.Object, mockCreateLogger.Object, mockFileWrapper.Object, mockReportExcel.Object, mockPdfGenerator.Object);

        var exception = await Assert.ThrowsAsync<Exception>(() => service.GeneratePdf("prueba@yopmail.com", "ce55348c-61a1-4264-99c4-1d9d67974ab9"));
        Assert.Equal("Error al eliminar el archivo", exception.Message);

        mockCreateLogger.Verify(l => l.LogWriteExcepcion("Error al eliminar el archivo"), Times.Once);
    }
    #endregion
}





