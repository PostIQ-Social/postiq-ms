using AutoMapper;
using PostIQ.Core.Database.Entities;
using PostIQ.Core.Response;

namespace PostIQ.Core.Shared.Mappers
{
    public class ListResponseMapper : Profile
    {
        public ListResponseMapper()
        {
            CreateMap(typeof(IPaginate<>), typeof(ListResponse<>)).ConvertUsing(typeof(ListResponseConverter<,>));
        }
    }


    public class ListResponseConverter<TSource, TDest>
    : ITypeConverter<IPaginate<TSource>, ListResponse<TDest>>
    {
        public ListResponse<TDest> Convert(
            IPaginate<TSource> source,
            ListResponse<TDest> dest,
            ResolutionContext context)
        {
            dest = dest ?? new ListResponse<TDest>(new List<TDest>());

            List<TDest> payload = context.Mapper.Map<List<TDest>>(source.Data);

            dest.Data = payload;
            dest.Count = source.Count;
            dest.HasNext = source.HasNext;
            dest.HasPrevious = source.HasPrevious;
            dest.TotalPages = source.Pages;
            dest.Page = source.Index;
            dest.PerPage = source.Size;
            dest.Size = source.Size;

            return dest;
        }
    }
}
