import {Component, EventEmitter, Inject, OnInit, Output} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {DatePipe} from '@angular/common';
import {Router} from '@angular/router';
import {getCookie} from '../app.component';
import {FilterMessageService} from '../_services/filterMessage.service';

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

  constructor(protected router: Router, http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.http = http;
    this.baseUrl = baseUrl;
    const formData: FormData = new FormData();
    formData.append('startDateString', '');
    formData.append('finishDateString', '');
  }


  ngOnInit() {
    let cookieValue = getCookie();
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
