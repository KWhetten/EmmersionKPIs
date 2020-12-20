import {Component, Inject, OnInit} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {DatePipe} from '@angular/common';
import {Router} from '@angular/router';
import {FilterMessageService} from '../../app/_services/filterMessage.service';
import {Subscription} from 'rxjs';

@Component({
  selector: 'app-task-item-overview-component',
  templateUrl: './task-item-overview.component.html',
  styleUrls: ['./task-item-overview.component.css'],
  providers: [DatePipe]
})
export class TaskItemOverviewComponent implements OnInit {
  public taskItemOverviewData: TaskItemOverviewData;
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
    this.taskItemOverviewData = null;
    this.timeStart();

    this.http.get<TaskItemOverviewData>(this.baseUrl + 'task-item-overview', {
      params:
        {
          startDateString: startDate,
          finishDateString: finishDate,
          product: product,
          engineering: engineering,
          unanticipated: unanticipated
        }
    })
      .subscribe(x => {
        this.taskItemOverviewData = x;
        this.timeStop();
      });
  }

  timeStart() {
    console.time('Task Item Data Load')
  }

  timeStop() {
    console.timeEnd('Task Item Data Load')
  }
}

interface TaskItemOverviewData {
  averageLeadTime: number;
  longestLeadTime: number;
  shortestLeadTime: number;
  totalCards: number;
}
