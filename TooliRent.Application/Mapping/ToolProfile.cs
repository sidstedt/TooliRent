using AutoMapper;
using TooliRent.Application.DTOs;
using TooliRent.Domain.Entities;
using TooliRent.Domain.Enums;

namespace TooliRent.Application.Mapping
{
    public sealed class ToolProfile : Profile
    {
        public ToolProfile()
        {
            CreateMap<Tool, ToolListItemDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status));

            CreateMap<Tool, ToolDetailDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status));

            CreateMap<ToolCreateDto, Tool>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Trim()))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description.Trim()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ToolStatus.Available))
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

            CreateMap<ToolUpdateDto, Tool>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Trim()))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description.Trim()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (ToolStatus)src.Status))
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
        }
    }
}
