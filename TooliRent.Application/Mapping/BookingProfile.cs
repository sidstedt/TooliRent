using AutoMapper;
using TooliRent.Application.DTOs;
using TooliRent.Domain.Entities;
using TooliRent.Domain.Enums;

namespace TooliRent.Application.Mapping
{
    public sealed class BookingProfile : Profile
    {
        public BookingProfile()
        {
            CreateMap<Booking, BookingListItemDto>()
                .ForMember(d => d.Status,  o => o.MapFrom(s => (int)s.Status))
                .ForMember(d => d.ItemCount, o => o.MapFrom(s => s.Items != null ? s.Items.Count : 0));

            CreateMap<Booking, BookingDetailDto>()
                .ForMember(d => d.Status, o => o.MapFrom(s => (int)s.Status))
                .ForMember(d => d.Items,  o => o.MapFrom(s => s.Items));

            CreateMap<BookingItem, BookingItemDto>()
                .ForMember(d => d.ToolName, o => o.MapFrom(s => s.Tool != null ? s.Tool.Name : string.Empty))
                .ForMember(d => d.Status,   o => o.MapFrom(s => (int)s.Status));

            CreateMap<CreateBookingDto, Booking>()
                .ForMember(d => d.StartDate, o => o.MapFrom(s => s.StartDate.Date))
                .ForMember(d => d.EndDate, o => o.MapFrom(s => s.EndDate.Date))
                .ForMember(d => d.Status, o => o.MapFrom(s => BookingStatus.Pending))
                .ForMember(d => d.Items, o => o.MapFrom(s => s.Items))
                .ForMember(d => d.User, o => o.Ignore())
                .ForMember(d => d.CreatedAt, o => o.Ignore());

            CreateMap<BookingItemCreateDto, BookingItem>()
                .ForMember(d => d.Status, o => o.MapFrom(s => BookingItemStatus.Reserved))
                .ForMember(d => d.Booking, o => o.Ignore())
                .ForMember(d => d.Tool, o => o.Ignore())
                .ForMember(d => d.CheckedOutAt, o => o.Ignore())
                .ForMember(d => d.ReturnedAt, o => o.Ignore());
        }
    }
}
