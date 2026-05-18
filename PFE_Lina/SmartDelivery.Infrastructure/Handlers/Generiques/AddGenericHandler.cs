using MediatR;
using SmartDelivery.Domaine.Commands;
using SmartDelivery.Domaine.Interface;

namespace SmartDelivery.Infrastructure.Handlers.Generiques
{
    public class AddGenericHandler<T> : IRequestHandler<AddGenericCommand<T>, T> where T : class
    {
        private readonly IGenericRepository<T> _repository;
        public AddGenericHandler(IGenericRepository<T> repository) => _repository = repository;

        public async Task<T> Handle(AddGenericCommand<T> requete, CancellationToken annulation)
            => await _repository.Ajouter(requete.Entite);
    }
}
