using MediatR;

namespace SmartDelivery.Domaine.Commands
{
    public class PutGenericCommand<T> : IRequest<T> where T : class
    {
        public T Entite { get; set; }
        public PutGenericCommand(T entite) => Entite = entite;
    }
}
