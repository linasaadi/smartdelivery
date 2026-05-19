using AutoMapper;
using SmartDelivery.Domaine.Models;
using SmartDelivery.Domaine.Models.DTOs;

namespace SmartDelivery.Infrastructure.Mappings
{
    public class ProfilDivers : Profile
    {
        public ProfilDivers()
        {
            // ── Destination ───────────────────────────────────────────────────
            CreateMap<Destination, DestinationDto>();
            CreateMap<DestinationFormDto, Destination>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.Livraisons, opt => opt.Ignore());

            // ── Anomalie ──────────────────────────────────────────────────────
            CreateMap<Anomalie, AnomalieDto>()
                .ForCtorParam("Type", opt => opt.MapFrom(s => s.Type.ToString()));

            // ── Tracking ──────────────────────────────────────────────────────
            CreateMap<PointTracking, PointTrackingDto>();

            // ── Reclamation ───────────────────────────────────────────────────
            CreateMap<ReclamationFormDto, Reclamation>()
                .ForMember(r => r.Id, opt => opt.Ignore())
                .ForMember(r => r.Statut, opt => opt.Ignore())
                .ForMember(r => r.DateCreation, opt => opt.Ignore())
                .ForMember(r => r.DateResolution, opt => opt.Ignore())
                .ForMember(r => r.ReponseAdmin, opt => opt.Ignore())
                .ForMember(r => r.UtilisateurId, opt => opt.Ignore())
                .ForMember(r => r.Utilisateur, opt => opt.Ignore())
                .ForMember(r => r.Livraison, opt => opt.Ignore());

            // ── Dispatcher ────────────────────────────────────────────────────
            CreateMap<Dispatcher, DispatcherResumeDto>()
                .ForCtorParam("NomComplet", opt => opt.MapFrom(s => $"{s.Prenom} {s.Nom}"));

            CreateMap<Dispatcher, DispatcherDetailDto>();

            CreateMap<DispatcherFormDto, Dispatcher>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.DatePriseEnCharge, opt => opt.Ignore())
                .ForMember(d => d.Livraisons, opt => opt.Ignore());
        }
    }
}
