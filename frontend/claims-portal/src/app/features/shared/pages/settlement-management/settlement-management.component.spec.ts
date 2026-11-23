import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SettlementManagement } from './settlement-management.component';

describe('SettlementManagement', () => {
  let component: SettlementManagement;
  let fixture: ComponentFixture<SettlementManagement>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SettlementManagement],
    }).compileComponents();

    fixture = TestBed.createComponent(SettlementManagement);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
