import {Component, Inject} from "@angular/core";
import {HttpClient} from "@angular/common/http";

@Component({
  selector: "app-overview-component",
  templateUrl: "./overview.component.html",
  styleUrls: ["./overview.component.css"]
})
export class OverviewComponent {
  public overviewData: OverviewData;
  public startDate;
  public endDate;

  constructor(http: HttpClient, @Inject("BASE_URL") baseUrl: string) {
    http.get<OverviewData>(baseUrl + '').subscribe(x => this.overviewData = x);
    this.startDate = document.getElementById("start-date");
    this.endDate = document.getElementById("end-date");

  }

  submit() {
    this.startDate = document.getElementById("start-date");
    this.endDate = document.getElementById("end-date");
  }
}

interface OverviewData {
  AverageLeadTime: number;
  LongestLeadTime: number;
  ShortestLeadTime: number;
  TotalDeploys: number;
  SuccessfulDeploys: number;
  RolledBackDeploys: number;
  DeployFrequency: number;
  MeanTimeToRestore: number;
  ChangeFailPercentage: number;
}
