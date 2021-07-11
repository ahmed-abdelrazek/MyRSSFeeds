using System.Threading.Tasks;

namespace MyRSSFeeds.Activation
{
    public interface IActivationHandler
    {
        bool CanHandle(object args);

        Task HandleAsync(object args);
    }
}
