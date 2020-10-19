import {Component, OnChanges, OnInit} from '@angular/core';
import {getCookie} from '../../app/app.component';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})

export class NavMenuComponent implements OnInit, OnChanges {
  public isExpanded = false;

  constructor() {
  }

  ngOnInit() {
    console.log("ngOnInit");
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
