using System.Threading.Tasks;

namespace Vaultify.Service;

public interface IUpdateService
{
    public Task<GitHubReleaseData?> CheckUpdateInfoAsync();
}