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
import {ReleasesComponent} from "./releases/releases.component";
import {LoginComponent} from "./login/login.component";

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    FilterOptionsComponent,
    OverviewComponent,
    WorkItemCardsComponent,
    ReleasesComponent,
    LoginComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot([
      { path: 'home', component: OverviewComponent, pathMatch: 'full' },
      { path: 'work-item-cards', component: WorkItemCardsComponent },
      { path: "releases", component: ReleasesComponent },
      { path: "login", component: LoginComponent }
    ])
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule {
}
