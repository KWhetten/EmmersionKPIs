import {Component, Inject, OnInit} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {DatePipe} from '@angular/common';
import {Router} from '@angular/router';
import {getAuthorizedCookie} from '../app.component';
import {FilterMessageService} from '../_services/filterMessage.service';
import {Subscription} from 'rxjs';

@Component({
  selector: 'app-home-component',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css'],
  providers: [DatePipe]
})
export class HomeComponent implements OnInit {
  protected http: HttpClient;
  protected baseUrl: string;
  public startDate;
  public finishDate;
  authorized: boolean = false;
  private subscription: Subscription;
  private messages: any[] = [];

  constructor(protected router: Router, http: HttpClient, @Inject('BASE_URL') baseUrl: string, private messageService: FilterMessageService) {
    this.http = http;
    this.baseUrl = baseUrl;
    const formData: FormData = new FormData();
    formData.append('startDateString', '');
    formData.append('finishDateString', '');

    this.subscription = this.messageService.onMessage().subscribe(message => {
      if(message){
        this.startDate = message.startDate;
        this.finishDate = message.finishDate;
      } else {
        this.messages = [];
      }
    });
  }


  ngOnInit() {
    let cookieValue = getAuthorizedCookie();
    this.http.get<boolean>(this.baseUrl + 'authorize-user', {
      params: {guid: cookieValue}
    })
      .subscribe((result) => {
        this.authorized = result;
        if (cookieValue == undefined || !this.authorized) {
          this.router.navigate(['/login']);
        }
        this.startDate = "October 19, 2020";
        this.finishDate = "Present Day";
      });
  }
}
