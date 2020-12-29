import {HttpClient, HttpHeaders} from '@angular/common/http';
import {Component, Inject, OnInit} from '@angular/core';
import * as bcrypt from 'bcryptjs';
import {getAuthorizedCookie} from '../app.component';
import {getEmailCookie} from '../app.component';
import {Router} from '@angular/router';
import {catchError} from 'rxjs/operators';

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
  authorized: boolean = false;
  httpOptions = {
  headers: new HttpHeaders({
    'Content-Type':  'application/json'
  })}
  private data: any;
  private url: string;

  constructor(private router: Router, http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.baseUrl = baseUrl;
    this.http = http;
  }

  ngOnInit() {
    let cookieValue = getAuthorizedCookie();
    this.http.get<boolean>(this.baseUrl + 'authorize-user', {
      params: {guid: cookieValue}
    })
      .subscribe((result) => {
        this.authorized = result;
        if (cookieValue == undefined || !this.authorized) {
          this.router.navigate(['/login']);
        }
      });
  }

  async submit() {
    this.oldPassword = await bcrypt.hash((document.getElementById('old-password') as HTMLInputElement).value, 10);
    this.password = await bcrypt.hash((document.getElementById('password') as HTMLInputElement).value, 10);
    this.confirmPassword = await bcrypt.hash((document.getElementById('password') as HTMLInputElement).value, 10);
    this.email = getEmailCookie();
    this.url = this.baseUrl + 'change-password';

    if (this.NoFieldsAreBlank() && this.PasswordValid() && await this.PasswordsMatch()) {
      return this.http.post<any>(this.url, { email: this.email, password: this.password }).subscribe(data => {
        this.data = data;
      })
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
