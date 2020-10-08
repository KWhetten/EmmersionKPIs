import {Component, Inject, OnInit} from '@angular/core';
import * as Highcharts from 'highcharts';
import {HttpClient} from '@angular/common/http';
import {HomeComponent} from '../../app/home/home.component';

declare var require: any;
let Boost = require('highcharts/modules/boost');
let noData = require('highcharts/modules/no-data-to-display');
let More = require('highcharts/highcharts-more');

Boost(Highcharts);
noData(Highcharts);
More(Highcharts);
noData(Highcharts);

@Component({
  selector: 'app-scatter-plot',
  templateUrl: './scatter-plot.component.html',
  styleUrls: ['./scatter-plot.component.css']
})
export class ScatterPlotComponent extends HomeComponent implements OnInit {

  public options: any = {
    chart: {
      type: 'scatter',
      height: 295
    },
    title: {
      text: 'Lead Time'
    },
    credits: {
      enabled: false
    },
    tooltip: {
      formatter: function () {
        return 'x: ' + Highcharts.dateFormat('%e %b %y %H:%M:%S', this.x) +
          'y: ' + this.y.toFixed(2);
      }
    },
    xAxis: {
      type: 'datetime',
      labels: {
        formatter: function () {
          return Highcharts.dateFormat('%e %b %y', this.value);
        }
      },
    },
    series: []
  }

  ngOnInit() {
    this.http.get<ScatterPlotData>(this.baseUrl + 'lead-time', {
      params: {startDateString: this.startDate, endDateString: this.endDate}
    })
      .subscribe(x => {
        this.options.series = x;
        Highcharts.chart('container', this.options);
      });

  }
}

class ScatterPlotData {
  name: string;
  turboThreshold: number;
  data: [Date, number]
}
