import { Component, Input, OnInit } from "@angular/core";

@Component({
  selector: 'avatar-image',
  styleUrls: ['./avatar-image.component.scss'],
  templateUrl: './avatar-image.component.html'
})
export class AvatarImageCompomemt implements OnInit {

  @Input() imageUrl: string = '';

  constructor() { }

  ngOnInit(): void {

  }
}
