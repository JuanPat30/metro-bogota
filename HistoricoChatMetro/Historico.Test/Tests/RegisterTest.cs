using Moq;
using ServiceLayer.Service;
using RepositoryLayer.IRepository;
using Commun;
using Google.Cloud.Firestore;
using DomainLayer.Dtos;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using System.Globalization;
using iText.Kernel.Geom;



public class RegisterTest
{
    #region Pruebas método GetUsers()
    [Fact]
    public async Task GetUsersList()
    {
        var mockRepository = new Mock<IRegisterRepository>();
        var expectedUsers = new List<string> { "user1", "user2", "user3" };
        mockRepository.Setup(repo => repo.GetUsers()).ReturnsAsync(expectedUsers);

        var service = new RegisterService(mockRepository.Object);

        var result = await service.GetUsers();

        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs200, result.MessageHttp);
        Assert.Equal(expectedUsers, result.Data);
    }

    [Fact]
    public async Task GetUsersException()
    {
        var mockRepository = new Mock<IRegisterRepository>();
        mockRepository.Setup(repo => repo.GetUsers()).ThrowsAsync(new System.Exception("Repository error"));

        var service = new RegisterService(mockRepository.Object);

        await Assert.ThrowsAsync<Exception>(async() => await service.GetUsers());
    }

    [Fact]
    public async Task GetUsersReturnsEmptyList()
    {
        var mockRepository = new Mock<IRegisterRepository>();
        mockRepository.Setup(repo => repo.GetUsers()).ReturnsAsync(new List<string>());
        var service = new RegisterService(mockRepository.Object);

        var result = await service.GetUsers();

        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs200, result.MessageHttp);
        Assert.Empty((List<string>)result.Data);
    }

    [Fact]
    public async Task GetUsersReturnsNull()
    {
        var mockRepository = new Mock<IRegisterRepository>();
        mockRepository.Setup(repo => repo.GetUsers()).ReturnsAsync((List<string>)null);
        var service = new RegisterService(mockRepository.Object);

        var result = await service.GetUsers();

        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs200, result.MessageHttp);
        Assert.Null(result.Data);
    }
    
    [Fact]
    public async Task GetUsersCallableMultiple()
    {
        var mockRepository = new Mock<IRegisterRepository>();
        var expectedUsers = new List<string> { "user1", "user2", "user3" };
        mockRepository.Setup(repo => repo.GetUsers()).ReturnsAsync(expectedUsers);
        var service = new RegisterService(mockRepository.Object);

        var result = await service.GetUsers();
        var result2 = await service.GetUsers();

        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs200, result.MessageHttp);
        Assert.Equal(expectedUsers, result.Data);

        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs200, result.MessageHttp);
        Assert.Equal(expectedUsers, result2.Data);
    }
    #endregion

    #region Pruebas método GetAll()
    [Fact]
    public async Task GetAllWithPaginated()
    {
        var mockRepository = new Mock<IRegisterRepository>();
        var conversations = new List<ConversationsUserDto> 
        { 
            new ConversationsUserDto { ConversationId = "3b4fc952-e566-4c49-b605-6984a1e5d6ee", Name = "Primera prueba", UserName ="adriana.correa@metrodebogota.gov.co", Date = "19/03/2025 11:36 AM", Estado ="Activo"},
            new ConversationsUserDto { ConversationId = "4b4fc952-e566-5c49-b605-6984a1e5d6ef", Name = "Segunda prueba", UserName ="adriana.correa@metrodebogota.gov.co", Date = "29/03/2025 07:40 AM", Estado ="Inactivo"},
            new ConversationsUserDto { ConversationId = "5b4fc952-e566-6c49-b605-6984a1e5d6eg", Name = "Tercera prueba", UserName ="jaime.perez@metrodebogota.gov.co", Date = "30/03/2025 08:57 AM", Estado ="Activo"},
        };
        mockRepository.Setup(repo => repo.GetAllConversations(null,null,null,null,null)).ReturnsAsync(conversations);

        var service = new RegisterService(mockRepository.Object);

        var result = await service.GetAll(1,2);

        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs200, result.MessageHttp);

        var paginated = (PaginatedResponseDto<ConversationsUserDto>)result.Data;

        Assert.NotNull(paginated);
        Assert.Equal(1, paginated.Page);
        Assert.Equal(2, paginated.PageSize);
        Assert.Equal(conversations.Count, paginated.TotalItems);
        Assert.Equal((int)Math.Ceiling((double)conversations.Count / 2), paginated.TotalPages);
        Assert.Equal(conversations.Take(2).ToList(), paginated.Items);
    }

    [Fact]
    public async Task GetAllWithPaginatedSpecified()
    {
        var mockRepository = new Mock<IRegisterRepository>();
        var conversations = new List<ConversationsUserDto>
        {
            new ConversationsUserDto { ConversationId = "3b4fc952-e566-4c49-b605-6984a1e5d6ee", Name = "Primera prueba", UserName ="adriana.correa@metrodebogota.gov.co", Date = "19/03/2025 11:36 AM", Estado ="Activo"},
            new ConversationsUserDto { ConversationId = "4b4fc952-e566-5c49-b605-6984a1e5d6ef", Name = "Segunda prueba", UserName ="adriana.correa@metrodebogota.gov.co", Date = "29/03/2025 07:40 AM", Estado ="Inactivo"},
            new ConversationsUserDto { ConversationId = "5b4fc952-e566-6c49-b605-6984a1e5d6eg", Name = "Tercera prueba", UserName ="jaime.perez@metrodebogota.gov.co", Date = "30/03/2025 08:57 AM", Estado ="Activo"},
            new ConversationsUserDto { ConversationId = "6b4fc952-e566-7c49-b605-6984a1e5d6eh", Name = "Cuarta prueba", UserName ="camila.perez@metrodebogota.gov.co", Date = "17/03/2025 13:22 AM", Estado ="Activo"},
            new ConversationsUserDto { ConversationId = "7b4fc952-e566-8c49-b605-6984a1e5d6ei", Name = "Quina prueba", UserName ="kevin.raul@metrodebogota.gov.co", Date = "09/03/2025 15:16 AM", Estado ="Activo"},
            new ConversationsUserDto { ConversationId = "8b4fc952-e566-9c49-b605-6984a1e5d6ej", Name = "Sexta prueba", UserName ="lina.lopez@metrodebogota.gov.co", Date = "10/03/2025 16:39 AM", Estado ="Activo"},
        };
        mockRepository.Setup(repo => repo.GetAllConversations(null, null, null, null, null)).ReturnsAsync(conversations);

        var service = new RegisterService(mockRepository.Object);

        int page = 2;
        int pageSize = 2;
        var result = await service.GetAll(page, pageSize);

        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs200, result.MessageHttp);

        var paginated = (PaginatedResponseDto<ConversationsUserDto>)result.Data;

        Assert.NotNull(paginated);
        Assert.Equal(page, paginated.Page);
        Assert.Equal(pageSize, paginated.PageSize);
        Assert.Equal(conversations.Count, paginated.TotalItems);
        Assert.Equal((int)Math.Ceiling((double)conversations.Count / pageSize), paginated.TotalPages);

        var expectedItems = conversations.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        Assert.Equal(expectedItems, paginated.Items);
    }

    [Fact]
    public async Task GetAllFilterByName()
    {
        var mockRepository = new Mock<IRegisterRepository>();
        var conversations = new List<ConversationsUserDto>
        {
            new ConversationsUserDto { ConversationId = "3b4fc952-e566-4c49-b605-6984a1e5d6ee", Name = "Primera prueba", UserName ="adriana.correa@metrodebogota.gov.co", Date = "19/03/2025 11:36 AM", Estado ="Activo"},
            new ConversationsUserDto { ConversationId = "4b4fc952-e566-5c49-b605-6984a1e5d6ef", Name = "Segunda prueba", UserName ="adriana.correa@metrodebogota.gov.co", Date = "29/03/2025 07:40 AM", Estado ="Inactivo"},
            new ConversationsUserDto { ConversationId = "5b4fc952-e566-6c49-b605-6984a1e5d6eg", Name = "Tercera prueba", UserName ="jaime.perez@metrodebogota.gov.co", Date = "30/03/2025 08:57 AM", Estado ="Activo"},
            new ConversationsUserDto { ConversationId = "6b4fc952-e566-7c49-b605-6984a1e5d6eh", Name = "Cuarta prueba", UserName ="camila.perez@metrodebogota.gov.co", Date = "17/03/2025 13:22 AM", Estado ="Activo"},
            new ConversationsUserDto { ConversationId = "7b4fc952-e566-8c49-b605-6984a1e5d6ei", Name = "Quina prueba", UserName ="kevin.raul@metrodebogota.gov.co", Date = "09/03/2025 15:16 AM", Estado ="Activo"},
            new ConversationsUserDto { ConversationId = "8b4fc952-e566-9c49-b605-6984a1e5d6ej", Name = "Sexta prueba", UserName ="lina.lopez@metrodebogota.gov.co", Date = "10/03/2025 16:39 AM", Estado ="Activo"},
        };

        var filterName = "lina.lopez@metrodebogota.gov.co";
        mockRepository.Setup(repo => repo.GetAllConversations(filterName, null, null, null, null)).ReturnsAsync(conversations.Where(c => c.UserName == filterName).ToList());

        var service = new RegisterService(mockRepository.Object);

        int page = 1;
        int pageSize = 10;
        var result = await service.GetAll(page, pageSize, name: "lina.lopez@metrodebogota.gov.co");

        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs200, result.MessageHttp);

        var paginated = (PaginatedResponseDto<ConversationsUserDto>)result.Data;

        Assert.NotNull(paginated);
        Assert.Single(paginated.Items);
        Assert.Equal(filterName, paginated.Items.FirstOrDefault().UserName);
        Assert.Equal(1, paginated.TotalItems);
        Assert.Equal(1, paginated.TotalPages);

    }

    [Fact]
    public async Task GetAllFilterByRangeDates()
    {
        var mockRepository = new Mock<IRegisterRepository>();
        var conversations = new List<ConversationsUserDto>
        {
            new ConversationsUserDto { ConversationId = "3b4fc952-e566-4c49-b605-6984a1e5d6ee", Name = "Primera prueba", UserName ="adriana.correa@metrodebogota.gov.co", Date = new DateTime(2025, 3, 19, 11, 36, 0).ToString(), Estado ="Activo"},
            new ConversationsUserDto { ConversationId = "4b4fc952-e566-5c49-b605-6984a1e5d6ef", Name = "Segunda prueba", UserName ="adriana.correa@metrodebogota.gov.co", Date = new DateTime(2025, 3, 29, 7, 40, 0).ToString(), Estado ="Inactivo"},
            new ConversationsUserDto { ConversationId = "5b4fc952-e566-6c49-b605-6984a1e5d6eg", Name = "Tercera prueba", UserName ="jaime.perez@metrodebogota.gov.co", Date = new DateTime(2025, 3, 30, 8, 57, 0).ToString(), Estado ="Activo"},
            new ConversationsUserDto { ConversationId = "6b4fc952-e566-7c49-b605-6984a1e5d6eh", Name = "Cuarta prueba", UserName ="camila.perez@metrodebogota.gov.co", Date = new DateTime(2025, 3, 17, 13, 22, 0).ToString(), Estado ="Activo"},
            new ConversationsUserDto { ConversationId = "7b4fc952-e566-8c49-b605-6984a1e5d6ei", Name = "Quina prueba", UserName ="kevin.raul@metrodebogota.gov.co", Date = new DateTime(2025, 3, 9, 15, 16, 0).ToString(), Estado ="Activo"},
            new ConversationsUserDto { ConversationId = "8b4fc952-e566-9c49-b605-6984a1e5d6ej", Name = "Sexta prueba", UserName ="lina.lopez@metrodebogota.gov.co", Date = new DateTime(2025, 3, 19, 16, 39, 0).ToString(), Estado ="Activo"}
        };

        var from = System.DateTime.Now.Date;
        var to = System.DateTime.Now.Date.AddDays(4);

        var filter = conversations.Where(c => DateTime.Parse(c.Date).Date >= from && DateTime.Parse(c.Date).Date <= to).ToList();
        
        mockRepository.Setup(repo => repo.GetAllConversations(null, from, to, null, null))
            .ReturnsAsync(filter);

        var service = new RegisterService(mockRepository.Object);

        int page = 1;
        int pageSize = 10;
        var result = await service.GetAll(page, pageSize, from: from, to: to);

        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs200, result.MessageHttp);

        var paginated = (PaginatedResponseDto<ConversationsUserDto>)result.Data;

        Assert.NotNull(paginated);
        Assert.Equal(filter, paginated.Items);
        Assert.Equal(filter.Count, paginated.TotalItems);
        Assert.Equal((int)Math.Ceiling((double)filter.Count / pageSize), paginated.TotalPages);

    }

    [Fact]
    public async Task GetAllFilterByStatus()
    {
        var mockRepository = new Mock<IRegisterRepository>();
        var conversations = new List<ConversationsUserDto>
        {
            new ConversationsUserDto { ConversationId = "3b4fc952-e566-4c49-b605-6984a1e5d6ee", Name = "Primera prueba", UserName ="adriana.correa@metrodebogota.gov.co", Date = "19/03/2025 11:36 AM", Estado ="Activo"},
            new ConversationsUserDto { ConversationId = "4b4fc952-e566-5c49-b605-6984a1e5d6ef", Name = "Segunda prueba", UserName ="adriana.correa@metrodebogota.gov.co", Date = "29/03/2025 07:40 AM", Estado ="Inactivo"},
            new ConversationsUserDto { ConversationId = "5b4fc952-e566-6c49-b605-6984a1e5d6eg", Name = "Tercera prueba", UserName ="jaime.perez@metrodebogota.gov.co", Date = "30/03/2025 08:57 AM", Estado ="Activo"},
            new ConversationsUserDto { ConversationId = "6b4fc952-e566-7c49-b605-6984a1e5d6eh", Name = "Cuarta prueba", UserName ="camila.perez@metrodebogota.gov.co", Date = "17/03/2025 13:22 AM", Estado ="Activo"},
            new ConversationsUserDto { ConversationId = "7b4fc952-e566-8c49-b605-6984a1e5d6ei", Name = "Quina prueba", UserName ="kevin.raul@metrodebogota.gov.co", Date = "09/03/2025 15:16 AM", Estado ="Activo"},
            new ConversationsUserDto { ConversationId = "8b4fc952-e566-9c49-b605-6984a1e5d6ej", Name = "Sexta prueba", UserName ="lina.lopez@metrodebogota.gov.co", Date = "10/03/2025 16:39 AM", Estado ="Activo"},
        };

        var filter = conversations.Where(c => c.Estado == "Inactivo").ToList();

        mockRepository.Setup(repo => repo.GetAllConversations(null, null, null, false, null))
            .ReturnsAsync(filter);

        var service = new RegisterService(mockRepository.Object);

        var result = await service.GetAll(1, 10, status: false);

        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs200, result.MessageHttp);

        var paginatedResponse = (PaginatedResponseDto<ConversationsUserDto>)result.Data;
        Assert.NotNull(paginatedResponse);
        Assert.Equal(filter, paginatedResponse.Items);
    }

    [Fact]
    public async Task GetAllFilterByIsDescending()
    {
        var mockRepository = new Mock<IRegisterRepository>();
        var conversations = new List<ConversationsUserDto>
        {
            new ConversationsUserDto { ConversationId = "3b4fc952-e566-4c49-b605-6984a1e5d6ee", Name = "Primera prueba", UserName ="adriana.correa@metrodebogota.gov.co", Date = new DateTime(2025, 3, 19, 11, 36, 0).ToString(), Estado ="Activo"},
            new ConversationsUserDto { ConversationId = "4b4fc952-e566-5c49-b605-6984a1e5d6ef", Name = "Segunda prueba", UserName ="adriana.correa@metrodebogota.gov.co", Date = new DateTime(2025, 3, 29, 7, 40, 0).ToString(), Estado ="Inactivo"},
            new ConversationsUserDto { ConversationId = "5b4fc952-e566-6c49-b605-6984a1e5d6eg", Name = "Tercera prueba", UserName ="jaime.perez@metrodebogota.gov.co", Date = new DateTime(2025, 3, 30, 8, 57, 0).ToString(), Estado ="Activo"},
            new ConversationsUserDto { ConversationId = "6b4fc952-e566-7c49-b605-6984a1e5d6eh", Name = "Cuarta prueba", UserName ="camila.perez@metrodebogota.gov.co", Date = new DateTime(2025, 3, 17, 13, 22, 0).ToString(), Estado ="Activo"},
            new ConversationsUserDto { ConversationId = "7b4fc952-e566-8c49-b605-6984a1e5d6ei", Name = "Quina prueba", UserName ="kevin.raul@metrodebogota.gov.co", Date = new DateTime(2025, 3, 9, 15, 16, 0).ToString(), Estado ="Activo"},
            new ConversationsUserDto { ConversationId = "8b4fc952-e566-9c49-b605-6984a1e5d6ej", Name = "Sexta prueba", UserName ="lina.lopez@metrodebogota.gov.co", Date = new DateTime(2025, 3, 19, 16, 39, 0).ToString(), Estado ="Activo"}
        };

        var filter = conversations.OrderByDescending(c => DateTime.Parse(c.Date)).ToList();

        mockRepository.Setup(repo => repo.GetAllConversations(null, null, null, null, true))
            .ReturnsAsync(filter);

        var service = new RegisterService(mockRepository.Object);

        var result = await service.GetAll(1, 10, isDescending: true);

        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs200, result.MessageHttp);

        var paginatedResponse = (PaginatedResponseDto<ConversationsUserDto>)result.Data;
        Assert.NotNull(paginatedResponse);
        Assert.Equal(filter, paginatedResponse.Items);
        Assert.Equal(filter.Count, paginatedResponse.TotalItems);
    }
    
    [Fact]
    public async Task GetAllFilterByAll()
    {
        var mockRepository = new Mock<IRegisterRepository>();
        var conversations = new List<ConversationsUserDto>
        {
            new ConversationsUserDto { ConversationId = "3b4fc952-e566-4c49-b605-6984a1e5d6ee", Name = "Primera prueba", UserName ="adriana.correa@metrodebogota.gov.co", Date = new DateTime(2025, 3, 19, 11, 36, 0).ToString(), Estado ="Activo"},
            new ConversationsUserDto { ConversationId = "4b4fc952-e566-5c49-b605-6984a1e5d6ef", Name = "Segunda prueba", UserName ="adriana.correa@metrodebogota.gov.co", Date = new DateTime(2025, 3, 29, 7, 40, 0).ToString(), Estado ="Inactivo"},
            new ConversationsUserDto { ConversationId = "5b4fc952-e566-6c49-b605-6984a1e5d6eg", Name = "Tercera prueba", UserName ="jaime.perez@metrodebogota.gov.co", Date = new DateTime(2025, 3, 30, 8, 57, 0).ToString(), Estado ="Activo"},
            new ConversationsUserDto { ConversationId = "6b4fc952-e566-7c49-b605-6984a1e5d6eh", Name = "Cuarta prueba", UserName ="camila.perez@metrodebogota.gov.co", Date = new DateTime(2025, 3, 17, 13, 22, 0).ToString(), Estado ="Activo"},
            new ConversationsUserDto { ConversationId = "7b4fc952-e566-8c49-b605-6984a1e5d6ei", Name = "Quina prueba", UserName ="kevin.raul@metrodebogota.gov.co", Date = new DateTime(2025, 3, 9, 15, 16, 0).ToString(), Estado ="Activo"},
            new ConversationsUserDto { ConversationId = "8b4fc952-e566-9c49-b605-6984a1e5d6ej", Name = "Sexta prueba", UserName ="lina.lopez@metrodebogota.gov.co", Date = new DateTime(2025, 3, 19, 16, 39, 0).ToString(), Estado ="Activo"}
        };

        string filterName = "adriana.correa@metrodebogota.gov.co";
        DateTime from = DateTime.Now.Date;
        DateTime to = DateTime.Now.Date.AddDays(4);
        bool filterStatus = true;
        bool isDescending = true;

        var filter = conversations.Where(c => c.UserName == filterName &&
                       DateTime.Parse(c.Date).Date >= from.Date &&
                       DateTime.Parse(c.Date).Date <= to.Date &&
                       c.Estado == (filterStatus ? "Activo" : "Inactivo"))
            .OrderByDescending(c => DateTime.Parse(c.Date)).ToList();

        mockRepository.Setup(repo => repo.GetAllConversations(filterName, from, to, filterStatus, isDescending))
            .ReturnsAsync(filter);

        var service = new RegisterService(mockRepository.Object);

        var result = await service.GetAll(1, 10, name: filterName, from: from, to: to, status: filterStatus, isDescending: isDescending);

        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs200, result.MessageHttp);

        var paginatedResponse = (PaginatedResponseDto<ConversationsUserDto>)result.Data;
        Assert.NotNull(paginatedResponse);
        Assert.Equal(filter, paginatedResponse.Items);
        Assert.Equal(filter.Count, paginatedResponse.TotalItems);
        Assert.Equal((int)Math.Ceiling((double)filter.Count / 10), paginatedResponse.TotalPages);

    }
    [Fact]
    public async Task GetAllEmpty()
    {
        var mockRepository = new Mock<IRegisterRepository>();
        mockRepository.Setup(repo => repo.GetAllConversations("User", null, null, null, null))
            .ReturnsAsync(new List<ConversationsUserDto>()); 

        var service = new RegisterService(mockRepository.Object);

        var result = await service.GetAll(1, 10, name: "User");

        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs200, result.MessageHttp);

        var paginatedResponse = (PaginatedResponseDto<ConversationsUserDto>)result.Data;
        Assert.NotNull(paginatedResponse);
        Assert.Empty(paginatedResponse.Items);
    }

    [Fact]
    public async Task GetAllPageOutOfRange()
    {
        var mockRepository = new Mock<IRegisterRepository>();
        var conversations = new List<ConversationsUserDto>
        {
            new ConversationsUserDto { ConversationId = "3b4fc952-e566-4c49-b605-6984a1e5d6ee", Name = "Primera prueba", UserName ="adriana.correa@metrodebogota.gov.co", Date = new DateTime(2025, 3, 19, 11, 36, 0).ToString(), Estado ="Activo"},
            new ConversationsUserDto { ConversationId = "4b4fc952-e566-5c49-b605-6984a1e5d6ef", Name = "Segunda prueba", UserName ="adriana.correa@metrodebogota.gov.co", Date = new DateTime(2025, 3, 29, 7, 40, 0).ToString(), Estado ="Inactivo"},
            new ConversationsUserDto { ConversationId = "5b4fc952-e566-6c49-b605-6984a1e5d6eg", Name = "Tercera prueba", UserName ="jaime.perez@metrodebogota.gov.co", Date = new DateTime(2025, 3, 30, 8, 57, 0).ToString(), Estado ="Activo"},
            new ConversationsUserDto { ConversationId = "6b4fc952-e566-7c49-b605-6984a1e5d6eh", Name = "Cuarta prueba", UserName ="camila.perez@metrodebogota.gov.co", Date = new DateTime(2025, 3, 17, 13, 22, 0).ToString(), Estado ="Activo"},
            new ConversationsUserDto { ConversationId = "7b4fc952-e566-8c49-b605-6984a1e5d6ei", Name = "Quina prueba", UserName ="kevin.raul@metrodebogota.gov.co", Date = new DateTime(2025, 3, 9, 15, 16, 0).ToString(), Estado ="Activo"},
            new ConversationsUserDto { ConversationId = "8b4fc952-e566-9c49-b605-6984a1e5d6ej", Name = "Sexta prueba", UserName ="lina.lopez@metrodebogota.gov.co", Date = new DateTime(2025, 3, 19, 16, 39, 0).ToString(), Estado ="Activo"}
        };



        mockRepository.Setup(repo => repo.GetAllConversations(null,null,null,null,null))
            .ReturnsAsync(conversations);

        var service = new RegisterService(mockRepository.Object);

        var result = await service.GetAll(100, 10);

        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs200, result.MessageHttp);

        var paginatedResponse = (PaginatedResponseDto<ConversationsUserDto>)result.Data;
        Assert.NotNull(paginatedResponse);
        Assert.Empty(paginatedResponse.Items);

    }

    [Fact]
    public async Task GetAllPageSizeZero()
    {
        var mockRepository = new Mock<IRegisterRepository>();
        var conversations = new List<ConversationsUserDto>
        {
            new ConversationsUserDto { ConversationId = "3b4fc952-e566-4c49-b605-6984a1e5d6ee", Name = "Primera prueba", UserName ="adriana.correa@metrodebogota.gov.co", Date = new DateTime(2025, 3, 19, 11, 36, 0).ToString(), Estado ="Activo"},
            new ConversationsUserDto { ConversationId = "4b4fc952-e566-5c49-b605-6984a1e5d6ef", Name = "Segunda prueba", UserName ="adriana.correa@metrodebogota.gov.co", Date = new DateTime(2025, 3, 29, 7, 40, 0).ToString(), Estado ="Inactivo"},
            new ConversationsUserDto { ConversationId = "5b4fc952-e566-6c49-b605-6984a1e5d6eg", Name = "Tercera prueba", UserName ="jaime.perez@metrodebogota.gov.co", Date = new DateTime(2025, 3, 30, 8, 57, 0).ToString(), Estado ="Activo"},
            new ConversationsUserDto { ConversationId = "6b4fc952-e566-7c49-b605-6984a1e5d6eh", Name = "Cuarta prueba", UserName ="camila.perez@metrodebogota.gov.co", Date = new DateTime(2025, 3, 17, 13, 22, 0).ToString(), Estado ="Activo"},
            new ConversationsUserDto { ConversationId = "7b4fc952-e566-8c49-b605-6984a1e5d6ei", Name = "Quina prueba", UserName ="kevin.raul@metrodebogota.gov.co", Date = new DateTime(2025, 3, 9, 15, 16, 0).ToString(), Estado ="Activo"},
            new ConversationsUserDto { ConversationId = "8b4fc952-e566-9c49-b605-6984a1e5d6ej", Name = "Sexta prueba", UserName ="lina.lopez@metrodebogota.gov.co", Date = new DateTime(2025, 3, 19, 16, 39, 0).ToString(), Estado ="Activo"}
        };



        mockRepository.Setup(repo => repo.GetAllConversations(null, null, null, null, null))
            .ReturnsAsync(conversations);

        var service = new RegisterService(mockRepository.Object);

        var result = await service.GetAll(1, 0);

        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs200, result.MessageHttp);

        var paginatedResponse = (PaginatedResponseDto<ConversationsUserDto>)result.Data;
        Assert.NotNull(paginatedResponse);
        Assert.Empty(paginatedResponse.Items);

    }
    [Fact]
    public async Task GetAllReturnsNull()
    {
        var mockRepository = new Mock<IRegisterRepository>();
        mockRepository.Setup(repo => repo.GetAllConversations(null, null, null, null, null)).ReturnsAsync((List<ConversationsUserDto>)null);
        var service = new RegisterService(mockRepository.Object);

        var result = await service.GetAll(1,10);

        Assert.True(result.Success);
        Assert.Equal(Constants.msjMs200, result.MessageHttp);
        Assert.Null(result.Data);
    }
    [Fact]
    public async Task GetAllThrowException()
    {

        var mockRepository = new Mock<IRegisterRepository>();
        mockRepository.Setup(repo => repo.GetAllConversations(null, null, null, null, null))
            .ThrowsAsync(new System.Exception("Repository error"));

        var service = new RegisterService(mockRepository.Object);

        await Assert.ThrowsAsync<System.Exception>(() => service.GetAll(1, 10));
    }
    #endregion
}


