using ClinicBooking.API.Common;
using ClinicBooking.API.Contracts;
using ClinicBooking.API.Dtos.Specializations;
using ClinicBooking.API.Mappings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicBooking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SpecializationsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public SpecializationsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> GetAll()
        {
            var specializations = await _unitOfWork.Specializations.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<object>>.Ok(specializations.Select(s => s.ToDto())));
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var specialization = await _unitOfWork.Specializations.GetByIdAsync(id);
            if (specialization == null)
                return NotFound(ApiResponse.FailNoData($"Specialization with id {id} not found."));

            return Ok(ApiResponse<object>.Ok(specialization.ToDto()));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateSpecializationDto specializationDto)
        {
            var specialization = specializationDto.ToEntity();
            await _unitOfWork.Specializations.AddAsync(specialization);
            await _unitOfWork.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = specialization.Id },
                ApiResponse<object>.Ok(specialization.ToDto(), "Specialization created successfully."));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update([FromBody] UpdateSpecializationDto specializationDto, Guid id)
        {
            var specialization = await _unitOfWork.Specializations.GetByIdAsync(id);
            if (specialization == null)
                return NotFound(ApiResponse.FailNoData($"Specialization with id {id} not found."));

            specialization.UpdateEntity(specializationDto);
            _unitOfWork.Specializations.Update(specialization);
            await _unitOfWork.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(specialization.ToDto(), "Specialization updated successfully."));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _unitOfWork.Specializations.Delete(id);
            if (!deleted)
                return NotFound(ApiResponse.FailNoData($"Specialization with id {id} not found."));

            await _unitOfWork.SaveChangesAsync();
            return Ok(ApiResponse.OkNoData("Specialization deleted successfully."));
        }
    }
}
