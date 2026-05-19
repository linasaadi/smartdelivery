using AutoMapper;
using SmartDelivery.Domaine.Models;
using SmartDelivery.Domaine.Models.DTOs;

namespace SmartDelivery.Infrastructure.Mappings
{
    public class ProfilLivraison : Profile
    {
        public ProfilLivraison()
        {
            CreateMap<Livraison, LivraisonResumeDto>()
                .ForCtorParam("NomChauffeur", opt => opt.MapFrom(s =>
                    s.Camion != null && s.Camion.Chauffeur != null
                        ? $"{s.Camion.Chauffeur.Prenom} {s.Camion.Chauffeur.Nom}"
                        : null))
                .ForCtorParam("ImmatriculationCamion", opt => opt.MapFrom(s =>
                    s.Camion != null ? s.Camion.Immatriculation : null))
                .ForCtorParam("Destination", opt => opt.MapFrom(s =>
                    s.Destination != null
                        ? $"{s.Destination.Ville}, {s.Destination.Adresse}"
                        : ""))
                .ForCtorParam("Statut", opt => opt.MapFrom(s => s.Statut.ToString()))
                .ForCtorParam("NombreAnomaliesOuvertes", opt => opt.MapFrom(s =>
                    s.Anomalies.Count(a => !a.EstResolue)));

            CreateMap<Livraison, LivraisonDetailDto>()
                .ForCtorParam("Statut", opt => opt.MapFrom(s => s.Statut.ToString()))
                .ForCtorParam("Camion", opt => opt.MapFrom(s => s.Camion))
                .ForCtorParam("Destination", opt => opt.MapFrom(s => s.Destination))
                .ForCtorParam("Anomalies", opt => opt.MapFrom(s => s.Anomalies));
        }
    }
}
