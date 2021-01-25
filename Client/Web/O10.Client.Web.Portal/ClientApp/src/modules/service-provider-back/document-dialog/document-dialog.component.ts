import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { HttpEventType, HttpClient } from '@angular/common/http';

export interface DialogData {
  documentName: string;
  hash: string;
}

@Component({
  selector: 'dialog-document',
  templateUrl: 'document-dialog.component.html',
})
export class DocumentDialog {
  public progress: number;
  public message: string;
  public isUploaded = false;
  public isUploading = false;

  constructor(private http: HttpClient,
    public dialogRef: MatDialogRef<DocumentDialog>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData) {
    this.isUploaded = false;
  }

  onCancelClick(): void {
    this.dialogRef.close();
  }

  public uploadFiles = (files) => {
    if (files.length === 0) {
      return;
    }

    this.isUploading = true;
    let fileToUpload = <File>files[0];
    const formData = new FormData();
    formData.append('file', fileToUpload, fileToUpload.name);
    this.data = {
      documentName: fileToUpload.name,
      hash: null
    };

    this.http.post('/ServiceProviders/CalculateFileHash', formData, { reportProgress: true, observe: 'events' })
      .subscribe(event => {
        if (event.type === HttpEventType.UploadProgress)
          this.progress = Math.round(100 * event.loaded / event.total);
        else if (event.type === HttpEventType.Response) {
          console.log("finished");
          this.message = 'Upload success.';
          this.isUploaded = true;
          this.isUploading = false;
          this.data = event.body as DialogData;
        }
      });
  }

}
