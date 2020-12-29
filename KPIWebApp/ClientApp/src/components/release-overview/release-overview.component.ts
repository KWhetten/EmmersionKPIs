import {Component, Inject, OnInit} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {DatePipe} from '@angular/common';
import {Router} from '@angular/router';
import {FilterMessageService} from '../../app/_services/filterMessage.service';
import {Subscription} from 'rxjs';

@Component({
  selector: 'app-release-overview-component',
  templateUrl: './release-overview.component.html',
  styleUrls: ['./release-overview.component.css'],
  providers: [DatePipe]
})
export class ReleaseOverviewComponent implements OnInit {
  public releaseOverviewData: ReleaseOverviewData;
  public http: HttpClient;
  private baseUrl: string;
  private subscription: Subscription;
  private messages: any[] = [];

  constructor(protected router: Router, datepipe: DatePipe, http: HttpClient, @Inject('BASE_URL') baseUrl: string, private messageService: FilterMessageService) {
    this.http = http;
    this.baseUrl = baseUrl;
    const formData: FormData = new FormData();
    formData.append('startDateString', '');
    formData.append('finishDateString', '');

    this.subscription = this.messageService.onMessage().subscribe(message => {
      if(message){
        this.reloadData(message.startDate, message.finishDate, message.product, message.engineering, message.unanticipated, message.assessmentsTeam, message.enterpriseTeam);
      } else {
        this.messages = [];
      }
    });
  }


  ngOnInit() {
    this.reloadData("", "", true, true, true, true, true);
  }

  reloadData(startDate, finishDate, product, engineering, unanticipated, assessmentsTeam, enterpriseTeam) {
    this.releaseOverviewData = null;
    this.timeStart();

    this.http.get<ReleaseOverviewData>(this.baseUrl + 'release-overview', {
      params:
        {
          startDateString: startDate,
          finishDateString: finishDate,
          product: product,
          engineering: engineering,
          unanticipated: unanticipated,
          assessmentsTeam: assessmentsTeam,
          enterpriseTeam: enterpriseTeam
        }
    })
      .subscribe(x => {
        this.releaseOverviewData = x;
        this.timeStop();
      });
  }

  timeStart() {
    console.time('Release Data Load')
  }

  timeStop() {
    console.timeEnd('Release Data Load')
  }
}

interface ReleaseOverviewData {
  totalDeploys: number;
  successfulDeploys: number;
  rolledBackDeploys: number;
  deployFrequency: number;
  meanTimeToRestore: number;
  changeFailPercentage: number;
}
