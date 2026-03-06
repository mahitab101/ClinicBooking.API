using ClinicBooking.API.Contracts;
using ClinicBooking.API.Dtos.Apoinment;
using ClinicBooking.API.Mappings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ClinicBooking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public AppointmentsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var appoinments = await _unitOfWork.Appointments.GetAllAsync();
            var response = appoinments.Select(a => a.ToDto());
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(Guid id)
        {
            var appoinment = await _unitOfWork.Appointments.GetByIdAsync(id);
            if (appoinment == null) return NotFound();

            return Ok(appoinment.ToDto());
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CreateAppointmentDto appointmentDto)
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
    }
}
