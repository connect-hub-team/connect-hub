import { Component, OnInit } from '@angular/core';
import { UploadService } from 'src/app/modules/shared/upload.service';

@Component({
  selector: 'app-upload',
  templateUrl: './upload.component.html',
  styleUrls: ['./upload.component.scss']
})
export class UploadComponent implements OnInit {

  constructor(private upload: UploadService) { }

  ngOnInit(): void {
  }

  async onFileSelected(event: any) {
    console.log(event)
    const file = event.target.files[0]
    const result = await this.upload.upload(file)
    console.log(result)
  }
}
