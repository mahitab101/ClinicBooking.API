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

        // Admin, Doctor — full appointment list
        [HttpGet]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> GetAll()
        {
            var appointments = await _unitOfWork.Appointments.GetAllAsync();
            return Ok(appointments.Select(a => a.ToDto()));
        }

        // Admin, Doctor — view any appointment
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(id);
            if (appointment == null) return NotFound();
            return Ok(appointment.ToDto());
        }

        // Patient, Admin — book an appointment
        [HttpPost]
        [Authorize(Roles = "Admin,Patient")]
        public async Task<IActionResult> Create([FromBody] CreateAppointmentDto appointmentDto)
        {
            var patient = await _unitOfWork.Patients.GetByIdAsync(appointmentDto.PatientId);
            if (patient == null) return NotFound($"Patient with id {appointmentDto.PatientId} not found.");

            var doctor = await _unitOfWork.Doctors.GetByIdAsync(appointmentDto.DoctorId);
            if (doctor == null) return NotFound($"Doctor with id {appointmentDto.DoctorId} not found.");

            var appointment = appointmentDto.ToEntity();
            await _unitOfWork.Appointments.AddAsync(appointment);
            await _unitOfWork.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, appointment.ToDto());
        }

        // Admin only — reschedule/update appointment details
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, UpdateAppointmentDto dto)
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(id);
            if (appointment == null) return NotFound();

            var doctorExists = await _unitOfWork.Doctors.Query().AnyAsync(d => d.Id == dto.DoctorId);
            if (!doctorExists) return BadRequest("Doctor not found");

            var conflict = await _unitOfWork.Appointments
                .Query()
                .AnyAsync(a =>
                    a.DoctorId == dto.DoctorId &&
                    a.AppointmentDate == dto.AppointmentDate &&
                    a.Id != id &&
                    (a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Confirmed));

            if (conflict) return BadRequest("Doctor already has an appointment at this time");

            appointment.UpdateEntity(dto);
            await _unitOfWork.SaveChangesAsync();

            return Ok(appointment.ToDto());
        }

        // Admin, Doctor — confirm an appointment
        [HttpPatch("{id}/confirm")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Confirm(Guid id)
        {
            try
            {
                var result = await _appointmentService.Confirm(id);
                if (!result) return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Admin, Doctor — mark appointment as complete
        [HttpPatch("{id}/complete")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Complete(Guid id)
        {
            try
            {
                var result = await _appointmentService.Complete(id);
                if (!result) return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Admin, Doctor, Patient — anyone involved can cancel
        [HttpPatch("{id}/cancel")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            try
            {
                var result = await _appointmentService.Cancel(id);
                if (!result) return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
