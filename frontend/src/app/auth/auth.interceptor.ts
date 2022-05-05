import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { OAuthResourceServerErrorHandler, OAuthStorage } from "angular-oauth2-oidc";
import { catchError, Observable } from "rxjs";

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(
    private authStorage: OAuthStorage,
    private errorHandler: OAuthResourceServerErrorHandler,
  ) { }

  private isReqInterceptable(req: HttpRequest<any>): boolean {
    return true; // TODO:
  }

  intercept(req: HttpRequest<any>, next: HttpHandler)
    : Observable<HttpEvent<any>> {
    const headers = req.headers
      .set('Cache-Control', 'no-cache')
      .set('Pragma', 'no-cache')
      .set('Expires', 'Sat, 01 Jan 2000 00:00:00 GMT');

    req = req.clone({ headers });

    if (!this.isReqInterceptable(req))
      return next.handle(req);

    const token = this.authStorage.getItem('access_token');

    if (token) {
      const header = `Bearer ${token}`;
      const headers = req.headers.set('Authorization', header);
      req = req.clone({ headers });
    } else {
      console.error('auth', 'can\'t find access token');
    }

    return next.handle(req)
      .pipe(catchError(err => this.errorHandler.handleError(err)));
  }
}
