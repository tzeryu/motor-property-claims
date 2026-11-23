import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Worklist } from './worklist.component';

describe('Worklist', () => {
  let component: Worklist;
  let fixture: ComponentFixture<Worklist>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Worklist],
    }).compileComponents();

    fixture = TestBed.createComponent(Worklist);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
