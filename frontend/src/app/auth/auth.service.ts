import { Injectable } from "@angular/core";
import { OAuthService } from "angular-oauth2-oidc";
import { JwksValidationHandler } from "angular-oauth2-oidc-jwks";
import { filter, map, Subject } from "rxjs";
import { authConfig } from "./auth.config";

@Injectable({ providedIn: 'root' })
export class AuthService {
  public isAuthenticated$ = new Subject<boolean>();

  constructor(
    private oauthService: OAuthService
  ) {
    this.oauthService.configure(authConfig);
  }

  async init() {
    console.log('loading...');

    this.oauthService.events
      .pipe(
        map(ev => {
          console.log(ev);
          return ev;
        }),
        filter(ev => ev.type === 'token_received')
      ).subscribe(_ => {
        console.log('loaded');
        // oauthService.loadUserProfile();
        this.isAuthenticated$.next(true);
      });

    await this.oauthService.loadDiscoveryDocumentAndLogin({
      customHashFragment: window.location.search
    });
  }
}
