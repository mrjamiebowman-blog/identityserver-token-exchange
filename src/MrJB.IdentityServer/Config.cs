using Duende.IdentityServer.Models;
using IdentityModel;

namespace MrJB.IdentityServer;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("scope1"),
            new ApiScope("scope2"),
        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            // m2m client credentials flow client
            new Client
            {
                ClientId = "m2m.client",
                ClientName = "Client Credentials Client",

                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets = { new Secret("8C593E98-4099-40B6-9A37-9A99E9984EEE".Sha256()) },

                AllowedScopes = { "scope1" }
            },

            // interactive client using code flow + pkce
            new Client
            {
                ClientId = "interactive",
                ClientSecrets = { new Secret("F611C82D-DA60-43CF-B2F0-E5D016BE902E".Sha256()) },
                    
                AllowedGrantTypes = GrantTypes.Code,

                RedirectUris = { "https://localhost:44300/signin-oidc" },
                FrontChannelLogoutUri = "https://localhost:44300/signout-oidc",
                PostLogoutRedirectUris = { "https://localhost:44300/signout-callback-oidc" },

                AllowOfflineAccess = true,
                AllowedScopes = { "openid", "profile", "scope2" }
            },

            /* token exchange */
            new Client {
                ClientId = "client-token-exchange",
                ClientSecrets = { new Secret("secret".Sha256()) },
                ClientName = "Token Exchange (Client)",
                AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
                RedirectUris = {
                    "http://localhost:5473/signin-oidc"
                },
                PostLogoutRedirectUris = {
                    "http://localhost:5473/signin-oidc"
                },
                // scopes that client has access to
                AllowedScopes = {
                    "api1",
                    "api2",
                    "openid",
                    "profile",
                    "email",
                    "verification"
                },
                AllowOfflineAccess = true,
                EnableLocalLogin = false,
                AccessTokenType = AccessTokenType.Reference,
                RequirePkce = true
            },
            
            /* token exchange for apim demo */
            new Client {
                ClientId = "client-apim-token-exchange",
                ClientSecrets = { new Secret("secret".Sha256()) },
                ClientName = "Token Exchange (APIM)",
                AllowedGrantTypes = new List<string> { 
                    OidcConstants.GrantTypes.TokenExchange
                },
                RedirectUris = {
                    "http://localhost:5473/signin-oidc"
                },
                PostLogoutRedirectUris = {
                    "http://localhost:5473/signin-oidc"
                },
                // scopes that client has access to
                AllowedScopes = {
                    "api1",
                    "api2",
                    "openid",
                    "profile",
                    "email",
                    "verification"
                },
                AllowOfflineAccess = true,
                EnableLocalLogin = false,
                AccessTokenType = AccessTokenType.Jwt,
                RequirePkce = true
            },
        };
}
