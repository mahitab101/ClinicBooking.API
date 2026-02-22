using ClinicBooking.API.Contracts;
using ClinicBooking.API.Dtos.Specializations;
using ClinicBooking.API.Entities;
using ClinicBooking.API.Mappings;
using Microsoft.AspNetCore.Mvc;

namespace ClinicBooking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpecializationsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public SpecializationsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var specializations = await _unitOfWork.Specializations.GetAllAsync();
            var result = specializations.Select(s => s.ToDto()).ToList();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(Guid id)
        {
            var specialization = await _unitOfWork.Specializations.GetByIdAsync(id);
            if (specialization == null) return NotFound();

            return Ok(specialization.ToDto());
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSpecializationDto specializationDto)
        {
            var Specialization = specializationDto.ToEntity();
            await _unitOfWork.Specializations.AddAsync(Specialization);
            await _unitOfWork.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = Specialization.Id },
                Specialization.ToDto());
        }
    }
}
