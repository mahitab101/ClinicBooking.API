using ClinicBooking.API.Contracts;
using Microsoft.EntityFrameworkCore;
using ClinicBooking.API.Mappings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ClinicBooking.API.Enums;
using ClinicBooking.API.Dtos.Appoinments;

namespace ClinicBooking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
        public async Task<IActionResult> GetAll()
        {
            var appoinments = await _unitOfWork.Appointments.GetAllAsync();
            var response = appoinments.Select(a => a.ToDto());
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var appoinment = await _unitOfWork.Appointments.GetByIdAsync(id);
            if (appoinment == null) return NotFound();

            return Ok(appoinment.ToDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAppointmentDto appointmentDto)
        {
            //TODO: unified the response
            var patient = await _unitOfWork.Patients.GetByIdAsync(appointmentDto.PatientId);
            if (patient == null) return NotFound($"Patient with id {appointmentDto.PatientId} not found.");

            var doctor = await _unitOfWork.Doctors.GetByIdAsync(appointmentDto.DoctorId);
            if (doctor == null) return NotFound($"doctor with id {appointmentDto.DoctorId} not found.");

            var appointment = appointmentDto.ToEntity();

            await _unitOfWork.Appointments.AddAsync(appointment);
            await _unitOfWork.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, appointment.ToDto());
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateAppointmentDto dto)
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(id);

            if (appointment == null)
                return NotFound();

            var doctorExists = await _unitOfWork.Doctors
                .Query()
                .AnyAsync(d => d.Id == dto.DoctorId);

            if (!doctorExists)
                return BadRequest("Doctor not found");

            var conflict = await _unitOfWork.Appointments
                .Query()
                .AnyAsync(a =>
                    a.DoctorId == dto.DoctorId &&
                    a.AppointmentDate == dto.AppointmentDate &&
                    a.Id != id &&
                    (a.Status == AppointmentStatus.Pending ||
                     a.Status == AppointmentStatus.Confirmed));

            if (conflict)
                return BadRequest("Doctor already has an appointment at this time");

            appointment.UpdateEntity(dto);

            await _unitOfWork.SaveChangesAsync();

            return Ok(appointment.ToDto());
        }

        [HttpPatch("{id}/confirm")]
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

        [HttpPatch("{id}/complete")]
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

        [HttpPatch("{id}/cancel")]
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
