import {Component, Inject} from "@angular/core";
import {HttpClient} from "@angular/common/http";
import {DatePipe} from "@angular/common";

@Component({
  selector: "app-overview-component",
  templateUrl: "./overview.component.html",
  styleUrls: ["./overview.component.css"]
})
export class OverviewComponent {
  public overviewData: OverviewData;

  constructor(http: HttpClient, @Inject("BASE_URL") baseUrl: string) {
    http.get<OverviewData>(baseUrl + "overview").subscribe(x => {
      this.overviewData = x;
    });

    console.log(this.overviewData);
  }

  submit() {
  }
}

interface OverviewData {
  averageLeadTime: number;
  longestLeadTime: number;
  shortestLeadTime: number;
  totalDeploys: number;
  successfulDeploys: number;
  rolledBackDeploys: number;
  deployFrequency: number;
  meanTimeToRestore: number;
  changeFailPercentage: number;
}
