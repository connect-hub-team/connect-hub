import { NgModule } from '@angular/core';

import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { AppRoutingModule } from './app-routing.module';
import { AuthService } from './auth/auth.service';
import { OAuthModule } from 'angular-oauth2-oidc';
import { AuthGuard } from './auth/auth.guard';
import { HttpClientModule } from '@angular/common/http';
import { DebugModule } from './modules/debug/debug.module';
import { SharedModule } from './modules/shared/shared.module';
import { DashboardModule } from './modules/dashboard/dashboard.module';

@NgModule({
  declarations: [
    AppComponent,
  ],

  imports: [
    SharedModule,
    HttpClientModule,
    OAuthModule.forRoot(),
    BrowserAnimationsModule,
    AppRoutingModule,
    DebugModule,
    DashboardModule,
  ],

  providers: [
    AuthService,
    AuthGuard,
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
