import {Component, Inject, OnInit} from '@angular/core';
import {getCookie} from '../../app/app.component';
import {HttpClient} from '@angular/common/http';
import {LoginMessageService} from '../../app/_services/loginMessage.service';
import {Subscription} from 'rxjs';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})

export class NavMenuComponent implements OnInit {
  public isExpanded = false;
  private subscription: Subscription;

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string, private messageService: LoginMessageService) {
    this.subscription = this.messageService.onMessage().subscribe(message => {
      if(message.isLoggedIn){
        this.expand();
        this.reloadData();
      } else {
        this.collapse();
        this.reloadData();
      }
    });
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
