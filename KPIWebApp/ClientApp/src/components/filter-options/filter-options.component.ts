﻿import {Component, Inject} from '@angular/core';
import {DatePipe} from '@angular/common';
import {HttpClient} from '@angular/common/http';
import {FilterMessageService} from '../../app/_services/filterMessage.service';

@Component({
  selector: 'app-filter-options',
  templateUrl: './filter-options.component.html',
  styleUrls: ['./filter-options.component.css'],
  providers: [DatePipe]
})
export class FilterOptionsComponent {
  isExpanded = false;
  private datePipe: DatePipe;
  private startDateString: any;
  private finishDateString: any;
  today = Date.now().toString();
  displayCardTypes: boolean = true;

  constructor(datepipe: DatePipe, http: HttpClient, @Inject('BASE_URL') baseUrl: string, private messageService: FilterMessageService) {
    this.datePipe = datepipe;
  }

  submit() {
    this.reloadData();
  }

  sendInfo() {
    this.messageService.sendMessage(this.startDateString, this.finishDateString, (document.getElementById('product') as HTMLInputElement).checked,
      (document.getElementById('engineering') as HTMLInputElement).checked, (document.getElementById('unanticipated') as HTMLInputElement).checked,
      (document.getElementById('assessments-team') as HTMLInputElement).checked, (document.getElementById('enterprise-team') as HTMLInputElement).checked);
  }

  private clearMessages() {
    this.messageService.clearMessages();
  }

  reloadData() {
    let startDate = new Date((document.getElementById('start-date') as HTMLInputElement).value);
    let finishDate = new Date((document.getElementById('finish-date') as HTMLInputElement).value);

    try {
      this.startDateString = this.datePipe.transform(new Date(startDate.getFullYear(), startDate.getMonth(), startDate.getDate() + 1), 'MMMM d, yyyy');
      this.finishDateString = this.datePipe.transform(new Date(finishDate.getFullYear(), finishDate.getMonth(), finishDate.getDate() + 1), 'MMMM d, yyyy');
    } catch (ex) {
      document.getElementById('empty-date-error').hidden = false;
      return;
    }

    let today = new Date();
    let earliestDate = new Date(2020, 9, 18);

    if (startDate > finishDate) {
      document.getElementById('date-error').hidden = false;
    } else if (startDate > today || finishDate > today
      || startDate < earliestDate || finishDate < earliestDate) {
      document.getElementById('invalid-date-error').hidden = false;
    } else if (!(document.getElementById('assessments-team') as HTMLInputElement).checked
      && !(document.getElementById('enterprise-team') as HTMLInputElement).checked) {
      document.getElementById('team-error').hidden = false;
    } else {
      document.getElementById('date-error').hidden = true;
      document.getElementById('team-error').hidden = true;
      document.getElementById('empty-date-error').hidden = true;
      document.getElementById('invalid-date-error').hidden = true;
      this.sendInfo();
      this.clearMessages();
    }
  }

  getToday() {
    const today = new Date();
    let day = today.getDate();
    let month = today.getMonth() + 1;
    const year = today.getFullYear();

    let dayString = day.toString();
    let monthString = month.toString();

    if (day < 10) {
      dayString = '0' + day;
    }
    if (month < 10) {
      monthString = '0' + month;
    }

    return year + '-' + monthString + '-' + dayString;
  }
}
