using ClinicBooking.API.Common;
using ClinicBooking.API.Contracts;
using ClinicBooking.API.Dtos.DoctorSchedules;
using ClinicBooking.API.Enums;
using ClinicBooking.API.Mappings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicBooking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DoctorSchedulesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public DoctorSchedulesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> GetAll()
        {
            var schedules = await _unitOfWork.DoctorSchedules.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<object>>.Ok(schedules.Select(s => s.ToDto())));
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var schedule = await _unitOfWork.DoctorSchedules.GetByIdAsync(id);
            if (schedule == null)
                return NotFound(ApiResponse.FailNoData($"Schedule with id {id} not found."));

            return Ok(ApiResponse<object>.Ok(schedule.ToDto()));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateDoctorScheduleDto dto)
        {
            var doctorExists = await _unitOfWork.Doctors.Query().AnyAsync(d => d.Id == dto.DoctorId);
            if (!doctorExists)
                return NotFound(ApiResponse.FailNoData("Doctor not found."));

            var schedule = dto.ToEntity();
            await _unitOfWork.DoctorSchedules.AddAsync(schedule);
            await _unitOfWork.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = schedule.Id },
                ApiResponse<object>.Ok(schedule.ToDto(), "Schedule created successfully."));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, UpdateDoctorScheduleDto dto)
        {
            var schedule = await _unitOfWork.DoctorSchedules.GetByIdAsync(id);
            if (schedule == null)
                return NotFound(ApiResponse.FailNoData($"Schedule with id {id} not found."));

            schedule.UpdateEntity(dto);
            await _unitOfWork.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(schedule.ToDto(), "Schedule updated successfully."));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _unitOfWork.DoctorSchedules.Delete(id);
            if (!deleted)
                return NotFound(ApiResponse.FailNoData($"Schedule with id {id} not found."));

            await _unitOfWork.SaveChangesAsync();
            return Ok(ApiResponse.OkNoData("Schedule deleted successfully."));
        }

        [HttpGet("doctor/{doctorId}/available-slots")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> GetAvailableSlots(Guid doctorId, DateTime date)
        {
            var schedules = await _unitOfWork.DoctorSchedules
                .Query()
                .Where(s => s.DoctorId == doctorId && s.DayOfWeek == date.DayOfWeek)
                .ToListAsync();

            if (!schedules.Any())
                return Ok(ApiResponse<List<DateTime>>.Ok(new List<DateTime>(), "No schedule found for this day."));

            var allSlots = new List<DateTime>();
            foreach (var schedule in schedules)
            {
                var start = date.Date + schedule.StartTime;
                var end = date.Date + schedule.EndTime;
                while (start < end)
                {
                    allSlots.Add(start);
                    start = start.AddMinutes(schedule.SlotDurationMinutes);
                }
            }

            var booked = await _unitOfWork.Appointments
                .Query()
                .Where(a =>
                    a.DoctorId == doctorId &&
                    a.AppointmentDate.Date == date.Date &&
                    a.Status != AppointmentStatus.Cancelled)
                .Select(a => a.AppointmentDate)
                .ToListAsync();

            var available = allSlots.Where(slot => !booked.Contains(slot)).ToList();
            return Ok(ApiResponse<List<DateTime>>.Ok(available));
        }
    }
}
