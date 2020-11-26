import {Component, Inject, OnInit} from '@angular/core';
import * as Highcharts from 'highcharts';
import {HttpClient} from '@angular/common/http';

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
export class CumulativeFlowDiagramComponent implements OnInit {

  private http: HttpClient;
  private baseUrl: string;
  private startDate: string;
  private finishDate: string;

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

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.http = http;
    this.baseUrl = baseUrl;
    this.startDate = 'The Beginning of Time';
    this.finishDate = 'The Present Day';
  }

  ngOnInit() {
    this.reloadData(this.startDate, this.finishDate);
  }

  reloadData(startDate, finishDate) {
    const productElement = <HTMLInputElement>document.getElementById('product');
    const engineeringElement = <HTMLInputElement>document.getElementById('engineering');
    const unanticipatedElement = <HTMLInputElement>document.getElementById('unanticipated');

    this.http.get(this.baseUrl + 'cumulative-flow', {
      params:
        {
          startDateString: startDate,
          finishDateString: finishDate,
          product: String(productElement.checked),
          engineering: String(engineeringElement.checked.valueOf()),
          unanticipated: String(unanticipatedElement.checked.valueOf())
        }
    })
      .subscribe(x => {
        this.cumulativeFlowOptions.series = x['data'];
        this.cumulativeFlowOptions.xAxis.categories = x['dates'];
        Highcharts.chart('cumulative-flow-diagram-container', this.cumulativeFlowOptions);
      });
  }
}
