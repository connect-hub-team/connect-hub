import { Injectable } from "@angular/core";
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot, UrlTree } from "@angular/router";
import { OAuthService } from "angular-oauth2-oidc";
import { Observable } from "rxjs";
import { AuthService } from "./auth.service";

// @Injectable()
// export class AuthGuard implements CanActivate {
//   constructor(private authService: AuthService) { }

//   canActivate(): Observable<boolean> {
//     return this.authService.canActivateProtectedRoute$;
//   }
// }

@Injectable()
export class AuthGuard implements CanActivate {
  constructor(
    private authService: AuthService,
  ) { }

  canActivate() {
    return this.authService.isAuthenticated$.asObservable();
  }
}
