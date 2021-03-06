﻿import {Component, Inject, OnInit} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {FilterMessageService} from '../../app/_services/filterMessage.service';
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

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string, private messageService: FilterMessageService) {
    this.http = http;
    this.baseUrl = baseUrl;

    this.subscription = this.messageService.onMessage().subscribe(message => {
      if(message){
        this.reloadData(message.startDate, message.finishDate, message.product, message.engineering, message.unanticipated, message.assessmentsTeam, message.enterpriseTeam);
      } else {
        this.messages = [];
      }
    });
  }

  ngOnInit() {
    this.reloadData("", "", true, true, true, true, true);
  }

  reloadData(startDate, finishDate, product, engineering, unanticipated, assessmentsTeam, enterpriseTeam) {
    this.timeStart();
    this.analysisData = null;
    this.http.get<MultinomialLogisticRegressionAnalysisItemList>(this.baseUrl + 'multinomial-logistic-regression-analysis', {
      params:
        {
          startDateString: startDate,
          finishDateString: finishDate,
          product: product,
          engineering: engineering,
          unanticipated: unanticipated,
          assessmentsTeam: assessmentsTeam,
          enterpriseTeam: enterpriseTeam
        }
    })
      .subscribe(x => {
        this.analysisData = x;
        this.timeStop();
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

  timeStart() {
    console.time('Multinomial Logistic Regression Analysis Load')
  }

  timeStop() {
    console.timeEnd('Multinomial Logistic Regression Analysis Load')
  }
}

class MultinomialLogisticRegressionAnalysisItem {
  id: number;
  title: string;
  actual: number;
  prediction: number;
  probability: number;
  inputs: number[];
}

class MultinomialLogisticRegressionAnalysisItemList {
  items: MultinomialLogisticRegressionAnalysisItem[];
  users: string[];
  error: number;
}
