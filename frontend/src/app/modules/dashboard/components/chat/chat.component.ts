import { Component, Input } from "@angular/core";

@Component({
  selector: 'chat',
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.scss']
})
export class ChatComponent {

  @Input() dialog: any = {
    messages: [
      {
        author: 'Иванов Иван Иванович',
        text: 'Hello world!',
        sendAt: new Date()
      },
      {
        author: 'Иванов Иван Иванович',
        text: 'Hello world!',
        sendAt: new Date()
      },
      {
        author: 'Иванов Иван Иванович',
        text: 'Hello world!',
        sendAt: new Date()
      },
      {
        author: 'Иванов Иван Иванович',
        text: 'Hello world!',
        sendAt: new Date()
      },
    ]
  }

  constructor() {

  }
}
