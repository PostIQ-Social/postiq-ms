using AutoMapper;
using Published.Application.Commands;
using Published.Application.Response;
using Published.Core.Entities;

namespace Published.Application.Mappers
{
    public class RepoDetailsMapper : Profile
    {
        public RepoDetailsMapper()
        {
            CreateMap<ProcessedPost, BatchRepoRes>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Repo != null ? src.Repo.Job.UserId : 0))
                .ForMember(dest => dest.Source, opt => opt.MapFrom(src => src.Repo != null ? src.Repo.Job.Source : null))
                .ForMember(dest => dest.RepoUrl, opt => opt.MapFrom(src => src.Repo != null ? src.Repo.RepoUrl : null))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Headline != null ? src.Headline : src.OriginalTitle))
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.OriginalAuthor))
                .ForMember(dest => dest.PostedOn, opt => opt.MapFrom(src => src.Repo != null ? src.Repo.PostedOn : src.CreatedOn));

            CreateMap<AddJobCommand, Job>();
        }
    }
}
