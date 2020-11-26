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
  selector: 'app-lead-time-scatter-plot',
  templateUrl: './lead-time-scatter-plot.component.html',
  styleUrls: ['./lead-time-scatter-plot.component.css']
})
export class LeadTimeScatterPlotComponent implements OnInit {

  private http: HttpClient;
  private baseUrl: string;
  private startDate: string;
  private finishDate: string;

  public scatterPlotOptions: any = {
    chart: {
      type: 'scatter'
    },
    data: {
      dateFormat: '%e %b %y %H:%M:%S'
    },
    title: {
      text: 'Lead Time for Task Items'
    },
    credits: {
      enabled: false
    },
    tooltip: {
      formatter: function () {
        return 'x: ' + Highcharts.dateFormat('%e %b %y %H:%M:%S', this.x) +
          '  y: ' + this.y.toFixed(2);
      }
    },
    xAxis: {
      type: 'datetime',
      title: {
        text: 'FinishDate'
      },
      labels: {
        formatter: function () {
          return Highcharts.dateFormat('%b %y', this.value);
        }
      }
    },
    yAxis: {
      title: {
        text: 'Lead Time (Hours of Work)'
      },
      labels: {
        format: '{value:,.0f}'
      }
    },
    series: []
  }

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string){
    this.http = http;
    this.baseUrl = baseUrl;
    this.startDate = "The Beginning of Time";
    this.finishDate = "The Present";
  }

  ngOnInit() {
    this.reloadData(this.startDate, this.finishDate);
  }

  reloadData(startDate, finishDate) {
    const productElement = <HTMLInputElement>document.getElementById('product');
    const engineeringElement = <HTMLInputElement>document.getElementById('engineering');
    const unanticipatedElement = <HTMLInputElement>document.getElementById('unanticipated');

    this.http.get<ScatterPlotData[]>(this.baseUrl + 'lead-time-scatter', {
      params:
        {
          startDateString: startDate,
          finishDateString: finishDate,
          product: String(productElement.checked),
          engineering: String(engineeringElement.checked),
          unanticipated: String(unanticipatedElement.checked)
        }
    })
      .subscribe(x => {
        this.scatterPlotOptions.series = x;
        this.scatterPlotOptions.series.forEach(function (item) {
          item['data'].forEach(function (dataItem) {
            dataItem['x'] = new Date(dataItem['x'])
          });
        });
        Highcharts.chart('lead-time-scatter-plot-container', this.scatterPlotOptions);
      });
  }
}

  class ScatterPlotData {
  name: string;
  turboThreshold: number;
  data: [Date, number]
}
