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
  selector: 'app-lead-time-box-graph',
  templateUrl: './lead-time-box-graph.component.html',
  styleUrls: ['./lead-time-box-graph.component.css']
})
export class LeadTimeBoxGraphComponent extends HomeComponent implements OnInit {

  public options: any = {
    chart: {
      type: 'boxplot'
    },

    title: {
      text: 'Highcharts Box Plot Example'
    },

    legend: {
      enabled: false
    },

    xAxis: {
      categories: ['1', '2', '3', '4', '5'],
      title: {
        text: 'Experiment No.'
      }
    },

    yAxis: {
      title: {
        text: 'Observations'
      },
      plotLines: [{
        value: 932,
        color: 'red',
        width: 1,
        label: {
          text: 'Theoretical mean: 932',
          align: 'center',
          style: {
            color: 'gray'
          }
        }
      }]
    },

    series: [{
      name: 'Observations',
      data: [
        [760, 801, 848, 895, 965],
        [733, 853, 939, 980, 1080],
        [714, 762, 817, 870, 918],
        [724, 802, 806, 871, 950],
        [834, 836, 864, 882, 910]
      ],
      tooltip: {
        headerFormat: '<em>Experiment No {point.key}</em><br/>'
      }
    },
      {
        name: 'Outliers',
        color: Highcharts.getOptions().colors[0],
        type: 'scatter',
        data: [ // x, y positions where 0 is the first category
          [0, 644],
          [4, 718],
          [4, 951],
          [4, 969]
        ],
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

  ngOnInit() {
    Highcharts.chart('lead-time-box-graph-container', this.options);
  }
}
