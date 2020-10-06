import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule} from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { NavMenuComponent } from '../components/nav-menu/nav-menu.component';
import {OverviewComponent} from "./overview/overview.component";
import {WorkItemCardsComponent} from "./work-item-cards/work-item-cards.component";
import {FilterOptionsComponent} from "../components/filter-options/filter-options.component";
import {LoginComponent} from "./login/login.component";
import {RegisterComponent} from "./register/register.component";

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    FilterOptionsComponent,
    OverviewComponent,
    WorkItemCardsComponent,
    LoginComponent,
    RegisterComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot([
      { path: 'home', component: OverviewComponent, pathMatch: 'full' },
      { path: 'work-item-cards', component: WorkItemCardsComponent },
      { path: "login", component: LoginComponent },
      { path: "register", component: RegisterComponent }
    ])
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule {
}
