import {Component, Inject} from "@angular/core";
import {HttpClient} from "@angular/common/http";
import * as bcrypt from "bcryptjs";

@Component({
  selector: "app-register-component",
  templateUrl: "./register.component.html",
  styleUrls: ["./register.component.css"]
})
export class RegisterComponent {
  baseUrl: string;

  firstName: string;
  lastName: string;
  email: string;
  password: string;
  confirmPassword: string;

  firstNameError: string = "";
  lastNameError: string = "";
  emailError: string = "";
  passwordError: string = "";
  confirmPasswordError: string = "";
  private regexp: RegExp;
  private http: HttpClient;

  constructor(http: HttpClient, @Inject("BASE_URL") baseUrl: string) {
    this.baseUrl = baseUrl;
    this.http = http;
  }

  async submit() {

    this.firstName = (document.getElementById("first-name") as HTMLInputElement).value;
    this.lastName = (document.getElementById("last-name") as HTMLInputElement).value;
    this.email = (document.getElementById("email") as HTMLInputElement).value;
    this.password = await bcrypt.hash((document.getElementById("password") as HTMLInputElement).value, 10);
    let data = { firstName: this.firstName, lastName: this.lastName, email: this.email, password: this.password};

    if (this.NoFieldsAreBlank() && this.PasswordValid() && await this.PasswordsMatch() && this.EmailValid()) {
      this.http.post<any>(this.baseUrl + "register", data).subscribe(
        (res) => console.log(res),
        (err) => console.log(err)
      );
    }
  }

  EmailValid() {
    this.regexp = new RegExp(/^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/);

    if (this.regexp.test((document.getElementById("email") as HTMLInputElement).value)) {
      (document.getElementById("email-error") as HTMLInputElement).hidden = true;
    } else {
      (document.getElementById("email-error") as HTMLInputElement).hidden = false;
      this.emailError = "Invalid email.";
      return false;
    }
    if (this.email.indexOf("@emmersion.ai") >= 0) {
      (document.getElementById("email-error") as HTMLInputElement).hidden = true;
    } else {
      (document.getElementById("email-error") as HTMLInputElement).hidden = false;
      this.emailError = "Email must be in the @emmersion.ai domain.";
      return false;
    }
    return true;
  }

  async PasswordsMatch() {
    if (await bcrypt.compare((document.getElementById("confirm-password") as HTMLInputElement).value, this.password)) {
      if (this.confirmPasswordError == "" || this.confirmPasswordError == "Passwords don't match!") {
        (document.getElementById("confirm-password-error") as HTMLInputElement).hidden = true;
      }
      return true;
    } else {
      (document.getElementById("confirm-password-error") as HTMLInputElement).hidden = false;
      this.confirmPasswordError = "Passwords don't match!";
      return false;
    }
  }

  NoFieldsAreBlank() {
    let flag = true;
    if (this.firstName == "") {
      (document.getElementById("first-name-error") as HTMLInputElement).hidden = false;
      this.firstNameError = "Please enter a first name";
      flag = false;
    } else {
      (document.getElementById("first-name-error") as HTMLInputElement).hidden = true;
    }

    if (this.lastName == "") {
      (document.getElementById("last-name-error") as HTMLInputElement).hidden = false;
      this.lastNameError = "Please enter a last Name";
      flag = false;
    } else {
      (document.getElementById("last-name-error") as HTMLInputElement).hidden = true;
    }

    if (this.email == "") {
      (document.getElementById("email-error") as HTMLInputElement).hidden = false;
      this.emailError = "Please enter an email";
      flag = false;
    } else {
      (document.getElementById("email-error") as HTMLInputElement).hidden = true;
    }

    if (this.password == "") {
      (document.getElementById("password-error") as HTMLInputElement).hidden = false;
      this.passwordError = "Please enter a password";
      flag = false;
    } else {
      (document.getElementById("password-error") as HTMLInputElement).hidden = true;
    }

    if (this.confirmPassword == "") {
      (document.getElementById("confirm-password-error") as HTMLInputElement).hidden = false;
      this.confirmPasswordError = "Please confirm your password";
      flag = false;
    } else {
      (document.getElementById("confirm-password-error") as HTMLInputElement).hidden = true;
    }
    return flag;
  }

  cancel() {
    (document.getElementById("first-name-error") as HTMLInputElement).hidden = true;
    (document.getElementById("last-name-error") as HTMLInputElement).hidden = true;
    (document.getElementById("email-error") as HTMLInputElement).hidden = true;
    (document.getElementById("password-error") as HTMLInputElement).hidden = true;
    (document.getElementById("confirm-password-error") as HTMLInputElement).hidden = true;

    (document.getElementById("first-name") as HTMLInputElement).value = "";
    (document.getElementById("last-name") as HTMLInputElement).value = "";
    (document.getElementById("password") as HTMLInputElement).value = "";
    (document.getElementById("confirm-password") as HTMLInputElement).value = "";
  }

  PasswordValid() {
    this.regexp = new RegExp("^(?=.*[0-9])(?=.*[A-Z]).{8,32}$");

    if (this.regexp.test((document.getElementById("password") as HTMLInputElement).value)) {
      (document.getElementById("password-error") as HTMLInputElement).hidden = true;
      return true;
    } else {
      (document.getElementById("password-error") as HTMLInputElement).hidden = false;
      this.passwordError = "Password must be at least 8 characters long and contain the following:\n" +
        "At least one digit and at least one uppercase character";
      return false;
    }
  }
}
