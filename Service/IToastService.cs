using System.Threading.Tasks;
using Vaultify.ViewModels;
namespace Vaultify.Service;

public interface IToastService
{
    ToastNotificationViewModel Notification { get; }
    public Task ShowMessageAsync(string title, string message, bool isVisible, string iconName, string hexCodeBG,
        string hexCodeSFG, int durationMilliseconds = 3000);
}