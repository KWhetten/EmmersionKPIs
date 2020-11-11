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
  selector: 'app-cumulative-flow-diagram',
  templateUrl: './cumulative-flow-diagram.component.html',
  styleUrls: ['./cumulative-flow-diagram.component.css']
})
export class CumulativeFlowDiagramComponent extends HomeComponent implements OnInit {

  public options: any = {
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

  ngOnInit() {
    this.http.get(this.baseUrl + 'cumulative-flow', {
      params: {startDateString: this.startDate, endDateString: this.endDate}
    })
      .subscribe(x => {
        this.options.series = x['data'];
        this.options.xAxis.categories = x['dates'];
        Highcharts.chart('cumulative-flow-diagram-container', this.options);
      });
  }
}
