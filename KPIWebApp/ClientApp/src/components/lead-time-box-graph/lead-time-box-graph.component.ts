﻿import {Component, OnInit} from '@angular/core';
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
      categories: ['1', '2', '3', '4'],
      title: {
        text: 'Task Item Type'
      }
    },

    yAxis: {
      title: {
        text: 'Lead Time'
      },
    },

    series: [{
      name: 'Lead Time',
      data: [
        [760, 801, 848, 895, 965],
        [733, 853, 939, 980, 1080],
        [714, 762, 817, 870, 918],
        [724, 802, 806, 871, 950],
        [834, 836, 864, 882, 910]
      ],
      tooltip: {
        headerFormat: '<em>Lead Time {point.key}</em><br/>'
      }
    },
      {
        name: 'Outliers',
        color: Highcharts.getOptions().colors[0],
        type: 'scatter',
        data:[
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
    this.http.get<BoxGraphData>(this.baseUrl + 'lead-time-box', {
      params: {startDateString: this.startDate, endDateString: this.endDate}
    })
      .subscribe(x => {
        let array = new Array(x["entries"].length);
        let i = 0;
        x["entries"].forEach(function(item) {
          array[i] = [item["minimum"], item["lowerQuartile"], item["median"], item["upperQuartile"], item["maximum"]]
          i++;
        });
        this.options.series[0].data = array;
        this.options.series[1].data = x["outliers"];
        Highcharts.chart('lead-time-box-graph-container', this.options);
      });
  }
}

class BoxGraphData {
  entries: [];
  outliers: [];
}
