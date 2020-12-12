import {Component, Inject, OnInit} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {MessageService} from '../../app/_services/message.service';
import {Subscription} from 'rxjs';

@Component({
  selector: 'app-multinomial-logistic-regression-analysis',
  templateUrl: './multinomial-logistic-regression-analysis.component.html',
  styleUrls: ['./multinomial-logistic-regression-analysis.component.css']
})
export class MultinomialLogisticRegressionAnalysisComponent implements OnInit {

  private http: HttpClient;
  private baseUrl: string;
  private subscription: Subscription;
  private messages: any[] = [];
  analysisData: MultinomialLogisticRegressionAnalysisItemList;
  item: MultinomialLogisticRegressionAnalysisItem[];

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string, private messageService: MessageService) {
    this.http = http;
    this.baseUrl = baseUrl;

    this.subscription = this.messageService.onMessage().subscribe(message => {
      if(message){
        this.reloadData(message.startDate, message.finishDate);
      } else {
        this.messages = [];
      }
    });
  }

  ngOnInit() {
    this.reloadData("", "");
  }

  reloadData(startDate, finishDate) {
    this.analysisData = null;
    this.http.get<MultinomialLogisticRegressionAnalysisItemList>(this.baseUrl + 'multinomial-logistic-regression-analysis', {
      params:
        {
          startDateString: startDate,
          finishDateString: finishDate
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
  prediction: number;
  probability: number;
  inputs: number[];
}

class MultinomialLogisticRegressionAnalysisItemList {
  items: MultinomialLogisticRegressionAnalysisItem[];
  users: string[];
  error: number;
}
