using Microsoft.AspNetCore.Mvc;
using VehicleRentalSystem;
using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Services;

namespace API_SistemaLocacao.Controllers;

[ApiController]
[Route("[controller]")]
public class RentalController : ControllerBase
{
    private IRentalService _service;

    public RentalController(IRentalService service)
    {
        _service = service;
    }

    [HttpGet(Name = "GetAllRentals")]
    public List<RentalResponseDTO> Get()
    {
        return _service.GetRentals();
    }

    [HttpGet("{id:guid}")]
    public RentalResponseDTO GetById(Guid id)
    {
        return _service.GetRentalById(id);
    }
}
