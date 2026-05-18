using MediatR;

namespace SmartDelivery.Domaine.Commands
{
    public class AddGenericCommand<T> : IRequest<T> where T : class
    {
        public T Entite { get; set; }
        public AddGenericCommand(T entite) => Entite = entite;
    }
}
