import {Component, Inject} from "@angular/core";
import {HttpClient} from "@angular/common/http";

@Component({
  selector: "app-login-component",
  templateUrl: "./login.component.html",
  styleUrls: ["./login.component.css"]
})
export class LoginComponent {
  email: string;
  password: string;
  emailError: string = "";
  passwordError: string = "";

  constructor(http: HttpClient, @Inject("BASE_URL") baseUrl: string) {

  }

  submit() {
    this.email = (document.getElementById("email") as HTMLInputElement).value;
    this.password = (document.getElementById("password") as HTMLInputElement).value;

    let flag = false;

    if (this.email == "") {
      (document.getElementById("email-error") as HTMLInputElement).hidden = false;
      this.emailError = "Please enter a email";
      flag = true;
    }
    else{
      (document.getElementById("email-error") as HTMLInputElement).hidden = true;
    }

    if(this.password == ""){
      (document.getElementById("password-error") as HTMLInputElement).hidden = false;
      this.passwordError = "Please enter a password";
      flag = true;
    }
    else{
      (document.getElementById("password-error") as HTMLInputElement).hidden = true;
    }
    if(!flag){
      // submit stuff
    }
  }
}
