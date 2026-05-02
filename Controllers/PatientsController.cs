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

        // Admin, Doctor — full patient list
        [HttpGet]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> GetAll()
        {
            var patients = await _unitOfWork.Patients.GetAllAsync();
            return Ok(patients.Select(p => p.ToDto()));
        }

        // Admin, Doctor can view anyone; Patient can only view themselves
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> GetById(Guid id)
        {
            if (User.IsInRole("Patient") && !IsCurrentUser(id))
                return Forbid();

            var patient = await _unitOfWork.Patients.GetByIdAsync(id);
            if (patient == null) return NotFound();
            return Ok(patient.ToDto());
        }

        // Admin only — create a patient record
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreatePatientDto patientDto)
        {
            var patient = patientDto.ToEntity();
            await _unitOfWork.Patients.AddAsync(patient);
            await _unitOfWork.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = patient.Id }, patient.ToDto());
        }

        // Admin can update anyone; Patient can only update themselves
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Patient")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePatientDto patientDto)
        {
            if (User.IsInRole("Patient") && !IsCurrentUser(id))
                return Forbid();

            var patient = await _unitOfWork.Patients.GetByIdAsync(id);
            if (patient == null) return NotFound();

            patient.UpdateEntity(patientDto);
            _unitOfWork.Patients.Update(patient);
            await _unitOfWork.SaveChangesAsync();

            return Ok(patient.ToDto());
        }

        // Admin only — soft-delete a patient
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _unitOfWork.Patients.Delete(id);
            if (!deleted) return NotFound();

            await _unitOfWork.SaveChangesAsync();
            return NoContent();
        }

        // Admin, Doctor can view anyone's appointments; Patient can only view their own
        [HttpGet("{id}/appointments")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<ActionResult<PagedResult<AppointmentSummaryDto>>> GetAppointments(
            Guid id, int pageNumber = 1, int pageSize = 10)
        {
            if (User.IsInRole("Patient") && !IsCurrentUser(id))
                return Forbid();

            var exists = await _unitOfWork.Patients.Query().AnyAsync(p => p.Id == id);
            if (!exists) return NotFound();

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

            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        // ─── Helper ──────────────────────────────────────────────────────────
        private bool IsCurrentUser(Guid id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return currentUserId == id.ToString();
        }
    }
}
