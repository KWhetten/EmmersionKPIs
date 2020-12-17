import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class LoginMessageService {
  private subject = new Subject<any>();

  sendMessage(isLoggedIn: boolean) {
    this.subject.next({ isLoggedIn });
  }

  clearMessages() {
    this.subject.next();
  }

  onMessage(): Observable<any> {
    return this.subject.asObservable();
  }
}
