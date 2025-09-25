using AutoMapper;
using TooliRent.Application.DTOs;
using TooliRent.Domain.Entities;

namespace TooliRent.Application.Mapping
{
    public sealed class ToolCategoryProfile : Profile
    {
        public ToolCategoryProfile()
        {
            CreateMap<ToolCategory, ToolCategoryListItemDto>();
            CreateMap<ToolCategory, ToolCategoryDetailDto>();

            CreateMap<ToolCategoryCreateDto, ToolCategory>()
                .ForMember(desc => desc.Name, opt => opt.MapFrom(src => src.Name.Trim()))
                .ForMember(desc => desc.Description, opt => opt.MapFrom(src =>
                string.IsNullOrWhiteSpace(src.Description) ? null : src.Description!.Trim()))
                .ForMember(desc => desc.Tools, opt => opt.Ignore());

            CreateMap<ToolCategoryUpdateDto, ToolCategory>()
                .ForMember(d => d.Name, o => o.MapFrom(s => s.Name.Trim()))
                .ForMember(d => d.Description, o => o.MapFrom(s => 
                    string.IsNullOrWhiteSpace(s.Description) ? null : s.Description!.Trim()))
                .ForMember(d => d.Tools, o => o.Ignore());
        }
    }
}
