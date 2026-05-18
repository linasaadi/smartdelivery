using AutoMapper;
using SmartDelivery.Domaine.Models;
using SmartDelivery.Domaine.Models.DTOs;

namespace SmartDelivery.Infrastructure.Mappings
{
    public class ProfilDivers : Profile
    {
        public ProfilDivers()
        {
            // Destination
            CreateMap<Destination, DestinationDto>();
            CreateMap<DestinationFormDto, Destination>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.Livraisons, opt => opt.Ignore());

            // Produit
            CreateMap<Produit, ProduitDto>();
            CreateMap<ProduitFormDto, Produit>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.LivraisonProduits, opt => opt.Ignore());

            // Anomalie
            CreateMap<Anomalie, AnomalieDto>()
                .ForCtorParam("Type", opt => opt.MapFrom(s => s.Type.ToString()));

            // Tracking
            CreateMap<PointTracking, PointTrackingDto>();
        }
    }
}
