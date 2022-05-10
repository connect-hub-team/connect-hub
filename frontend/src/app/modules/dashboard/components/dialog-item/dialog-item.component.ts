import { Component, Input } from "@angular/core";
import { ChainDto } from "src/app/modules/shared/models/chain";

@Component({
  selector: 'dialog-item',
  templateUrl: './dialog-item.component.html',
  styleUrls: ['./dialog-item.component.scss']
})
export class DialogItemComponent {

  @Input() public dialog!: ChainDto;

  constructor() {

  }
}
