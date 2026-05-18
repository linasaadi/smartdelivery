using AutoMapper;
using SmartDelivery.Domaine.Models;
using SmartDelivery.Domaine.Models.DTOs;

namespace SmartDelivery.Infrastructure.Mappings
{
    public class ProfilChauffeur : Profile
    {
        public ProfilChauffeur()
        {
            CreateMap<Chauffeur, ChauffeurResumeDto>()
                .ForCtorParam("NomComplet", opt => opt.MapFrom(s => $"{s.Prenom} {s.Nom}"));

            CreateMap<Chauffeur, ChauffeurDetailDto>()
                .ForCtorParam("NomComplet", opt => opt.MapFrom(s => $"{s.Prenom} {s.Nom}"))
                .ForCtorParam("Camions", opt => opt.MapFrom(s => s.Camions));

            CreateMap<ChauffeurFormDto, Chauffeur>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.EstDisponible, opt => opt.Ignore())
                .ForMember(d => d.Camions, opt => opt.Ignore());
        }
    }
}
