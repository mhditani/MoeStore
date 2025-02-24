using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoeStore.Entities.DB;
using MoeStore.Entities.Models;
using MoeStore.Entities.Models.DTO;
using MoeStore.Services;
using MoeStore.Services.Repository.IRepository;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactsController : ControllerBase
    {
        private readonly IContactRepo repo;
        private readonly IMapper mapper;
        private readonly ISubjectRepo subjectRepo;
        private readonly ApplicationDbContext db;
        private readonly EmailSender emailSender;

        public ContactsController(IContactRepo repo, IMapper mapper, ISubjectRepo subjectRepo, ApplicationDbContext db, EmailSender emailSender)
        {
            this.repo = repo;
            this.mapper = mapper;
            this.subjectRepo = subjectRepo;
            this.db = db;
            this.emailSender = emailSender;
        }

        [HttpGet("subjects")]
        public async Task<IActionResult> GetSubjects()
        {
            var listSubjects = await db.Subjects.ToListAsync();
            return Ok(listSubjects);
        }

        [HttpGet]
        public async Task<IActionResult> GetContacts(int? page = 1, int pageSize = 5)
        {
            if (page < 1) page = 1;

            var (contactsDomain, totalCount) = await repo.GetAllAsync(page.Value, pageSize);
            var contactsDto = mapper.Map<IEnumerable<ContactDto>>(contactsDomain);

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var response = new
            {
                Contacts = contactsDto,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasNextPage = page < totalPages,
                HasPreviousPage = page > 1
            };

            return Ok(response);
        }


        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetContact([FromRoute] int id)
        {
            var contactDomain = await repo.GetByIdAsyn(id);
            if (contactDomain == null)
            {
                return NotFound();
            }
            return Ok(mapper.Map<ContactDto>(contactDomain));
        }

        [HttpPost]
        public async Task<IActionResult> CreateContact([FromBody] CreateContactDto createContactDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var subject = await db.Subjects.FindAsync(createContactDto.SubjectId);
            if (subject == null)
            {
                ModelState.AddModelError("Subject", "Please select a valid subject");
                return BadRequest(ModelState);
            }

            var contactDomain = mapper.Map<Contact>(createContactDto);
            contactDomain.Subject = subject; // Explicitly set Subject

            await repo.CreateAsync(contactDomain);

            // send confirmation email
            string emailSubject = "Contact Information";
            string username = createContactDto.FirstName + " " + createContactDto.LastName;
            string emailMessage = "Dear " + username + "\n" +
                "we recieved your message. Thank you for contacting us.\n" +
                "Our team will contact you very soon.\n" +
                "Best Regards\n\n" +
                "Your Message:\n" + createContactDto.Message;

            emailSender.SendEmail(emailSubject, contactDomain.Email, username, emailMessage).Wait();

            return Ok(mapper.Map<ContactDto>(contactDomain));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateContact([FromRoute] int id, [FromBody] UpdateContactDto updateContactDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var subject = await db.Subjects.FindAsync(updateContactDto.SubjectId);
            if (subject == null)
            {
                ModelState.AddModelError("Subject", "Please select a valid subject");
                return BadRequest(ModelState);
            }

            var existingContact = await repo.GetByIdAsyn(id);
            if (existingContact == null)
            {
                return NotFound();
            }

            mapper.Map(updateContactDto, existingContact);
            existingContact.Subject = subject; // Explicitly set Subject

            var updatedContact = await repo.UpdateAsync(id, existingContact);
            return Ok(mapper.Map<ContactDto>(updatedContact));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteContact([FromRoute] int id)
        {
            var deletedDomain = await repo.DeleteAsync(id);
            if (deletedDomain == null)
            {
                return NotFound();
            }
            return Ok(mapper.Map<ContactDto>(deletedDomain));
        }
    }
}
