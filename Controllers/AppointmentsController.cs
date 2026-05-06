using ClinicBooking.API.Common;
using ClinicBooking.API.Contracts;
using ClinicBooking.API.Dtos.Appoinments;
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
    public class AppointmentsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAppointmentService _appointmentService;

        public AppointmentsController(IUnitOfWork unitOfWork, IAppointmentService appointmentService)
        {
            _unitOfWork = unitOfWork;
            _appointmentService = appointmentService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> GetAll()
        {
            var appointments = await _unitOfWork.Appointments.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<object>>.Ok(appointments.Select(a => a.ToDto())));
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(id);
            if (appointment == null)
                return NotFound(ApiResponse.FailNoData($"Appointment with id {id} not found."));

            return Ok(ApiResponse<object>.Ok(appointment.ToDto()));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Patient")]
        public async Task<IActionResult> Create([FromBody] CreateAppointmentDto appointmentDto)
        {
            var patient = await _unitOfWork.Patients.GetByIdAsync(appointmentDto.PatientId);
            if (patient == null)
                return NotFound(ApiResponse.FailNoData($"Patient with id {appointmentDto.PatientId} not found."));

            var doctor = await _unitOfWork.Doctors.GetByIdAsync(appointmentDto.DoctorId);
            if (doctor == null)
                return NotFound(ApiResponse.FailNoData($"Doctor with id {appointmentDto.DoctorId} not found."));

            // Prevent double booking
            var conflict = await _unitOfWork.Appointments
                .Query()
                .AnyAsync(a =>
                    a.DoctorId == appointmentDto.DoctorId &&
                    a.AppointmentDate == appointmentDto.AppointmentDate &&
                    a.Status != AppointmentStatus.Cancelled);

            if (conflict)
                return BadRequest(ApiResponse.FailNoData("This time slot is already booked. Please choose another."));

            var appointment = appointmentDto.ToEntity();
            await _unitOfWork.Appointments.AddAsync(appointment);
            await _unitOfWork.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = appointment.Id },
                ApiResponse<object>.Ok(appointment.ToDto(), "Appointment booked successfully."));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, UpdateAppointmentDto dto)
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(id);
            if (appointment == null)
                return NotFound(ApiResponse.FailNoData($"Appointment with id {id} not found."));

            var doctorExists = await _unitOfWork.Doctors.Query().AnyAsync(d => d.Id == dto.DoctorId);
            if (!doctorExists)
                return BadRequest(ApiResponse.FailNoData("Doctor not found."));

            var conflict = await _unitOfWork.Appointments
                .Query()
                .AnyAsync(a =>
                    a.DoctorId == dto.DoctorId &&
                    a.AppointmentDate == dto.AppointmentDate &&
                    a.Id != id &&
                    a.Status != AppointmentStatus.Cancelled);

            if (conflict)
                return BadRequest(ApiResponse.FailNoData("Doctor already has an appointment at this time."));

            appointment.UpdateEntity(dto);
            await _unitOfWork.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(appointment.ToDto(), "Appointment updated successfully."));
        }

        [HttpPatch("{id}/confirm")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Confirm(Guid id)
        {
            var result = await _appointmentService.Confirm(id);
            if (!result)
                return NotFound(ApiResponse.FailNoData($"Appointment with id {id} not found."));

            return Ok(ApiResponse.OkNoData("Appointment confirmed."));
        }

        [HttpPatch("{id}/complete")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Complete(Guid id)
        {
            var result = await _appointmentService.Complete(id);
            if (!result)
                return NotFound(ApiResponse.FailNoData($"Appointment with id {id} not found."));

            return Ok(ApiResponse.OkNoData("Appointment marked as complete."));
        }

        [HttpPatch("{id}/cancel")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var result = await _appointmentService.Cancel(id);
            if (!result)
                return NotFound(ApiResponse.FailNoData($"Appointment with id {id} not found."));

            return Ok(ApiResponse.OkNoData("Appointment cancelled."));
        }
    }
}
