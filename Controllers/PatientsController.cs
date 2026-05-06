using ClinicBooking.API.Common;
using ClinicBooking.API.Contracts;
using ClinicBooking.API.Dtos.Appoinments;
using ClinicBooking.API.Dtos.Patients;
using ClinicBooking.API.Helpers;
using ClinicBooking.API.Mappings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClinicBooking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PatientsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public PatientsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> GetAll()
        {
            var patients = await _unitOfWork.Patients.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<object>>.Ok(patients.Select(p => p.ToDto())));
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> GetById(Guid id)
        {
            if (User.IsInRole("Patient") && !IsCurrentUser(id))
                return StatusCode(403, ApiResponse.FailNoData("You can only view your own profile."));

            var patient = await _unitOfWork.Patients.GetByIdAsync(id);
            if (patient == null)
                return NotFound(ApiResponse.FailNoData($"Patient with id {id} not found."));

            return Ok(ApiResponse<object>.Ok(patient.ToDto()));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreatePatientDto patientDto)
        {
            var patient = patientDto.ToEntity();
            await _unitOfWork.Patients.AddAsync(patient);
            await _unitOfWork.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = patient.Id },
                ApiResponse<object>.Ok(patient.ToDto(), "Patient created successfully."));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Patient")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePatientDto patientDto)
        {
            if (User.IsInRole("Patient") && !IsCurrentUser(id))
                return StatusCode(403, ApiResponse.FailNoData("You can only update your own profile."));

            var patient = await _unitOfWork.Patients.GetByIdAsync(id);
            if (patient == null)
                return NotFound(ApiResponse.FailNoData($"Patient with id {id} not found."));

            patient.UpdateEntity(patientDto);
            _unitOfWork.Patients.Update(patient);
            await _unitOfWork.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(patient.ToDto(), "Patient updated successfully."));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _unitOfWork.Patients.Delete(id);
            if (!deleted)
                return NotFound(ApiResponse.FailNoData($"Patient with id {id} not found."));

            await _unitOfWork.SaveChangesAsync();
            return Ok(ApiResponse.OkNoData("Patient deleted successfully."));
        }

        [HttpGet("{id}/appointments")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> GetAppointments(Guid id, int pageNumber = 1, int pageSize = 10)
        {
            if (User.IsInRole("Patient") && !IsCurrentUser(id))
                return StatusCode(403, ApiResponse.FailNoData("You can only view your own appointments."));

            var exists = await _unitOfWork.Patients.Query().AnyAsync(p => p.Id == id);
            if (!exists)
                return NotFound(ApiResponse.FailNoData($"Patient with id {id} not found."));

            var query = _unitOfWork.Appointments
                .Query()
                .Where(a => a.PatientId == id)
                .OrderByDescending(a => a.AppointmentDate)
                .Select(a => new AppointmentSummaryDto
                {
                    Id = a.Id,
                    AppointmentDate = a.AppointmentDate,
                    Status = a.Status,
                    DoctorId = a.DoctorId,
                    DoctorName = a.Doctor.FullName,
                    PatientId = a.PatientId,
                    PatientName = a.Patient.FullName
                });

            var paged = await query.ToPagedResultAsync(pageNumber, pageSize);
            return Ok(ApiResponse<object>.Ok(paged));
        }

        private bool IsCurrentUser(Guid id) =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) == id.ToString();
    }
}
