import {Component, EventEmitter, Inject, OnInit, Output} from '@angular/core';
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
        formatter: function() {
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
        data:[],
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
    // Check for login
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
    // Cumulative Flow Diagram
    this.http.get(this.baseUrl + 'cumulative-flow', {
      params: {startDateString: this.startDate, finishDateString: this.finishDate}
    })
      .subscribe(x => {
        this.cumulativeFlowOptions.series = x['data'];
        this.cumulativeFlowOptions.xAxis.categories = x['dates'];
        Highcharts.chart('cumulative-flow-diagram-container', this.cumulativeFlowOptions);
      });
    // Scatter Plot
    this.http.get<ScatterPlotData[]>(this.baseUrl + 'lead-time-scatter', {
      params: {startDateString: this.startDate, finishDateString: this.finishDate}
    })
      .subscribe(x => {
        this.scatterPlotOptions.series = x;
        this.scatterPlotOptions.series.forEach(function(item){
          item["data"].forEach(function(dataItem){
            dataItem["x"] = new Date(dataItem["x"])
          });
        });
        Highcharts.chart('lead-time-scatter-plot-container', this.scatterPlotOptions);
      });
    // Box Graph
    this.http.get<BoxGraphData>(this.baseUrl + 'lead-time-box', {
      params: {startDateString: this.startDate, finishDateString: this.finishDate}
    })
      .subscribe(x => {
        let boxGraphDataArray = new Array(x["entries"].length);
        let categoryArray = new Array(x["entries"].length);
        let i = 0;
        x["entries"].forEach(function(item) {
          categoryArray[i] = item["taskItemType"]
          boxGraphDataArray[i] = [item["minimum"], item["lowerQuartile"], item["median"], item["upperQuartile"], item["maximum"]]
          i++;
        });
        this.boxGraphOptions.xAxis.categories = categoryArray;
        this.boxGraphOptions.series[0].data = boxGraphDataArray;
        this.boxGraphOptions.series[1].data = x["outliers"];
        Highcharts.chart('lead-time-box-graph-container', this.boxGraphOptions);
      });
  }

  reloadData() {
    this.overviewData = null;

    // Overview Data
    this.http.get<OverviewData>(this.baseUrl + 'overview', {
      params: {startDateString: this.startDate, finishDateString: this.finishDate}
    })
      .subscribe(x => {
        this.overviewData = x;
      });
    // Cumulative Flow Diagram
    this.http.get(this.baseUrl + 'cumulative-flow', {
      params: {startDateString: this.startDate, finishDateString: this.finishDate}
    })
      .subscribe(x => {
        this.cumulativeFlowOptions.series = x['data'];
        this.cumulativeFlowOptions.xAxis.categories = x['dates'];
        Highcharts.chart('cumulative-flow-diagram-container', this.cumulativeFlowOptions);
      });
    // Scatter Plot
    this.http.get<ScatterPlotData[]>(this.baseUrl + 'lead-time-scatter', {
      params: {startDateString: this.startDate, finishDateString: this.finishDate}
    })
      .subscribe(x => {
        this.scatterPlotOptions.series = x;
        this.scatterPlotOptions.series.forEach(function(item){
          item["data"].forEach(function(dataItem){
            dataItem["x"] = new Date(dataItem["x"])
          });
        });
        Highcharts.chart('lead-time-scatter-plot-container', this.scatterPlotOptions);
      });
    // Box Graph
    this.http.get<BoxGraphData>(this.baseUrl + 'lead-time-box', {
      params: {startDateString: this.startDate, finishDateString: this.finishDate}
    })
      .subscribe(x => {
        let boxGraphDataArray = new Array(x["entries"].length);
        let categoryArray = new Array(x["entries"].length);
        let i = 0;
        x["entries"].forEach(function(item) {
          categoryArray[i] = item["taskItemType"]
          boxGraphDataArray[i] = [item["minimum"], item["lowerQuartile"], item["median"], item["upperQuartile"], item["maximum"]]
          i++;
        });
        this.boxGraphOptions.xAxis.categories = categoryArray;
        this.boxGraphOptions.series[0].data = boxGraphDataArray;
        this.boxGraphOptions.series[1].data = x["outliers"];
        Highcharts.chart('lead-time-box-graph-container', this.boxGraphOptions);
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
class BoxGraphData {
  entries: [];
  outliers: [];
}

class ScatterPlotData {
  name: string;
  turboThreshold: number;
  data: [Date, number]
}
