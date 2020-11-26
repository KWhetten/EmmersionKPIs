import {Component, Inject, OnInit} from '@angular/core';
import {getCookie} from '../../app/app.component';
import {HttpClient} from '@angular/common/http';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})

export class NavMenuComponent implements OnInit {
  public isExpanded = false;

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {

  }

  ngOnInit() {
    this.reloadData();
  }

  reloadData() {
    let cookieValue = getCookie();
    if (cookieValue == undefined) {
      if (this.isExpanded) {
        console.log('collapse');
        this.collapse();
      }
    } else {
      if (!this.isExpanded) {
        console.log('expand');
        this.expand();
      }
    }
  }

  ngOnChanges() {
    console.log("ngOnChanges");
    let cookieValue = getCookie();
    if (cookieValue == undefined) {
      if (this.isExpanded) {
        console.log("collapse");
        this.collapse();
      }
    } else {
      if (!this.isExpanded) {
        console.log("expand");
        this.expand();
      }
    }
  }

  expand() {
    this.isExpanded = true;
  }

  collapse() {
    this.isExpanded = false;
  }
}
