using ClinicBooking.API.Contracts;
using ClinicBooking.API.Dtos.Doctors;
using ClinicBooking.API.Mappings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicBooking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public DoctorsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var doctors = await _unitOfWork.Doctors
                                            .Query()
                                            .Include(d => d.Specialization)
                                            .Select(d => d.ToDto())
                                            .ToListAsync();
            return Ok(doctors);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var doctor = await _unitOfWork.Doctors
                                          .Query()
                                          .Include(d => d.Specialization)
                                          .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null) return NotFound();
            return Ok(doctor.ToDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDoctorDto doctorDto)
        {
            var specialization = await _unitOfWork
                                .Specializations
                                .GetByIdAsync(doctorDto.SpecializationId);

            if (specialization == null)
                return BadRequest("Invalid specialization");

            var doctor = doctorDto.ToEntity();
            await _unitOfWork.Doctors.AddAsync(doctor);
            await _unitOfWork.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = doctor.Id },
                doctor.ToDto()
                );
        }

        [HttpPut("{id}")]
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
        public async Task<IActionResult> Delete(Guid id)
        {
            var delete = await _unitOfWork.Doctors.Delete(id);
            if (!delete) return NotFound();

            await _unitOfWork.SaveChangesAsync();
            return Ok("Doctor deleted successfully");
        }
    }
}
