import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { RouterModule } from "@angular/router";
import { MaterialModule } from "../shared/material.module";
import { SharedModule } from "../shared/shared.module";
import { AvatarImageCompomemt } from "./components/avatar-image/avatar-image.component";
import { ChatComponent } from "./components/chat/chat.component";
import { DialogItemComponent } from "./components/dialog-item/dialog-item.component";
import { DialogsListComponent } from "./components/dialogs-list/dialogs-list.component";
import { DashboardComponent } from "./dashboard.component";

@NgModule({
  declarations: [
    DialogItemComponent,
    DialogsListComponent,
    ChatComponent,
    DashboardComponent,
    AvatarImageCompomemt,
  ],

  imports: [
    MaterialModule,
    CommonModule,
    SharedModule,
    RouterModule.forChild([{
      path: '',
      component: DashboardComponent,
    }])
  ],

  exports: [
    DashboardComponent,
  ],

  bootstrap: [DashboardComponent]
})
export class DashboardModule { }
