import {FilterOptionsComponent} from './filter-options.component';
import {async, TestBed} from "@angular/core/testing";

describe("FilterOptionsComponent", () => {
  let component: FilterOptionsComponent;
  let fixture;
  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [FilterOptionsComponent]
    }
    )}));
  beforeEach(() => {
    fixture = TestBed.createComponent(FilterOptionsComponent);
    component = fixture.componentInstance;
  });

  it("should run the test", function () {
    expect(true).toBeTruthy();
  });

  it("should submit", () => {
    component.submit();
  });
  }
)
