import {Component, Inject, OnInit} from '@angular/core';
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
  selector: 'app-lead-time-scatter-plot',
  templateUrl: './lead-time-scatter-plot.component.html',
  styleUrls: ['./lead-time-scatter-plot.component.css']
})
export class LeadTimeScatterPlotComponent implements OnInit {

  private http: HttpClient;
  private baseUrl: string;
  private subscription: Subscription;
  private messages: any[] = [];

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

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string, private messageService: FilterMessageService){
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

  reloadData(startDate, finishDate, product, engineering, unanticipated) {
    this.timeStart();
    this.http.get<ScatterPlotData[]>(this.baseUrl + 'lead-time-scatter', {
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
        this.scatterPlotOptions.series = x;
        this.scatterPlotOptions.series.forEach(function (item) {
          item['data'].forEach(function (dataItem) {
            dataItem['x'] = new Date(dataItem['x'])
          });
        });
        Highcharts.chart('lead-time-scatter-plot-container', this.scatterPlotOptions);
        this.timeStop();
      });
  }

  timeStart() {
    console.time('Lead Time Scatter Plot Load')
  }

  timeStop() {
    console.timeEnd('Lead Time Scatter Plot Load')
  }
}

  class ScatterPlotData {
  name: string;
  turboThreshold: number;
  data: [Date, number]
}
