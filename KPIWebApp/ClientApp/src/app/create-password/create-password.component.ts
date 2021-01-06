import {HttpClient} from '@angular/common/http';
import {Router, ActivatedRoute, Params} from '@angular/router';
import * as bcrypt from 'bcryptjs';
import {Component, Inject, OnInit} from '@angular/core';

@Component({
  selector: 'app-create-password-component',
  templateUrl: './create-password.component.html',
  styleUrls: ['./create-password.component.css']
})

export class CreatePasswordComponent implements OnInit {
  baseUrl: string;

  password: string;
  confirmPassword: string;

  passwordError: string = '';
  confirmPasswordError: string = '';
  private regexp: RegExp;
  private http: HttpClient;
  error: string;
  private email: string;
  success: string;


  constructor(private activatedRoute: ActivatedRoute, private router:Router, http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.baseUrl = baseUrl;
    this.http = http;
  }

  ngOnInit() {
    this.activatedRoute.queryParams.subscribe(params => {
        this.email = params['email'];
    });
  }

  async submit() {
    this.password = await bcrypt.hash((document.getElementById('password') as HTMLInputElement).value, 10);
    let data = { email: this.email, password: this.password };

    if (await this.NoFieldsAreBlank() && this.PasswordValid() && await this.PasswordsMatch()) {

      this.http.post<any>(this.baseUrl + 'change-password', data).subscribe(
        (result) => {
          (document.getElementById('success') as HTMLElement).hidden = false;
          (document.getElementById('general-error') as HTMLElement).hidden = true;
          this.router.navigateByUrl('login');
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
    (document.getElementById('password-error') as HTMLInputElement).hidden = true;
    (document.getElementById('confirm-password-error') as HTMLInputElement).hidden = true;

    (document.getElementById('password') as HTMLInputElement).value = '';
    (document.getElementById('confirm-password') as HTMLInputElement).value = '';
  }

  async PasswordsMatch() {
    let comparePassword = (document.getElementById('confirm-password') as HTMLInputElement).value
    if (await bcrypt.compare(comparePassword, this.password)) {
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

  async NoFieldsAreBlank() {
    let flag = true;

    this.confirmPassword = (document.getElementById("confirm-password") as HTMLInputElement).value;

    if (this.password == '') {
      (document.getElementById('password-error') as HTMLElement).hidden = false;
      this.passwordError = 'Please enter a new password';
      flag = false;
    } else {
      (document.getElementById('password-error') as HTMLElement).hidden = true;
    }

    if (this.confirmPassword == '') {
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
      this.passwordError = 'Password must be at least 8 characters long and contain at least one digit and at least one uppercase character';
      return false;
    }
  }
}
