using ClinicBooking.API.Common;
using ClinicBooking.API.Contracts;
using ClinicBooking.API.Dtos.Doctors;
using ClinicBooking.API.Entities;
using ClinicBooking.API.Mappings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicBooking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DoctorsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DoctorsController(IUnitOfWork unitOfWork, IEmailService emailService, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> GetAll()
        {
            var doctors = await _unitOfWork.Doctors
                .Query()
                .Where(d => d.IsApproved && d.IsActive)
                .Include(d => d.Specialization)
                .Select(d => d.ToDto())
                .ToListAsync();

            return Ok(ApiResponse<List<DoctorResponseDto>>.Ok(doctors));
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var doctor = await _unitOfWork.Doctors
                .Query()
                .Include(d => d.Specialization)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null)
                return NotFound(ApiResponse.FailNoData($"Doctor with id {id} not found."));

            return Ok(ApiResponse<DoctorResponseDto>.Ok(doctor.ToDto()));
        }

        [HttpGet("pending")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPending()
        {
            var pending = await _unitOfWork.Doctors
                .Query()
                .Where(d => !d.IsApproved && !d.IsRejected)
                .Include(d => d.Specialization)
                .Select(d => d.ToDto())
                .ToListAsync();

            return Ok(ApiResponse<List<DoctorResponseDto>>.Ok(pending));
        }

        [HttpPatch("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveDoctorDto dto)
        {
            var doctor = await _unitOfWork.Doctors.GetByIdAsync(id);
            if (doctor == null)
                return NotFound(ApiResponse.FailNoData($"Doctor with id {id} not found."));

            if (doctor.IsApproved)
                return BadRequest(ApiResponse.FailNoData("Doctor is already approved."));

            var specialization = await _unitOfWork.Specializations.GetByIdAsync(dto.SpecializationId);
            if (specialization == null)
                return BadRequest(ApiResponse.FailNoData("Invalid specialization."));

            doctor.IsApproved = true;
            doctor.IsActive = true;
            doctor.IsRejected = false;
            doctor.RejectionReason = null;
            doctor.SpecializationId = dto.SpecializationId;
            doctor.ConsultationFee = dto.ConsultationFee;
            doctor.ApprovedAt = DateTime.UtcNow;

            _unitOfWork.Doctors.Update(doctor);
            await _unitOfWork.SaveChangesAsync();

            await _emailService.SendDoctorApprovedAsync(doctor.Email, doctor.FullName);

            return Ok(ApiResponse.OkNoData($"Dr. {doctor.FullName} has been approved."));
        }

        [HttpPatch("{id}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectDoctorDto dto)
        {
            var doctor = await _unitOfWork.Doctors.GetByIdAsync(id);
            if (doctor == null)
                return NotFound(ApiResponse.FailNoData($"Doctor with id {id} not found."));

            await _emailService.SendDoctorRejectedAsync(doctor.Email, doctor.FullName, dto.Reason);

            if (dto.Permanent)
            {
                var user = await _userManager.FindByIdAsync(doctor.UserId.ToString());
                if (user != null) await _userManager.DeleteAsync(user);

                await _unitOfWork.Doctors.Delete(id);
                await _unitOfWork.SaveChangesAsync();

                return Ok(ApiResponse.OkNoData("Doctor account permanently removed."));
            }

            doctor.IsRejected = true;
            doctor.IsApproved = false;
            doctor.IsActive = false;
            doctor.RejectionReason = dto.Reason;

            _unitOfWork.Doctors.Update(doctor);
            await _unitOfWork.SaveChangesAsync();

            return Ok(ApiResponse.OkNoData("Doctor application rejected. They can resubmit documents."));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateDoctorDto doctorDto)
        {
            if (doctorDto.SpecializationId.HasValue)
            {
                var specialization = await _unitOfWork.Specializations.GetByIdAsync(doctorDto.SpecializationId.Value);
                if (specialization == null)
                    return BadRequest(ApiResponse.FailNoData("Invalid specialization."));
            }

            var doctor = doctorDto.ToEntity();
            await _unitOfWork.Doctors.AddAsync(doctor);
            await _unitOfWork.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = doctor.Id },
                ApiResponse<DoctorResponseDto>.Ok(doctor.ToDto(), "Doctor created successfully."));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDoctorDto doctorDto)
        {
            var doctor = await _unitOfWork.Doctors.GetByIdAsync(id);
            if (doctor == null)
                return NotFound(ApiResponse.FailNoData($"Doctor with id {id} not found."));

            doctor.UpdateEntity(doctorDto);
            _unitOfWork.Doctors.Update(doctor);
            await _unitOfWork.SaveChangesAsync();

            return Ok(ApiResponse<DoctorResponseDto>.Ok(doctor.ToDto(), "Doctor updated successfully."));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _unitOfWork.Doctors.Delete(id);
            if (!deleted)
                return NotFound(ApiResponse.FailNoData($"Doctor with id {id} not found."));

            await _unitOfWork.SaveChangesAsync();
            return Ok(ApiResponse.OkNoData("Doctor deleted successfully."));
        }
    }
}
