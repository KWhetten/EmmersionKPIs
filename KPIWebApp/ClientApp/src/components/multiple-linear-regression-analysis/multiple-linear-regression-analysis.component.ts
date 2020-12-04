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
  private readonly startDate: string;
  private readonly finishDate: string;
  analysisData: MultinomialLogisticRegressionAnalysisItemList;
  item: MultinomialLogisticRegressionAnalysisItem[];

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.http = http;
    this.baseUrl = baseUrl;
    this.startDate = 'The Beginning of Time';
    this.finishDate = 'The Present';
  }

  ngOnInit() {
    this.reloadData(this.startDate, this.finishDate);
  }

  reloadData(startDate, finishDate) {
    this.http.get<MultinomialLogisticRegressionAnalysisItemList>(this.baseUrl + 'multiple-linear-regression-analysis', {
      params:
        {

        }
    })
      .subscribe(x => {
        this.analysisData = x;
      });
  }

  getType(TypeId: number) {
    if(TypeId == 1){
      return "Product";
    }
    if(TypeId == 2){
      return "Engineering";
    }
    if(TypeId == 3){
      return "Unanticipated";
    }
  }
}

class MultinomialLogisticRegressionAnalysisItem {
  id: number;
  title: string;
  output: number;
  answer: number;
  probability: number;
  inputs: number[];
}

class MultinomialLogisticRegressionAnalysisItemList {
  items: MultinomialLogisticRegressionAnalysisItem[];
  users: string[];
  error: number;
}
