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
  selector: 'app-releases-bar-graph',
  templateUrl: './releases-bar-graph.component.html',
  styleUrls: ['./releases-bar-graph.component.css']
})
export class ReleasesBarGraphComponent implements OnInit {
  private http: HttpClient;
  private baseUrl: string;
  private subscription: Subscription;
  private messages: any[] = [];

  public barGraphOptions: any = {
    chart: {
      type: 'column'
    },
    title: {
      text: 'Releases'
    },
    xAxis: {
      categories: []
    },
    yAxis: {
      min: 0,
      title: {
        text: 'Total Releases'
      },
      stackLabels: {
        enabled: false
      }
    },
    legend: {
      align: 'right',
      x: -30,
      verticalAlign: 'top',
      y: 25,
      floating: true,
      backgroundColor:
        Highcharts.defaultOptions.legend.backgroundColor || 'white',
      borderColor: '#CCC',
      borderWidth: 1,
      shadow: false
    },
    tooltip: {
      headerFormat: '<b>{point.x}</b><br/>',
      pointFormat: '{series.name}: {point.y}<br/>Total: {point.stackTotal}'
    },
    plotOptions: {
      column: {
        stacking: 'normal',
        dataLabels: {
          enabled: false
        }
      }
    },
    series: []
  };

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string, private messageService: FilterMessageService){
    this.http = http;
    this.baseUrl = baseUrl;

    this.subscription = this.messageService.onMessage().subscribe(message => {
      if(message){
        this.reloadData(message.startDate, message.finishDate, message.assessmentsTeam, message.enterpriseTeam);
      } else {
        this.messages = [];
      }
    });
  }

  ngOnInit() {
    this.reloadData("", "", true, true);
  }

  reloadData(startDate, finishDate, assessmentsTeam, enterpriseTeam) {
    this.timeStart();this.http.get<ReleaseBarGraphData[]>(this.baseUrl + 'releases-bar-graph', {
      params:
        {
          startDateString: startDate,
          finishDateString: finishDate,
          assessmentsTeam: assessmentsTeam,
          enterpriseTeam: enterpriseTeam
        }
    })
      .subscribe(x => {
        this.barGraphOptions.xAxis.categories = x["dates"];
        this.barGraphOptions.series = x["rows"];
        Highcharts.chart('releases-bar-graph-container', this.barGraphOptions);
        this.timeStop();
      });
  }

  timeStart() {
    console.time('Releases Bar Graph Loaded')
  }

  timeStop() {
    console.timeEnd('Releases Bar Graph Loaded')
  }
}

class ReleaseBarGraphData {
}
