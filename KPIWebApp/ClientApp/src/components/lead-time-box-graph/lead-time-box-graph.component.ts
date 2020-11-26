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
  selector: 'app-lead-time-box-graph',
  templateUrl: './lead-time-box-graph.component.html',
  styleUrls: ['./lead-time-box-graph.component.css']
})
export class LeadTimeBoxGraphComponent implements OnInit {

  private http: HttpClient;
  private baseUrl: string;
  private startDate: string;
  private finishDate: string;

  public boxGraphOptions: any = {
    chart: {
      type: 'boxplot',
      inverted: true
    },

    title: {
      text: 'Lead Time by Task Item Type'
    },

    legend: {
      enabled: false
    },

    xAxis: {
      categories: [],
      title: {
        text: 'Task Item Type'
      }
    },

    yAxis: {
      title: {
        text: 'Lead Time (Hours of Work)'
      },
    },

    series: [{
      name: 'Lead Time',
      data: [],
      tooltip: {
        headerFormat: '<em>Lead Time {point.key}</em><br/>'
      }
    },
      {
        name: 'Outliers',
        color: Highcharts.getOptions().colors[0],
        type: 'scatter',
        data: [],
        marker: {
          fillColor: 'white',
          lineWidth: 1,
          lineColor: Highcharts.getOptions().colors[0]
        },
        tooltip: {
          pointFormat: 'Observation: {point.y}'
        }
      }]
  }

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
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

    this.http.get<BoxGraphData>(this.baseUrl + 'lead-time-box', {
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
        let boxGraphDataArray = new Array(x['entries'].length);
        let categoryArray = new Array(x['entries'].length);
        let i = 0;
        x['entries'].forEach(function (item) {
          categoryArray[i] = item['taskItemType']
          boxGraphDataArray[i] = [item['minimum'], item['lowerQuartile'], item['median'], item['upperQuartile'], item['maximum']]
          i++;
        });
        this.boxGraphOptions.xAxis.categories = categoryArray;
        this.boxGraphOptions.series[0].data = boxGraphDataArray;
        this.boxGraphOptions.series[1].data = x['outliers'];
        Highcharts.chart('lead-time-box-graph-container', this.boxGraphOptions);
      });
  }
}

class BoxGraphData {
  entries: [];
  outliers: [];
}
