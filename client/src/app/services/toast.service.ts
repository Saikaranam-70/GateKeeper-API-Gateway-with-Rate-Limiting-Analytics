import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

export interface ToastMessage {
  text: string;
  type: 'success' | 'error' | 'info';
}

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  private toastSubject = new Subject<ToastMessage>();
  public toast$ = this.toastSubject.asObservable();

  showSuccess(text: string): void {
    this.toastSubject.next({ text, type: 'success' });
  }

  showError(text: string): void {
    this.toastSubject.next({ text, type: 'error' });
  }

  showInfo(text: string): void {
    this.toastSubject.next({ text, type: 'info' });
  }
}
