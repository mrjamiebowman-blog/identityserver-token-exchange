using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;
using Duende.IdentityServer;
using IdentityModel;

namespace MrJB.IdentityServer.GrantFlow;

public class TokenExchangeFlow : IExtensionGrantValidator
{
    // logging
    private readonly ILogger<TokenExchangeFlow> _logger;

    private readonly IdentityServerTools _tools;
    private readonly ITokenValidator _validator;

    /// <summary>
    /// This grant type validates incoming reference tokens and returns a "JWT" token to the APIM.
    /// </summary>
    public string GrantType => OidcConstants.GrantTypes.TokenExchange;

    public TokenExchangeFlow(ILogger<TokenExchangeFlow> logger, IdentityServerTools tools, ITokenValidator validator)
    {
        _logger = logger;
        _tools = tools;
        _validator = validator;
    }

    public async Task ValidateAsync(ExtensionGrantValidationContext context)
    {
        _logger.LogInformation($"Client ID: {context.Request.ClientId} Using Token Exchange Grant Flow.");

        try
        {
            // default response is error
            context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest);

            // incoming token
            var subjectToken = context.Request.Raw.Get(OidcConstants.TokenRequest.SubjectToken);

            // token type
            var subjectTokenType = context.Request.Raw.Get(OidcConstants.TokenRequest.SubjectTokenType);

            // mandatory parameters
            if (String.IsNullOrWhiteSpace(subjectToken)) {
                return;
            }

            // for our impersonation/delegation scenario we require an access token
            if (!String.IsNullOrWhiteSpace(subjectToken)) {
                return;
            }

            // validate the incoming access token with the built-in token validator
            var validationResult = await _validator.ValidateAccessTokenAsync(subjectToken);

            if (validationResult == null || validationResult.IsError) {
                return;
            }

            // variables
            var clientId = validationResult.Claims?.First(x => x.Type == JwtClaimTypes.ClientId).Value;

            // scopes
            var scopes = validationResult.Claims?.Where(x => x.Type == JwtClaimTypes.Scope)
                                                            .Select(x => x.Value)
                                                            .ToList();

            // override scopes
            context.Request.RequestedScopes = scopes;

            // parsed scopes
            var parsedScopes = new HashSet<ParsedScopeValue>();

            foreach (var scope in scopes)
            {
                parsedScopes.Add(new ParsedScopeValue(scope));
            }
            
            context.Request.ValidatedResources.ParsedScopes = parsedScopes;

            // reference token expiration
            var refTokenExpiration = validationResult.Claims?.SingleOrDefault(x => x.Type == JwtClaimTypes.Expiration)!.Value;

            // the spec allows for various token types, most commonly your return an access token
            var customResponse = new Dictionary<string, object>
                {
                    { OidcConstants.TokenResponse.IssuedTokenType, OidcConstants.TokenTypeIdentifiers.Jwt },
                    { "reference_token_expires_in", refTokenExpiration }
                };

            // request context
            context.Request.ClientId = clientId;

            // set validation result
            context.Result = new GrantValidationResult(customResponse: customResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message, null);
            context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, errorDescription: ex.Message);
        }
    }
}
