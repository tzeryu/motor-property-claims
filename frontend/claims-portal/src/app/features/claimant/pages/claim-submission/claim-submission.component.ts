import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal
} from '@angular/core';

import { HttpErrorResponse } from '@angular/common/http';

import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators
} from '@angular/forms';

import { Router, RouterLink } from '@angular/router';

import { finalize } from 'rxjs';

import { AuthService } from '../../../../core/auth/services/auth.service';

import { ClaimsApiService } from '../../../../core/claims/services/claims-api.service';

import {
  ClaimType,
  SubmitClaimRequest,
  SubmitClaimResult
} from '../../../../core/claims/models/claim.models';

function claimDateValidator(
  control: AbstractControl
): ValidationErrors | null {

  const startDate = control.get('coverageStartDate')?.value as string;
  const endDate = control.get('coverageEndDate')?.value as string;
  const incidentDate = control.get('incidentDate')?.value as string;

  if (!startDate || !endDate || !incidentDate) {
    return null;
  }

  const errors: ValidationErrors = {};

  if (startDate > endDate) {
    errors['invalidCoveragePeriod'] = true;
  }

  if (incidentDate < startDate || incidentDate > endDate) {
    errors['incidentOutsideCoverage'] = true;
  }

  const today = new Date();
  const localToday = [
    today.getFullYear(),
    String(today.getMonth() + 1).padStart(2, '0'),
    String(today.getDate()).padStart(2, '0')
  ].join('-');

  if (incidentDate > localToday) {
    errors['incidentInFuture'] = true;
  }

  return Object.keys(errors).length ? errors : null;
}

@Component({
  selector: 'app-claim-submission',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink
  ],
  templateUrl: './claim-submission.component.html',
  styleUrl: './claim-submission.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ClaimSubmissionComponent {

  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly claimsApi = inject(ClaimsApiService);
  private readonly router = inject(Router);

  readonly today = this.getLocalDate();

  readonly claimForm = this.fb.nonNullable.group({

    claimType: this.fb.nonNullable.control<ClaimType>(
      'motor'
    ),

    // Claimant information
    claimantName: ['', Validators.required],
    claimantEmail: ['', [
      Validators.required,
      Validators.email
    ]],
    claimantPhone: ['', Validators.required],

    // Policy information
    policyNumber: ['', Validators.required],
    policyholderName: ['', Validators.required],
    coverageStartDate: ['', Validators.required],
    coverageEndDate: ['', Validators.required],
    insuredAmount: [50000, [
      Validators.required,
      Validators.min(0.01)
    ]],
    currencyCode: ['MYR', [
      Validators.required,
      Validators.pattern(/^[A-Za-z]{3}$/)
    ]],

    // Incident information
    incidentDate: ['', Validators.required],
    incidentLocation: ['', Validators.required],
    incidentDescription: ['', Validators.required],

    // Motor-specific information
    vehicleRegistrationNumber: [''],
    vehicleMake: [''],
    vehicleModel: [''],

    // Property-specific information
    propertyAddress: [''],
    propertyType: [''],
    damageDescription: ['']

  }, {
    validators: claimDateValidator
  });

  readonly isSubmitting = signal(false);

  readonly errorMessage = signal('');

  readonly apiValidationErrors = signal<string[]>([]);

  readonly submissionResult = signal<SubmitClaimResult | null>(null);

  constructor() {

    // Prepopulate claimant name and email from the logged-in user.
    const user = this.authService.currentUser;

    if (user) {
      this.claimForm.patchValue({
        claimantName: user.fullName,
        claimantEmail: user.email
      });
    }

    this.applyClaimTypeValidators();
  }

  get selectedClaimType(): ClaimType {
    return this.claimForm.controls.claimType.value;
  }

  onClaimTypeChange(): void {

    this.applyClaimTypeValidators();

    this.errorMessage.set('');
    this.apiValidationErrors.set([]);
  }

  private applyClaimTypeValidators(): void {

    const vehicleRegistration =
      this.claimForm.controls.vehicleRegistrationNumber;

    const propertyAddress =
      this.claimForm.controls.propertyAddress;

    const propertyType =
      this.claimForm.controls.propertyType;

    const damageDescription =
      this.claimForm.controls.damageDescription;

    vehicleRegistration.clearValidators();

    propertyAddress.clearValidators();
    propertyType.clearValidators();
    damageDescription.clearValidators();

    if (this.selectedClaimType === 'motor') {

      vehicleRegistration.setValidators([
        Validators.required
      ]);

    } else {

      propertyAddress.setValidators([
        Validators.required
      ]);

      propertyType.setValidators([
        Validators.required
      ]);

      damageDescription.setValidators([
        Validators.required
      ]);
    }

    vehicleRegistration.updateValueAndValidity({
      emitEvent: false
    });

    propertyAddress.updateValueAndValidity({
      emitEvent: false
    });

    propertyType.updateValueAndValidity({
      emitEvent: false
    });

    damageDescription.updateValueAndValidity({
      emitEvent: false
    });

  }

  submit(): void {

    if (this.claimForm.invalid) {

      this.claimForm.markAllAsTouched();

      return;
    }

    this.isSubmitting.set(true);

    this.errorMessage.set('');

    this.apiValidationErrors.set([]);

    this.submissionResult.set(null);

    const request = this.buildRequest();

    this.claimsApi.submitClaim(request)
      .pipe(
        finalize(() => {
          this.isSubmitting.set(false);
        })
      )
      .subscribe({

        next: result => {

          this.submissionResult.set(result);

        },

        error: (error: unknown) => {

          this.handleSubmissionError(error);

        }

      });

  }

  private buildRequest(): SubmitClaimRequest {

    const value = this.claimForm.getRawValue();

    const request: SubmitClaimRequest = {

      claimType: value.claimType,

      claimantName: value.claimantName.trim(),
      claimantEmail: value.claimantEmail.trim(),
      claimantPhone: value.claimantPhone.trim(),

      policyDetails: {
        policyNumber: value.policyNumber.trim(),
        policyholderName: value.policyholderName.trim(),
        coverageStartDate: value.coverageStartDate,
        coverageEndDate: value.coverageEndDate,
        insuredAmount: value.insuredAmount,
        currencyCode: value.currencyCode.trim().toUpperCase()
      },

      incidentDate: value.incidentDate,
      incidentLocation: value.incidentLocation.trim(),
      incidentDescription: value.incidentDescription.trim()

    };

    if (value.claimType === 'motor') {

      request.motorDetails = {
        vehicleRegistrationNumber:
          value.vehicleRegistrationNumber.trim(),

        vehicleMake: value.vehicleMake.trim(),

        vehicleModel: value.vehicleModel.trim()
      };

    } else {

      request.propertyDetails = {
        propertyAddress: value.propertyAddress.trim(),
        propertyType: value.propertyType.trim(),
        damageDescription: value.damageDescription.trim()
      };

    }

    return request;
  }

  private handleSubmissionError(error: unknown): void {

    if (error instanceof HttpErrorResponse) {

      if (error.status === 400) {

        const response = error.error as {
          title?: string;
          detail?: string;
          errors?: Record<string, string[]>;
        };

        const messages = Object.values(
          response?.errors ?? {}
        ).flat();

        this.apiValidationErrors.set(messages);

        this.errorMessage.set(
          messages.length
            ? 'Please review the validation errors below.'
            : response?.detail ||
            response?.title ||
            'Your claim could not be submitted.'
        );

        return;
      }

      if (error.status === 0) {

        this.errorMessage.set(
          'Unable to connect to the server. Please try again.'
        );

        return;
      }

      this.errorMessage.set(
        'An unexpected error occurred while submitting your claim.'
      );

      return;
    }

    this.errorMessage.set(
      'Unable to submit your claim. Please try again.'
    );
  }

  private getLocalDate(): string {

    const now = new Date();

    return [
      now.getFullYear(),
      String(now.getMonth() + 1).padStart(2, '0'),
      String(now.getDate()).padStart(2, '0')
    ].join('-');
  }

}
