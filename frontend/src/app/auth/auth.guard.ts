import { Injectable } from "@angular/core";
import { ActivatedRouteSnapshot, CanActivate, RouterStateSnapshot } from "@angular/router";
import { OAuthService } from "angular-oauth2-oidc";
import { Observable, tap } from "rxjs";
import { AuthService } from "./auth.service";

@Injectable()
export class AuthGuard implements CanActivate {
  constructor(
    private authService: AuthService,
    private oauthService: OAuthService,
  ) { }

  canActivate(): boolean {
    return true;
    // return this.oauthService.hasValidAccessToken();
  }

}
