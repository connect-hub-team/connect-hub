import { Injectable } from "@angular/core";
import { Router } from "@angular/router";
import { OAuthService, OAuthStorage } from "angular-oauth2-oidc";
import { BehaviorSubject, combineLatest, map } from "rxjs";
import { authConfig } from "./auth.config";

@Injectable({ providedIn: 'root' })
export class AuthService {
  private isAuthenticatedSubject$ = new BehaviorSubject<boolean>(false);
  public isAuthenticated$ = this.isAuthenticatedSubject$.asObservable();

  private isDoneLoadingSubject$ = new BehaviorSubject<boolean>(false);
  public isDoneLoading$ = this.isDoneLoadingSubject$.asObservable();

  public canActivateProtectedRoute$ = combineLatest([
    this.isAuthenticated$,
    this.isDoneLoading$
  ]).pipe(map(values => values.every(b => b)))

  constructor(
    private oauthService: OAuthService,
    private oauthStorage: OAuthStorage,
    private router: Router,
  ) {
    this.oauthService.configure(authConfig);
    this.oauthService.events.subscribe(_ =>
      this.isAuthenticatedSubject$.next(
        this.oauthService.hasValidAccessToken())
    );

    this.oauthService.setupAutomaticSilentRefresh();
  }

  public async init(url: string): Promise<void> {
    return this.oauthService.loadDiscoveryDocument()
      .then(() => this.oauthService.tryLogin())
      .then(() => {
        if (this.oauthService.hasValidAccessToken())
          return Promise.resolve();

        return this.oauthService.silentRefresh()
          .then(() => Promise.resolve())
          .catch(error => {
            if (error?.reason?.error == 'login_required')
              this.oauthService.initCodeFlow(encodeURIComponent(url));
            else
              throw error;

            return Promise.reject(error);
          })
      })
      .then(() => {
        this.isDoneLoadingSubject$.next(true);

        if (this.oauthService?.state !== 'undefined' &&
          this.oauthService?.state !== 'null')
          this.router.navigateByUrl(this.oauthService.state!);
      })
      .catch(_ => this.isDoneLoadingSubject$.next(true));
  }

  public get accountId(): string | null {
    if (this.oauthService.hasValidAccessToken()) {
      let claims = this.oauthService.getIdentityClaims() as any;
      return claims?.name;
    }
    return null;
  }

  public get authTail(): string {
    const accessToken = this.oauthStorage.getItem('access_token');
    return `?bearer=${accessToken}`;
  }
}
