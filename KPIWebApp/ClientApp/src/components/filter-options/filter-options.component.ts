import {Component, Inject} from '@angular/core';
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
  private startDate: any;
  private finishDate: any;
  today = Date.now().toString();

  constructor(datepipe: DatePipe, http: HttpClient, @Inject('BASE_URL') baseUrl: string, private messageService: FilterMessageService) {
    this.datePipe = datepipe;
  }

  submit() {
    this.reloadData();
  }

  sendDates() {
    this.messageService.sendMessage(this.startDate, this.finishDate, (document.getElementById('product') as HTMLInputElement).checked,
      (document.getElementById('engineering') as HTMLInputElement).checked, (document.getElementById('unanticipated') as HTMLInputElement).checked );
  }

  private clearMessages() {
    this.messageService.clearMessages();
  }

  reloadData() {
    let date = new Date((document.getElementById('start-date') as HTMLInputElement).value);
    this.startDate = this.datePipe.transform(new Date(date.getFullYear(), date.getMonth(), date.getDate() + 1), 'MMMM d, yyyy');
    date = new Date((document.getElementById('finish-date') as HTMLInputElement).value);
    this.finishDate = this.datePipe.transform(new Date(date.getFullYear(), date.getMonth(), date.getDate() + 1), 'MMMM d, yyyy');

    this.sendDates();
    this.clearMessages();
  }

  getToday() {
    const today = new Date();
    let day = today.getDate();
    let month = today.getMonth() + 1;
    const year = today.getFullYear();

    let dayString = day.toString();
    let monthString = month.toString();

    if(day<10){
      dayString = '0' + day;
    }
    if(month<10){
      monthString = '0' + month;
    }

    return year + '-' + monthString + '-' + dayString;
  }
}
