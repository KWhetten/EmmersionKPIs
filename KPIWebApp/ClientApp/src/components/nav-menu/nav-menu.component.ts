import {Component, OnInit} from '@angular/core';
import {getAuthorizedCookie} from '../../app/app.component';
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

  constructor(private messageService: LoginMessageService) {
    this.subscription = this.messageService.onMessage().subscribe(message => {
      if (message.isLoggedIn) {
        this.expand();
        this.reloadData();
      } else {
        this.collapse();
        this.reloadData();
      }
    });
  }

  ngOnInit() {
    let cookieValue = getAuthorizedCookie();
    if (cookieValue == undefined) {
      console.log('collapse');
      this.collapse();
    } else {
      console.log('expand');
      this.expand();
    }
  }

  reloadData() {
  }

  ngOnChanges() {
    console.log('ngOnChanges');
    let cookieValue = getAuthorizedCookie();
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

  expand() {
    this.isExpanded = true;
  }

  collapse() {
    this.isExpanded = false;
  }
}
