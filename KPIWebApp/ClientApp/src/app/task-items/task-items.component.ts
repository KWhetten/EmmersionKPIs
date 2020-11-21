import {Component, Inject, OnInit} from "@angular/core";
import {DatePipe} from "@angular/common";
import {Router} from '@angular/router';
import {getCookie} from '../app.component';
import {HttpClient} from '@angular/common/http';

@Component({
  selector: "app-task-items-component",
  templateUrl: "./task-items.component.html",
  styleUrls: ["./task-items.component.css"],
  providers: [DatePipe]
})
export class TaskItemsComponent implements OnInit {

  protected http: HttpClient;
  protected baseUrl: string;


  constructor(private router: Router, @Inject("BASE_URL") baseUrl: string, http: HttpClient) {
    this.http = http;
    this.baseUrl = baseUrl;

  }
  ngOnInit() {
    let cookieValue = getCookie();
    if (cookieValue == undefined) {
      this.router.navigate(['/login']);
    }
    this.http.get<number[]>(this.baseUrl + 'cluster-analysis')
      .subscribe(x => {
      });
  }
}
