import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';

import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';

import { PagedClaimsResult } from '../models/claim.models';

@Injectable({
  providedIn: 'root'
})
export class ClaimsApiService {

  private readonly http = inject(HttpClient);

  private readonly apiUrl =
    `${environment.apiBaseUrl}/claims`;

  getMyClaims(
    pageNumber: number = 1,
    pageSize: number = 20,
    status?: string
  ): Observable<PagedClaimsResult> {

    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (status) {
      params = params.set('status', status);
    }

    return this.http.get<PagedClaimsResult>(
      `${this.apiUrl}/mine`,
      { params }
    );
  }
}
