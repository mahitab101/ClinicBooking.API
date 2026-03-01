using ClinicBooking.API.Common;
using ClinicBooking.API.Contracts;
using ClinicBooking.API.Dtos.Apoinments;
using ClinicBooking.API.Dtos.Patients;
using ClinicBooking.API.Helpers;
using ClinicBooking.API.Mappings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicBooking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public PatientsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var patients = await _unitOfWork.Patients.GetAllAsync();
            var response = patients.Select(p => p.ToDto()).ToList();
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var patient = await _unitOfWork.Patients.GetByIdAsync(id);
            if (patient == null) return NotFound();
            return Ok(patient.ToDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePatientDto patientDto)
        {
            var patient = patientDto.ToEntity();
            await _unitOfWork.Patients.AddAsync(patient);
            await _unitOfWork.SaveChangesAsync();


            return CreatedAtAction(
                nameof(GetById),
                new { id = patient.Id },
                patient.ToDto()
                );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePatientDto patientDto)
        {
            var patient = await _unitOfWork.Patients.GetByIdAsync(id);
            if (patient == null) return NotFound();

            patient.UpdateEntity(patientDto);

            _unitOfWork.Patients.Update(patient);
            await _unitOfWork.SaveChangesAsync();

            return Ok(patient.ToDto());
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var delete = await _unitOfWork.Patients.Delete(id);
            if (!delete) return NotFound();

            await _unitOfWork.SaveChangesAsync();
            return Ok("Record deleted successfully");
        }

        [HttpGet("{id}/appointments")]
        public async Task<ActionResult<PagedResult<AppointmentSummaryDto>>> GetAppointments(Guid id, int pageNumber = 1, int pageSize = 10)
        {
            var exist = await _unitOfWork.Patients.Query().AnyAsync(p => p.Id == id);
            if (!exist) return NotFound();

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
                                             DoctorName = a.Doctor.FullName
                                         });
            var response = await query.ToPagedResultAsync(pageNumber, pageSize);
            return response;
        }
    }
}
