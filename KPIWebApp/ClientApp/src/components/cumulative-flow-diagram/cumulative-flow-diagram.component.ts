import {Component, Inject, OnDestroy, OnInit} from '@angular/core';
import * as Highcharts from 'highcharts';
import {HttpClient} from '@angular/common/http';
import {FilterMessageService} from '../../app/_services/filterMessage.service';
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
  selector: 'app-cumulative-flow-diagram',
  templateUrl: './cumulative-flow-diagram.component.html',
  styleUrls: ['./cumulative-flow-diagram.component.css']
})
export class CumulativeFlowDiagramComponent implements OnInit, OnDestroy {

  private http: HttpClient;
  private baseUrl: string;
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
      labels: {}
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

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string, private messageService: FilterMessageService) {
    this.http = http;
    this.baseUrl = baseUrl;

    this.subscription = this.messageService.onMessage().subscribe(message => {
      if(message){
        this.reloadData(message.startDate, message.finishDate, message.product, message.engineering, message.unanticipated);
      } else {
        this.messages = [];
      }
    });
  }

  ngOnInit() {
    this.reloadData("", "", true, true, true);
  }
  ngOnDestroy() {
    this.subscription.unsubscribe();
  }

  reloadData(startDate, finishDate, product, engineering, unanticipated) {
    this.timeStart();
    this.http.get(this.baseUrl + 'cumulative-flow', {
      params:
        {
          startDateString: startDate,
          finishDateString: finishDate,
          product: String(product),
          engineering: String(engineering),
          unanticipated: String(unanticipated)
        }
    })
      .subscribe(x => {
        this.cumulativeFlowOptions.series = x['data'];
        this.cumulativeFlowOptions.xAxis.categories = x['dates'];
        Highcharts.chart('cumulative-flow-diagram-container', this.cumulativeFlowOptions);
        this.timeStop();
      });
  }

  timeStart() {
    console.time('Cumulative Flow Diagram Load')
  }

  timeStop() {
    console.timeEnd('Cumulative Flow Diagram Load')
  }
}
