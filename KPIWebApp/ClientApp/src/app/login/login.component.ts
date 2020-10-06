import {Component, Inject, OnInit} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import * as bcrypt from 'bcryptjs';
import {Router} from '@angular/router';
import {NavMenuComponent} from '../../components/nav-menu/nav-menu.component';

@Component({
  selector: 'app-login-component',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  email: string;
  password: string;
  emailError: string = '';
  passwordError: string = '';
  private http: HttpClient;
  baseUrl: string;
  error: string;

  constructor(private router: Router, http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.baseUrl = baseUrl;
    this.http = http;
  }

  ngOnInit() {
    const date = new Date();
    date.setTime(date.getTime() + (-1 * 24 * 60 * 60 * 1000));

    document.cookie = "Authorized=; expires="+date.toUTCString()+"; path=/";

    this.router.navigateByUrl('/RefreshComponent', { skipLocationChange: true }).then(() => {
      this.router.navigate(['NavMenuComponent']);
    });
  }

  async submit() {
    this.email = (document.getElementById('email') as HTMLInputElement).value;
    this.password = await bcrypt.hash((document.getElementById('password') as HTMLInputElement).value, 10);

    if (this.noBlankFields()) {
      let data = {Email: this.email}

      this.http.post<any>(this.baseUrl + 'login-user', data).subscribe(
        (result) => {
          bcrypt.compare((document.getElementById('password') as HTMLInputElement).value, result['password'], (err, bcryptResult) => {
            if(bcryptResult) {
              this.setCookie("Authorized", result["guid"])
              this.router.navigateByUrl('');
            } else if (result['email'] == '' || result['password'] == '') {
              this.emailError = 'That user does not exist in our system. Please contact your supervisor';
              (document.getElementById('email-error') as HTMLElement).hidden = false;
            } else {
              this.passwordError = 'The password you entered was incorrect. Try again.';
              (document.getElementById('password-error') as HTMLElement).hidden = false;
            }
          });
        },
        (error) => {
          this.error = 'An error prevented you from logging in at this time. Please try again later.';
          (document.getElementById('general-error') as HTMLElement).hidden = false;
        }
      );
    }
  }
  setCookie(name: string, val: string) {
    const date = new Date();
    const value = val;

    // Set it expire in 2 hours
    date.setTime(date.getTime() + (2 * 60 * 60 * 1000));

    // Set it
    document.cookie = name+"="+value+"; expires="+date.toUTCString()+"; path=/";
  }

  private noBlankFields() {
    let flag = true;

    if (this.email == '') {
      (document.getElementById('email-error') as HTMLInputElement).hidden = false;
      this.emailError = 'Please enter a email';
      flag = false;
    } else {
      (document.getElementById('email-error') as HTMLInputElement).hidden = true;
    }

    if (this.password == '') {
      (document.getElementById('password-error') as HTMLInputElement).hidden = false;
      this.passwordError = 'Please enter a password';
      flag = false;
    } else {
      (document.getElementById('password-error') as HTMLInputElement).hidden = true;
    }
    return flag;
  }
}
