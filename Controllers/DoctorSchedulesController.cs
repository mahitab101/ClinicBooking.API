using ClinicBooking.API.Contracts;
using ClinicBooking.API.Dtos.DoctorSchedules;
using ClinicBooking.API.Mappings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ClinicBooking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorSchedulesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public DoctorSchedulesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var schedules = await _unitOfWork.DoctorSchedules.GetAllAsync();

            return Ok(schedules.Select(s => s.ToDto()));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var schedule = await _unitOfWork.DoctorSchedules.GetByIdAsync(id);

            if (schedule == null)
                return NotFound();

            return Ok(schedule.ToDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateDoctorScheduleDto dto)
        {
            var doctorExists = await _unitOfWork.Doctors
                .Query()
                .AnyAsync(d => d.Id == dto.DoctorId);

            if (!doctorExists)
                return BadRequest("Doctor not found");

            var schedule = dto.ToEntity();

            await _unitOfWork.DoctorSchedules.AddAsync(schedule);
            await _unitOfWork.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = schedule.Id }, schedule.ToDto());
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateDoctorScheduleDto dto)
        {
            var schedule = await _unitOfWork.DoctorSchedules.GetByIdAsync(id);

            if (schedule == null)
                return NotFound();

            schedule.UpdateEntity(dto);

            await _unitOfWork.SaveChangesAsync();

            return Ok(schedule.ToDto());
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _unitOfWork.DoctorSchedules.Delete(id);

            if (!deleted)
                return NotFound();

            await _unitOfWork.SaveChangesAsync();

            return Ok("Record deleted successfully");
        }
    }
}
