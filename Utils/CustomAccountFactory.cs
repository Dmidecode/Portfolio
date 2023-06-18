using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Security.Claims;
using System.Text.Json;

namespace Portfolio.Utils
{
    public class CustomAccountFactory : AccountClaimsPrincipalFactory<RemoteUserAccount>
    {
        public CustomAccountFactory(IAccessTokenProviderAccessor accessor)
      : base(accessor)
        { }

        public async override ValueTask<ClaimsPrincipal> CreateUserAsync(RemoteUserAccount account, RemoteAuthenticationUserOptions options)
        {
            // Step 1: create the user account
            var userAccount = await base.CreateUserAsync(account, options);
            var userIdentity = (ClaimsIdentity)userAccount.Identity;

            if (userIdentity.IsAuthenticated)
            {
                // Step 2: get the associated roles
                var roles = account.AdditionalProperties[userIdentity.RoleClaimType] as JsonElement?;

                if (roles?.ValueKind == JsonValueKind.Array)
                {
                    // Step 3: remove the existing role claim with the serialized array
                    userIdentity.TryRemoveClaim(userIdentity.Claims.FirstOrDefault(c => c.Type == userIdentity.RoleClaimType));

                    // Step 4: add each role separately
                    foreach (JsonElement element in roles.Value.EnumerateArray())
                    {
                        userIdentity.AddClaim(new Claim(userIdentity.RoleClaimType, element.GetString()));
                    }
                }
            }

            return userAccount;
        }
    }
}
