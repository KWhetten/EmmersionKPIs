﻿import {Component, Inject, OnInit} from "@angular/core";
import {DatePipe} from "@angular/common";
import {Router} from '@angular/router';
import {getCookie} from '../app.component';

@Component({
  selector: "app-work-item-cards-component",
  templateUrl: "./work-item-cards.component.html",
  styleUrls: ["./work-item-cards.component.css"],
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
