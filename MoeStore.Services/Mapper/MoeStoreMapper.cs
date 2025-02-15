using AutoMapper;
using MoeStore.Entities.Models;
using MoeStore.Entities.Models.DTO;

namespace MoeStore.Services.Mapper
{
    public class MoeStoreMapper : Profile
    {
        public MoeStoreMapper()
        {
            CreateMap<Contact, ContactDto>()
                .ForMember(dest => dest.SubjectId, opt => opt.MapFrom(src => src.Subject != null ? src.Subject.Id : 0))
                .ReverseMap()
                .ForMember(dest => dest.Subject, opt => opt.Ignore());

            CreateMap<CreateContactDto, Contact>()
                .ForMember(dest => dest.Subject, opt => opt.Ignore());

            CreateMap<UpdateContactDto, Contact>()
                .ForMember(dest => dest.Subject, opt => opt.Ignore());
        }
    }
}
