import { AuthConfig } from "angular-oauth2-oidc";
import { environment } from "src/environments/environment";

export const authConfig: AuthConfig = {
  issuer: environment.authURL,
  redirectUri: window.location.origin + '/index.html',
  clientId: 'angular-client',
  responseType: 'code',
  scope: 'openid profile',
  clearHashAfterLogin: true,
}
