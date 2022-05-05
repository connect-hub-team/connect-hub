import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { AppComponent } from "./app.component";
import { AuthGuard } from "./auth/auth.guard";
import { FirstComponentComponent } from "./components/first-component/first-component.component";
import { SecondComponentComponent } from "./components/second-component/second-component.component";
import { UploadComponent } from "./components/upload/upload.component";

const ROUTES: Routes = [
  {
    path: '',
    canActivate: [AuthGuard],
    children: [
      { path: 'first-component', component: FirstComponentComponent },
      { path: 'second-component', component: SecondComponentComponent },
      { path: 'upload', component: UploadComponent },
    ]
  }
];

@NgModule({
  imports: [RouterModule.forRoot(ROUTES)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
