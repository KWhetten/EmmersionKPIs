import {Component, Inject, OnInit} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {Router} from '@angular/router';
import {getCookie} from '../app.component';

@Component({
  selector: 'app-invite-user-component',
  templateUrl: './invite-user.component.html',
  styleUrls: ['./invite-user.component.css']
})
export class InviteUserComponent implements OnInit {
  baseUrl: string;

  firstName: string;
  lastName: string;
  email: string;

  firstNameError: string = '';
  lastNameError: string = '';
  emailError: string = '';
  private regexp: RegExp;
  private http: HttpClient;
  error: string;

  constructor(private router: Router, http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.baseUrl = baseUrl;
    this.http = http;
  }

  ngOnInit() {
    // let cookieValue = getCookie();
    // if (cookieValue == undefined) {
    //   this.router.navigate(['/login']);
    // }
  }

  async submit() {

    this.firstName = (document.getElementById('first-name') as HTMLInputElement).value;
    this.lastName = (document.getElementById('last-name') as HTMLInputElement).value;
    this.email = (document.getElementById('email') as HTMLInputElement).value;
    let data = {firstName: this.firstName, lastName: this.lastName, email: this.email, baseUrl: this.baseUrl};

    if (this.NoFieldsAreBlank() && this.EmailValid()) {
      this.http.post<any>(this.baseUrl + 'register', data).subscribe(
        (result) => {
          console.log('Result: ' + result);
          (document.getElementById('general-error') as HTMLElement).hidden = true;
          (document.getElementById('success') as HTMLElement).hidden = false;
          result.body;
        },
        (error) => {
          this.error = error.error;
          (document.getElementById('general-error') as HTMLElement).hidden = false;
          (document.getElementById('success') as HTMLElement).hidden = true;
        }
      );
    }
  }

  EmailValid() {
    this.regexp = new RegExp(/^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/);

    if (this.regexp.test((document.getElementById('email') as HTMLInputElement).value)) {
      (document.getElementById('email-error') as HTMLElement).hidden = true;
    } else {
      (document.getElementById('email-error') as HTMLElement).hidden = false;
      this.emailError = 'Invalid email.';
      return false;
    }
    if (this.email.indexOf('@emmersion.ai') >= 0) {
      (document.getElementById('email-error') as HTMLElement).hidden = true;
    } else {
      (document.getElementById('email-error') as HTMLElement).hidden = false;
      this.emailError = 'Email must be in the @emmersion.ai domain.';
      return false;
    }
    return true;
  }

  NoFieldsAreBlank() {
    let flag = true;
    if (this.firstName == '') {
      (document.getElementById('first-name-error') as HTMLElement).hidden = false;
      this.firstNameError = 'Please enter a first name';
      flag = false;
    } else {
      (document.getElementById('first-name-error') as HTMLElement).hidden = true;
    }

    if (this.lastName == '') {
      (document.getElementById('last-name-error') as HTMLElement).hidden = false;
      this.lastNameError = 'Please enter a last Name';
      flag = false;
    } else {
      (document.getElementById('last-name-error') as HTMLElement).hidden = true;
    }

    if (this.email == '') {
      (document.getElementById('email-error') as HTMLElement).hidden = false;
      this.emailError = 'Please enter an email';
      flag = false;
    } else {
      (document.getElementById('email-error') as HTMLElement).hidden = true;
    }

    return flag;
  }

  cancel() {
    (document.getElementById('first-name-error') as HTMLInputElement).hidden = true;
    (document.getElementById('last-name-error') as HTMLInputElement).hidden = true;
    (document.getElementById('email-error') as HTMLInputElement).hidden = true;

    (document.getElementById('first-name') as HTMLInputElement).value = '';
    (document.getElementById('last-name') as HTMLInputElement).value = '';
    (document.getElementById('email') as HTMLInputElement).value = '';
  }
}
