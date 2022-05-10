import { Component, OnInit } from "@angular/core";
import { ChainDto } from "src/app/modules/shared/models/chain";
import { ChainType } from "src/app/modules/shared/models/chain-type.enum";

@Component({
  selector: 'dialogs-list',
  templateUrl: './dialogs-list.component.html',
  styleUrls: ['./dialogs-list.component.scss']
})
export class DialogsListComponent implements OnInit {

  public dialogs!: ChainDto[];

  constructor() { }

  ngOnInit(): void {
    this.dialogs = [
      new ChainDto(
        'Test dialog',
        'https://cdn.sstatic.net/Img/teams/teams-illo-free-sidebar-promo.svg?v=47faa659a05e',
        ChainType.Group,
        []),
      new ChainDto(
        'Test dialog',
        'https://cdn.sstatic.net/Img/teams/teams-illo-free-sidebar-promo.svg?v=47faa659a05e',
        ChainType.Group,
        []),
      new ChainDto(
        'Test dialog',
        'https://cdn.sstatic.net/Img/teams/teams-illo-free-sidebar-promo.svg?v=47faa659a05e',
        ChainType.Group,
        []),
      new ChainDto(
        'Test dialog',
        'https://cdn.sstatic.net/Img/teams/teams-illo-free-sidebar-promo.svg?v=47faa659a05e',
        ChainType.Group,
        []),
    ];
    console.log(this.dialogs);
  }


}
