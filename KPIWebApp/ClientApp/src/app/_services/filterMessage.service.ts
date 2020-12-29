import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class FilterMessageService {
  private subject = new Subject<any>();

  sendMessage(startDate: any, finishDate: any, product, engineering, unanticipated, assessmentsTeam, enterpriseTeam) {
    this.subject.next({ startDate, finishDate, product, engineering, unanticipated, assessmentsTeam, enterpriseTeam });
  }

  clearMessages() {
    this.subject.next();
  }

  onMessage(): Observable<any> {
    return this.subject.asObservable();
  }
}
