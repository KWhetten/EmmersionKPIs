import { Component } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent {
  title = 'app';
}

export function getCookie() {
  const value = '; ' + document.cookie;
  const parts = value.split('; Authorized=');

  if (parts.length == 2) {
    return parts.pop().split(';').shift();
  }
}
