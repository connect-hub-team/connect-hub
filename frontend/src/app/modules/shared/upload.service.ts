import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UploadService {

  private prefix = 'file/';

  constructor(private http: HttpClient) { }

  upload(file: File) {
    const formData = new FormData();
    formData.append('file1', file)
    return firstValueFrom(this.http.post(this.prefix + '/upload', formData))
  }
}
