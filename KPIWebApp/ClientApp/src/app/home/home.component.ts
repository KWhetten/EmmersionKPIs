import {Component, Inject, OnInit} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {DatePipe} from '@angular/common';
import {Router} from '@angular/router';
import {getCookie} from '../app.component';

@Component({
  selector: 'app-home-component',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css'],
  providers: [DatePipe]
})
export class HomeComponent implements OnInit {
  public overviewData: OverviewData;
  protected http: HttpClient;
  protected baseUrl: string;
  private datePipe: DatePipe;
  startDate: string = 'The Beginning of Time';
  endDate: string = 'The Present Day';
  startTimer: any;

  constructor(private router: Router, datepipe: DatePipe, http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.startDate = 'The Beginning of Time';
    this.endDate = 'The Present Day';
    this.datePipe = datepipe;
    this.http = http;
    this.baseUrl = baseUrl;
    const formData: FormData = new FormData();
    formData.append('startDateString', '');
    formData.append('endDateString', '');
  }

  ngOnInit() {
    this.timeStart();
    let cookieValue = getCookie();
    if (cookieValue == undefined) {
      this.router.navigate(['/login']);
    }
    this.http.get<OverviewData>(this.baseUrl + 'overview')
      .subscribe(x => {
        this.overviewData = x;
        this.timeStop();
      });
  }

  reloadData() {
    this.overviewData = null;

    this.http.get<OverviewData>(this.baseUrl + 'overview', {
      params: {startDateString: this.startDate, endDateString: this.endDate}
    })
      .subscribe(x => {
        this.overviewData = x;
      });
  }

  submit() {
    this.startTimer = this.timeStart();

    this.overviewData = null;

    let dateValue = (document.getElementById('start-date') as HTMLInputElement).value
    if (dateValue != '') {
      let date = new Date(dateValue);
      this.startDate = this.datePipe.transform(new Date(date.getFullYear(), date.getMonth(), date.getDate() + 1), 'MMMM d, yyyy');
    }
    dateValue = (document.getElementById('end-date') as HTMLInputElement).value;
    if (dateValue != '') {
      let date = new Date(dateValue);
      this.endDate = this.datePipe.transform(new Date(date.getFullYear(), date.getMonth(), date.getDate() + 1), 'MMMM d, yyyy');
    }

    this.reloadData();
  }

  timeStart() {
    console.time('Page Load')
  }

  timeStop() {
    console.timeEnd('Page Load')
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
  totalCards: number;
}
