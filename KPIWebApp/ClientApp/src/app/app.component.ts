import { Component } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent {
  title = 'app';
}

export function getAuthorizedCookie() {
  const value = '; ' + document.cookie;
  const parts = value.split('; EmmersionAuthorized=');

  if (parts.length == 2) {
    return parts.pop().split(';').shift();
  }
}

export function getEmailCookie() {
  const value = '; ' + document.cookie;
  const parts = value.split('; EmmersionEmail=');

  if (parts.length == 2) {
    return parts.pop().split(';').shift();
  }
}
