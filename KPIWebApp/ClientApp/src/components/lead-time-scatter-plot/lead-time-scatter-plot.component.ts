import {Component, OnInit} from '@angular/core';
import * as Highcharts from 'highcharts';
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
  selector: 'app-lead-time-scatter-plot',
  templateUrl: './lead-time-scatter-plot.component.html',
  styleUrls: ['./lead-time-scatter-plot.component.css']
})
export class LeadTimeScatterPlotComponent extends HomeComponent implements OnInit {

  public options: any = {
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

  ngOnInit() {
    this.http.get<ScatterPlotData[]>(this.baseUrl + 'lead-time-scatter', {
      params: {startDateString: this.startDate, endDateString: this.endDate}
    })
      .subscribe(x => {
        this.options.series = x;
        this.options.series.forEach(function(item){
          item["data"].forEach(function(dataItem){
            dataItem["x"] = new Date(dataItem["x"])
          });
        });
        Highcharts.chart('lead-time-scatter-plot-container', this.options);
      });

  }
}

class ScatterPlotData {
  name: string;
  turboThreshold: number;
  data: [Date, number]
}
