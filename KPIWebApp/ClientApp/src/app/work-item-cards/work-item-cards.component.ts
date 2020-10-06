import {Component, Inject} from "@angular/core";
import {HttpClient} from "@angular/common/http";
import {DatePipe} from "@angular/common";

@Component({
  selector: "app-work-item-cards-component",
  templateUrl: "./work-item-cards.component.html",
  styleUrls: ["./work-item-cards.component.css"],
  providers: [DatePipe]
})
export class WorkItemCardsComponent {

  constructor(http: HttpClient, @Inject("BASE_URL") baseUrl: string, datepipe: DatePipe) {

  }
}
