using Microsoft.EntityFrameworkCore;
using MoeStore.Entities.DB;
using MoeStore.Entities.Models;
using MoeStore.Services.Repository.IRepository;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeStore.Services.Repository
{
    public class ContactRepo : IContactRepo
    {
        private readonly ApplicationDbContext db;

        public ContactRepo(ApplicationDbContext db)
        {
            this.db = db;
        }

        public async Task<Contact> CreateAsync(Contact contact)
        {
            await db.AddAsync(contact);
            await db.SaveChangesAsync();
            return contact;
        }

        public async Task<Contact?> DeleteAsync(int id)
        {
            var existingContact = await db.Contacts.FindAsync(id);
            if (existingContact == null)
            {
                return null;
            }
            db.Remove(existingContact);
            await db.SaveChangesAsync();
            return existingContact;
        }

        public async Task<List<Contact>> GetAllAsync()
        {
            return await db.Contacts.Include(c => c.Subject).ToListAsync(); // ✅ Fix: Include Subject
        }

        public async Task<Contact?> GetByIdAsyn(int id)
        {
            return await db.Contacts.Include(c => c.Subject).FirstOrDefaultAsync(x => x.Id == id); // ✅ Fix: Include Subject
        }

        public async Task<Contact?> UpdateAsync(int id, Contact contact)
        {
            var existingContact = await db.Contacts.Include(c => c.Subject).FirstOrDefaultAsync(x => x.Id == id);
            if (existingContact == null)
            {
                return null;
            }

            // Fetch the subject explicitly
            var subject = await db.Subjects.FindAsync(contact.Subject.Id);
            if (subject == null)
            {
                return null; // Ensure Subject exists
            }

            existingContact.FirstName = contact.FirstName;
            existingContact.LastName = contact.LastName;
            existingContact.Email = contact.Email;
            existingContact.Phone = contact.Phone;
            existingContact.Subject = subject; // ✅ Fix: Assign the tracked Subject
            existingContact.Message = contact.Message;

            await db.SaveChangesAsync();
            return existingContact;
        }
    }
}
