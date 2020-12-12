import {Component, Inject, OnInit} from '@angular/core';
import {HttpClient} from '@angular/common/http';

@Component({
  selector: 'app-multiple-linear-regression-analysis',
  templateUrl: './multiple-linear-regression-analysis.component.html',
  styleUrls: ['./multiple-linear-regression-analysis.component.css']
})
export class MultipleLinearRegressionAnalysisComponent implements OnInit {

  private http: HttpClient;
  private baseUrl: string;
  estimation: number;

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.http = http;
    this.baseUrl = baseUrl;
  }

  ngOnInit() {
    this.reloadData();
  }

  submit() {
    this.reloadData();
  }

  reloadData() {
    let timeSpentInBacklog = (document.getElementById('time-spent-in-backlog') as HTMLInputElement).value;
    let type = (document.getElementById('task-item-type') as HTMLSelectElement).value;
    let devTeam = (document.getElementById('dev-team') as HTMLSelectElement).value;
    let createdBy = (document.getElementById('created-by') as HTMLSelectElement).value;
    this.http.get<number>(this.baseUrl + 'multiple-linear-regression-analysis', {
      params:
        {
          timeSpentInBacklog,
          type,
          devTeam,
          createdBy
        }
    })
      .subscribe(x => {
        this.estimation = x;
      });
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
}
