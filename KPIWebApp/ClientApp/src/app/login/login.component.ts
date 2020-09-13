import {Component, Inject} from "@angular/core";
import {HttpClient} from "@angular/common/http";

@Component({
  selector: "app-login-component",
  templateUrl: "./login.component.html",
  styleUrls: ["./login.component.css"]
})
export class LoginComponent {

  constructor(http: HttpClient, @Inject("BASE_URL") baseUrl: string) {

  }
}
