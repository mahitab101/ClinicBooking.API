using ClinicBooking.API.Contracts;
using ClinicBooking.API.Dtos.Doctors;
using ClinicBooking.API.Mappings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicBooking.API.Entities;

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

        public DoctorsController(
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _userManager = userManager;
        }

        // Admin, Doctor, Patient — browse approved doctors only
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
            return Ok(doctors);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var doctor = await _unitOfWork.Doctors
                .Query()
                .Include(d => d.Specialization)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null) return NotFound();
            return Ok(doctor.ToDto());
        }

        // ─── Approval workflow (Admin only) ───────────────────────────────────

        /// <summary>Get all doctors pending approval</summary>
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
            return Ok(pending);
        }

        /// <summary>Approve a doctor — assign specialization and set them active</summary>
        [HttpPatch("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveDoctorDto dto)
        {
            var doctor = await _unitOfWork.Doctors.GetByIdAsync(id);
            if (doctor == null) return NotFound();

            if (doctor.IsApproved)
                return BadRequest(new { message = "Doctor is already approved." });

            var specialization = await _unitOfWork.Specializations.GetByIdAsync(dto.SpecializationId);
            if (specialization == null)
                return BadRequest(new { message = "Invalid specialization." });

            doctor.IsApproved = true;
            doctor.IsActive = true;
            doctor.IsRejected = false;
            doctor.RejectionReason = null;
            doctor.SpecializationId = dto.SpecializationId;
            doctor.ConsultationFee = dto.ConsultationFee;
            doctor.ApprovedAt = DateTime.UtcNow;

            _unitOfWork.Doctors.Update(doctor);
            await _unitOfWork.SaveChangesAsync();

            // Notify doctor
            await _emailService.SendDoctorApprovedAsync(doctor.Email, doctor.FullName);

            return Ok(new { message = $"Dr. {doctor.FullName} has been approved." });
        }

        /// <summary>
        /// Reject a doctor.
        /// Permanent = true  → deletes ApplicationUser + Doctor row (fraud/fake cert)
        /// Permanent = false → soft reject, doctor can resubmit documents
        /// </summary>
        [HttpPatch("{id}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectDoctorDto dto)
        {
            var doctor = await _unitOfWork.Doctors.GetByIdAsync(id);
            if (doctor == null) return NotFound();

            // Notify doctor before any deletion
            await _emailService.SendDoctorRejectedAsync(doctor.Email, doctor.FullName, dto.Reason);

            if (dto.Permanent)
            {
                // Hard delete — remove user account and doctor profile entirely
                var user = await _userManager.FindByIdAsync(doctor.UserId.ToString());
                if (user != null)
                    await _userManager.DeleteAsync(user);

                await _unitOfWork.Doctors.Delete(id);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { message = "Doctor account permanently removed." });
            }

            // Soft reject — keep the account, allow resubmission
            doctor.IsRejected = true;
            doctor.IsApproved = false;
            doctor.IsActive = false;
            doctor.RejectionReason = dto.Reason;

            _unitOfWork.Doctors.Update(doctor);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { message = "Doctor application rejected. They can resubmit documents." });
        }

        // ─── Standard CRUD (Admin only) ───────────────────────────────────────

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateDoctorDto doctorDto)
        {
            if (doctorDto.SpecializationId.HasValue)
            {
                var specialization = await _unitOfWork.Specializations
                    .GetByIdAsync(doctorDto.SpecializationId.Value);
                if (specialization == null)
                    return BadRequest("Invalid specialization");
            }

            var doctor = doctorDto.ToEntity();
            await _unitOfWork.Doctors.AddAsync(doctor);
            await _unitOfWork.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = doctor.Id }, doctor.ToDto());
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDoctorDto doctorDto)
        {
            var doctor = await _unitOfWork.Doctors.GetByIdAsync(id);
            if (doctor == null) return NotFound();

            doctor.UpdateEntity(doctorDto);
            _unitOfWork.Doctors.Update(doctor);
            await _unitOfWork.SaveChangesAsync();

            return Ok(doctor.ToDto());
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _unitOfWork.Doctors.Delete(id);
            if (!deleted) return NotFound();

            await _unitOfWork.SaveChangesAsync();
            return NoContent();
        }
    }
}
