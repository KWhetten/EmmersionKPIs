import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class FilterMessageService {
  private subject = new Subject<any>();

  sendMessage(startDate: any, finishDate: any, product, engineering, unanticipated) {
    this.subject.next({ startDate, finishDate, product, engineering, unanticipated });
  }

  clearMessages() {
    this.subject.next();
  }

  onMessage(): Observable<any> {
    return this.subject.asObservable();
  }
}
