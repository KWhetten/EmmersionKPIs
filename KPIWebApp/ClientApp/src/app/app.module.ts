import {BrowserModule} from '@angular/platform-browser';
import {NgModule} from '@angular/core';
import {FormsModule} from '@angular/forms';
import {HttpClientModule} from '@angular/common/http';
import {RouterModule} from '@angular/router';

import {AppComponent} from './app.component';
import {NavMenuComponent} from '../components/nav-menu/nav-menu.component';
import {HomeComponent} from './home/home.component';
import {TaskItemsComponent} from './task-items/task-items.component';
import {FilterOptionsComponent} from '../components/filter-options/filter-options.component';
import {LoginComponent} from './login/login.component';
import {InviteUserComponent} from './invite-user/invite-user.component';
import {CreatePasswordComponent} from './create-password/create-password.component';
import {ChangePasswordComponent} from './change-password/change-password.component';
import {ForgotPasswordComponent} from './forgot-password/forgot-password.component';
import {CumulativeFlowDiagramComponent} from '../components/cumulative-flow-diagram/cumulative-flow-diagram.component'
import {LeadTimeScatterPlotComponent} from '../components/lead-time-scatter-plot/lead-time-scatter-plot.component';
import {LeadTimeBoxGraphComponent} from '../components/lead-time-box-graph/lead-time-box-graph.component';
import {MultinomialLogisticRegressionAnalysisComponent} from '../components/multinomial-logistic-regression-analysis/multinomial-logistic-regression-analysis.component';
import {MultipleLinearRegressionAnalysisComponent} from '../components/multiple-linear-regression-analysis/multiple-linear-regression-analysis.component';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    FilterOptionsComponent,
    HomeComponent,
    TaskItemsComponent,
    LoginComponent,
    InviteUserComponent,
    CreatePasswordComponent,
    ChangePasswordComponent,
    ForgotPasswordComponent,
    CumulativeFlowDiagramComponent,
    LeadTimeScatterPlotComponent,
    LeadTimeBoxGraphComponent,
    MultinomialLogisticRegressionAnalysisComponent,
    MultipleLinearRegressionAnalysisComponent
  ],
  imports: [
    BrowserModule.withServerTransition({appId: 'ng-cli-universal'}),
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot([
      {path: '', component: HomeComponent, pathMatch: 'full'},
      {path: 'task-items', component: TaskItemsComponent},
      {path: 'login', component: LoginComponent},
      {path: 'invite-user', component: InviteUserComponent},
      {path: 'create-password', component: CreatePasswordComponent},
      {path: 'change-password', component: ChangePasswordComponent},
      {path: 'forgot-password', component: ForgotPasswordComponent}
    ])
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule {
}
