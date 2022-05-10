import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { AuthGuard } from "./auth/auth.guard";

const ROUTES: Routes = [
  {
    path: '',
    canActivate: [AuthGuard],
    children: [
      {
        path: 'debug',
        loadChildren: () => import('./modules/debug/debug.module')
          .then(m => m.DebugModule)
      },
      {
        path: 'dashboard',
        loadChildren: () => import('./modules/dashboard/dashboard.module')
          .then(m => m.DashboardModule)
      }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forRoot(ROUTES)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
