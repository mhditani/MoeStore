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

            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.ImageFile, opt => opt.Ignore()) // Ignore because it's a file, not a string
                .ReverseMap()
                .ForMember(dest => dest.ImageFileName, opt => opt.MapFrom(src =>
                    src.ImageFile != null ? src.ImageFile.FileName : null)); // Extract filename if file is provided

        }
    }
}
