import {HttpClient} from '@angular/common/http';
import {Component, Inject, OnInit} from '@angular/core';
import {Router} from '@angular/router';

@Component({
  selector: 'app-forgot-password-component',
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.css']
})

export class ForgotPasswordComponent implements OnInit {
  baseUrl: string;

  private regexp: RegExp;
  private http: HttpClient;
  error: string;
  private email: string;
  emailError: string;

  constructor(private router: Router, http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.baseUrl = baseUrl;
    this.http = http;
  }

  ngOnInit() {
  }

  async submit() {
    this.email = (document.getElementById('email') as HTMLInputElement).value;
    let data = {email: this.email};

    if (this.NoFieldsAreBlank() && this.EmailValid()) {
      this.http.post<any>(this.baseUrl + 'forgot-password', data).subscribe(
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

  cancel() {
    (document.getElementById('email-error') as HTMLInputElement).hidden = true;

    (document.getElementById('email') as HTMLInputElement).value = '';
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

    this.email = (document.getElementById('email') as HTMLInputElement).value;

    if (this.email == '') {
      (document.getElementById('email-error') as HTMLElement).hidden = false;
      this.emailError = 'Please enter an email';
      flag = false;
    } else {
      (document.getElementById('email-error') as HTMLElement).hidden = true;
    }

    return flag;
  }
}
