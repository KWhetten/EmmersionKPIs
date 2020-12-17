import {Component, EventEmitter, Inject, OnInit, Output} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {DatePipe} from '@angular/common';
import {Router} from '@angular/router';
import {getCookie} from '../app.component';
import * as Highcharts from 'highcharts';
import {FilterMessageService} from '../_services/filterMessage.service';
import {Subscription} from 'rxjs';

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
  startTimer: any;
  @Output() submitted = new EventEmitter<any>();
  isExpanded = false;
  private subscription: Subscription;
  private messages: any[] = [];

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
  authorized: boolean = false;

  constructor(protected router: Router, datepipe: DatePipe, http: HttpClient, @Inject('BASE_URL') baseUrl: string, private messageService: FilterMessageService) {
    this.datePipe = datepipe;
    this.http = http;
    this.baseUrl = baseUrl;
    const formData: FormData = new FormData();
    formData.append('startDateString', '');
    formData.append('finishDateString', '');

    this.subscription = this.messageService.onMessage().subscribe(message => {
      if(message){
        this.reloadData(message.startDate, message.finishDate, message.product, message.engineering, message.unanticipated);
      } else {
        this.messages = [];
      }
    });
  }

  ngOnInit() {
    let cookieValue = getCookie();
    this.http.get<boolean>(this.baseUrl + 'authorize-user', {
      params: {guid: cookieValue}
    })
      .subscribe((result) => {
        this.authorized = result;
        if (cookieValue == undefined || !this.authorized) {
          this.router.navigate(['/login']);
        }
        this.reloadData("", "", true, true, true);
      });
  }

  reloadData(startDate, finishDate, product, engineering, unanticipated) {
    this.overviewData = null;

    // Overview Data
    this.timeStart();
    this.http.get<OverviewData>(this.baseUrl + 'overview', {
      params:
        {
          startDateString: startDate,
          finishDateString: finishDate,
          product: product,
          engineering: engineering,
          unanticipated: unanticipated
        }
    })
      .subscribe(x => {
        this.overviewData = x;
        this.timeStop();
      });
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
