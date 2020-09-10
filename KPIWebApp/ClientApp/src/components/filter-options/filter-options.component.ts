import { Component } from '@angular/core';
import {DatePipe} from "@angular/common";

@Component({
  selector: 'app-filter-options',
  templateUrl: './filter-options.component.html',
  styleUrls: ['./filter-options.component.css'],
  providers: [DatePipe]
})
export class FilterOptionsComponent {
  isExpanded = false;
  startDate: string = "The Beginning of Time";
  endDate: string = "The Present Day";
  private datePipe: DatePipe;

  constructor(datepipe: DatePipe) {
    this.startDate = "The Beginning of Time";
    this.endDate = "The Present Day";
    this.datePipe = datepipe;
  }

  submit() {
    let date = new Date((document.getElementById("start-date") as HTMLInputElement).value);
    this.startDate = this.datePipe.transform(new Date(date.getFullYear(), date.getMonth(), date.getDate() + 1) , 'MMMM d, yyyy');
    date = new Date((document.getElementById("end-date") as HTMLInputElement).value);
    this.endDate = this.datePipe.transform(new Date(date.getFullYear(), date.getMonth(), date.getDate() + 1) , 'MMMM d, yyyy');
  }
}

export class DateRange {
  startDate: string = "The Beginning of Time";
  endDate: string = "The Present Day";
}
