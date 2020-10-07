import {HttpClient} from '@angular/common/http';
import {Component, Inject, OnInit} from '@angular/core';
import * as bcrypt from 'bcryptjs';
import {getCookie} from '../app.component';
import {Router} from '@angular/router';

@Component({
  selector: 'app-change-password-component',
  templateUrl: './change-password.component.html',
  styleUrls: ['./change-password.component.css']
})

export class ChangePasswordComponent implements OnInit {
  baseUrl: string;

  oldPassword: string;
  password: string;
  confirmPassword: string;

  passwordError: string = '';
  confirmPasswordError: string = '';
  private regexp: RegExp;
  private http: HttpClient;
  error: string;
  private email: string;
  oldPasswordError: string;

  constructor(private router:Router, http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.baseUrl = baseUrl;
    this.http = http;
  }

  ngOnInit() {
    let cookieValue = getCookie();
    if (cookieValue == undefined) {
      this.router.navigate(['/login']);
    }
  }

  async submit() {
    //this.email = (document.getElementById('email') as HTMLInputElement).value;
    this.oldPassword = await bcrypt.hash((document.getElementById('old-password') as HTMLInputElement).value, 10);
    this.password = await bcrypt.hash((document.getElementById('password') as HTMLInputElement).value, 10);
    this.confirmPassword = await bcrypt.hash((document.getElementById('password') as HTMLInputElement).value, 10);
    let data = { email: this.email, password: this.password };

    if (this.NoFieldsAreBlank() && this.PasswordValid() && await this.PasswordsMatch()) {
      this.http.post<any>(this.baseUrl + 'change-password', data).subscribe(
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
    (document.getElementById('old-password-error') as HTMLInputElement).hidden = true;
    (document.getElementById('password-error') as HTMLInputElement).hidden = true;
    (document.getElementById('confirm-password-error') as HTMLInputElement).hidden = true;

    (document.getElementById('old-password') as HTMLInputElement).value = '';
    (document.getElementById('password') as HTMLInputElement).value = '';
    (document.getElementById('confirm-password') as HTMLInputElement).value = '';
  }

  async PasswordsMatch() {
    if (await bcrypt.compare((document.getElementById('confirm-password') as HTMLInputElement).value, this.password)) {
      if (this.confirmPasswordError == '' || this.confirmPasswordError == 'Passwords don\'t match!') {
        (document.getElementById('confirm-password-error') as HTMLElement).hidden = true;
      }
      return true;
    } else {
      (document.getElementById('confirm-password-error') as HTMLElement).hidden = false;
      this.confirmPasswordError = 'Passwords don\'t match!';
      return false;
    }
  }

  NoFieldsAreBlank() {
    let flag = true;
    this.oldPassword = (document.getElementById('old-password-error') as HTMLInputElement).value;
    this.password = (document.getElementById('password-error') as HTMLInputElement).value;
    this.confirmPassword = (document.getElementById('confirm-password-error') as HTMLInputElement).value;

    if (this.oldPassword == undefined) {
      (document.getElementById('old-password-error') as HTMLElement).hidden = false;
      this.oldPasswordError = 'Please enter your current password';
      flag = false;
    } else {
      (document.getElementById('password-error') as HTMLElement).hidden = true;
    }

    if (this.password == undefined) {
      (document.getElementById('password-error') as HTMLElement).hidden = false;
      this.passwordError = 'Please enter a new password';
      flag = false;
    } else {
      (document.getElementById('password-error') as HTMLElement).hidden = true;
    }

    if (this.confirmPassword == undefined) {
      (document.getElementById('confirm-password-error') as HTMLElement).hidden = false;
      this.confirmPasswordError = 'Please confirm your new password';
      flag = false;
    } else {
      (document.getElementById('confirm-password-error') as HTMLElement).hidden = true;
    }
    return flag;
  }

  PasswordValid() {
    this.regexp = new RegExp('^(?=.*[0-9])(?=.*[A-Z]).{8,32}$');

    if (this.regexp.test((document.getElementById('password') as HTMLInputElement).value)) {
      (document.getElementById('password-error') as HTMLElement).hidden = true;
      return true;
    } else {
      (document.getElementById('password-error') as HTMLElement).hidden = false;
      this.passwordError = 'Password must be at least 8 characters long and contain the following:\n' +
        'At least one digit and at least one uppercase character';
      return false;
    }
  }
}
