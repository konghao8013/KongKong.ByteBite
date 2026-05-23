using ByteBite.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ByteBite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerStoreController : ControllerBase
{
    private readonly CustomerStoreService _customerStoreService;

    public CustomerStoreController(CustomerStoreService customerStoreService) { _customerStoreService = customerStoreService; }

    [HttpGet("{storeId:guid}/menu")]
    public async Task<object> GetStoreMenu(Guid storeId) => await _customerStoreService.GetStoreMenuAsync(storeId);
}