import {Component, Inject, ViewChild} from '@angular/core';
import {DatePipe} from "@angular/common";
import {HomeComponent} from '../../app/home/home.component';
import {HttpClient} from '@angular/common/http';

@Component({
  selector: 'app-filter-options',
  templateUrl: './filter-options.component.html',
  styleUrls: ['./filter-options.component.css'],
  providers: [DatePipe]
})
export class FilterOptionsComponent {
  isExpanded = false;
  private datePipe: DatePipe;
  private startDate: string;
  private finishDate: string;


  constructor(datepipe: DatePipe, http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.datePipe = datepipe;
  }

  submit() {
    this.reloadData();
  }

  reloadData() {
    let date = new Date((document.getElementById("start-date") as HTMLInputElement).value);
    this.startDate = this.datePipe.transform(new Date(date.getFullYear(), date.getMonth(), date.getDate() + 1) , 'MMMM d, yyyy');
    date = new Date((document.getElementById("end-date") as HTMLInputElement).value);
    this.finishDate = this.datePipe.transform(new Date(date.getFullYear(), date.getMonth(), date.getDate() + 1) , 'MMMM d, yyyy');
  }
}
