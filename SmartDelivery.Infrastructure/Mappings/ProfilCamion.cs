using AutoMapper;
using SmartDelivery.Domaine.Models;
using SmartDelivery.Domaine.Models.DTOs;

namespace SmartDelivery.Infrastructure.Mappings
{
    public class ProfilCamion : Profile
    {
        public ProfilCamion()
        {
            CreateMap<Camion, CamionResumeDto>()
                .ForCtorParam("Statut", opt => opt.MapFrom(s => s.Statut.ToString()))
                .ForCtorParam("NomChauffeur", opt => opt.MapFrom(s =>
                    s.Chauffeur != null
                        ? $"{s.Chauffeur.Prenom} {s.Chauffeur.Nom}"
                        : null));

            CreateMap<Camion, CamionDetailDto>()
                .ForCtorParam("Statut", opt => opt.MapFrom(s => s.Statut.ToString()))
                .ForCtorParam("Chauffeur", opt => opt.MapFrom(s => s.Chauffeur));

            CreateMap<CamionFormDto, Camion>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.Statut, opt => opt.Ignore())
                .ForMember(d => d.LatitudeActuelle, opt => opt.Ignore())
                .ForMember(d => d.LongitudeActuelle, opt => opt.Ignore())
                .ForMember(d => d.Livraisons, opt => opt.Ignore())
                .ForMember(d => d.Chauffeur, opt => opt.Ignore());
        }
    }
}
