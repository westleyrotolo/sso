using Microsoft.AspNetCore.Identity;

namespace SsoServer.Infrastructures;

public static class Extensions
{
    
    public static async Task ManageIdentityResultAsync(this Task<IdentityResult> identityResultTask)
    {
        var identityResult = await identityResultTask.WaitAsync(CancellationToken.None);
        if (!identityResult.Succeeded)
            throw new Exception(string.Join('\n', identityResult.Errors.Select(x => $"{x.Code} - {x.Description}")));

    }
}