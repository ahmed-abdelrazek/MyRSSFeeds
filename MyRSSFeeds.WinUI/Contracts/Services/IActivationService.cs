using System.Threading.Tasks;

namespace MyRSSFeeds.Contracts.Services
{
    public interface IActivationService
    {
        Task ActivateAsync(object activationArgs);
    }
}
