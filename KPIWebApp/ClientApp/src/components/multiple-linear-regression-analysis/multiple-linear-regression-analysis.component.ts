import {Component, Inject, OnInit} from '@angular/core';
import {HttpClient} from '@angular/common/http';

@Component({
  selector: 'app-multiple-linear-regression-analysis',
  templateUrl: './multiple-linear-regression-analysis.component.html',
  styleUrls: ['./multiple-linear-regression-analysis.component.css']
})
export class MultipleLinearRegressionAnalysisComponent {

  private http: HttpClient;
  private baseUrl: string;
  estimation: number;

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.http = http;
    this.baseUrl = baseUrl;
  }

  submit() {
    this.reloadData();
  }

  reloadData() {
    this.timeStart();

    let timeSpentInBacklog = +(document.getElementById('time-spent-in-backlog') as HTMLInputElement).value;
    let type = (document.getElementById('task-item-type') as HTMLSelectElement).value;
    let devTeam = (document.getElementById('dev-team') as HTMLSelectElement).value;
    let createdBy = (document.getElementById('created-by') as HTMLSelectElement).value;

    if(!Number.isNaN(timeSpentInBacklog) && type != 'select' && devTeam != 'select' && createdBy != 'select') {
      (document.getElementById('time-spent-in-backlog-error') as HTMLSelectElement).hidden = true;
      (document.getElementById('task-type-error') as HTMLSelectElement).hidden = true;
      (document.getElementById('dev-team-error') as HTMLSelectElement).hidden = true;
      (document.getElementById('created-by-error') as HTMLSelectElement).hidden = true;

      this.http.get<number>(this.baseUrl + 'multiple-linear-regression-analysis', {
        params:
          {
            timeSpentInBacklog: timeSpentInBacklog.toString(),
            type,
            devTeam,
            createdBy
          }
      })
        .subscribe(x => {
          if(x >= 0) {
            (document.getElementById('result-error') as HTMLSelectElement).hidden = true;
            this.estimation = x;
          } else {
            this.estimation = null;
            (document.getElementById('result-error') as HTMLSelectElement).hidden = false;
          }
          this.timeStop();
        });
    }
    if(Number.isNaN(timeSpentInBacklog)){
      (document.getElementById('time-spent-in-backlog-error') as HTMLSelectElement).hidden = false;
    }
    if(type == 'select'){
      (document.getElementById('task-type-error') as HTMLSelectElement).hidden = false;
    }
    if(devTeam == 'select'){
      (document.getElementById('dev-team-error') as HTMLSelectElement).hidden = false;
    }
    if(createdBy == 'select'){
      (document.getElementById('created-by-error') as HTMLSelectElement).hidden = false;
    }
  }

  getType(TypeId: number) {
    if (TypeId == 1) {
      return 'Product';
    }
    if (TypeId == 2) {
      return 'Engineering';
    }
    if (TypeId == 3) {
      return 'Unanticipated';
    }
  }

  timeStart() {
    console.time('Multiple Linear Regression Analysis Load')
  }

  timeStop() {
    console.timeEnd('Multiple Linear Regression Analysis Load')
  }

  OnlyNumbers($event: KeyboardEvent) {
    let regex: RegExp = new RegExp(/^[\d.]$/g);
    let specialKeys: Array<string> = ['Backspace', 'Tab', 'End', 'Home', 'ArrowRight','ArrowLeft'];
    if (specialKeys.indexOf($event.key) !== -1) {
      return;
    } else {
      return regex.test($event.key);
    }
  }
}
