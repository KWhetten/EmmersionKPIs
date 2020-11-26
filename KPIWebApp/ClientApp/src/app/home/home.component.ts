import {Component, EventEmitter, Inject, OnInit, Output, ViewChild} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {DatePipe} from '@angular/common';
import {Router} from '@angular/router';
import {getCookie} from '../app.component';
import * as Highcharts from 'highcharts';

declare var require: any;
let Boost = require('highcharts/modules/boost');
let noData = require('highcharts/modules/no-data-to-display');
let More = require('highcharts/highcharts-more');

Boost(Highcharts);
noData(Highcharts);
More(Highcharts);
noData(Highcharts);

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
  finishDate: string = 'The Present Day';
  startTimer: any;
  @Output() submitted = new EventEmitter<any>();
  isExpanded = false;

  public cumulativeFlowOptions: any = {
    chart: {
      type: 'area',
      height: 495
    },
    title: {
      text: 'Task Items Flow'
    },
    xAxis: {
      categories: [],
      tickmarkPlacement: 'on',
      title: {
        enabled: false
      }
    },
    yAxis: {
      title: {
        text: 'Number of Tasks'
      },
      labels: {
        formatter: function () {
          return this.value;
        }
      }
    },
    tooltip: {
      split: true
    },
    plotOptions: {
      area: {
        stacking: 'normal',
        lineColor: '#666666',
        lineWidth: 1,
        marker: {
          lineWidth: 1,
          lineColor: '#666666'
        }
      }
    },
    series: []
  }
  constructor(protected router: Router, datepipe: DatePipe, http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.startDate = 'The Beginning of Time';
    this.finishDate = 'The Present Day';
    this.datePipe = datepipe;
    this.http = http;
    this.baseUrl = baseUrl;
    const formData: FormData = new FormData();
    formData.append('startDateString', '');
    formData.append('finishDateString', '');
  }

  ngOnInit() {
    this.reloadData();
  }

  reloadData() {
    this.overviewData = null;
    this.cumulativeFlowOptions.series = [];

    const productElement = <HTMLInputElement>document.getElementById('product');
    const engineeringElement = <HTMLInputElement>document.getElementById('engineering');
    const unanticipatedElement = <HTMLInputElement>document.getElementById('unanticipated');
    // Check for login
    this.timeStart();
    let cookieValue = getCookie();
    if (cookieValue == undefined) {
      this.router.navigate(['/login']);
    }
    // Overview Data
    this.http.get<OverviewData>(this.baseUrl + 'overview', {
      params:
        {
          startDateString: this.startDate,
          finishDateString: this.finishDate,
          product: String(productElement.checked),
          engineering: String(engineeringElement.checked.valueOf()),
          unanticipated: String(unanticipatedElement.checked.valueOf())
        }
    })
      .subscribe(x => {
        this.overviewData = x;
        this.timeStop();
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
    dateValue = (document.getElementById('finish-date') as HTMLInputElement).value;
    if (dateValue != '') {
      let date = new Date(dateValue);
      this.finishDate = this.datePipe.transform(new Date(date.getFullYear(), date.getMonth(), date.getDate() + 1), 'MMMM d, yyyy');
    }

    this.reloadData();
  }

  timeStart() {
    console.time('Page Load')
  }

  timeStop() {
    console.timeEnd('Page Load')
  }

  expand() {
    this.isExpanded = true;
  }

  collapse() {
    this.isExpanded = false;
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
