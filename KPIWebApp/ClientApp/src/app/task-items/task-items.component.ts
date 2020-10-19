import {Component, Inject, OnInit} from "@angular/core";
import {DatePipe} from "@angular/common";
import {Router} from '@angular/router';
import {getCookie} from '../app.component';

@Component({
  selector: "app-task-items-component",
  templateUrl: "./task-items.component.html",
  styleUrls: ["./task-items.component.css"],
  providers: [DatePipe]
})
export class TaskItemsComponent implements OnInit {

  constructor(private router: Router, @Inject("BASE_URL") baseUrl: string) {

  }
  ngOnInit() {
    let cookieValue = getCookie();
    if (cookieValue == undefined) {
      this.router.navigate(['/login']);
    }
  }
}
