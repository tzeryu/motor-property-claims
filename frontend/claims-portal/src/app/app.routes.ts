import { Routes } from '@angular/router';

import { PagePlaceholderComponent } from './shared/ui/page-placeholder/page-placeholder.component';

import { WorkspaceLayoutComponent } from './core/layouts/workspace-layout/workspace-layout.component';

import { LoginComponent } from './features/auth/pages/login/login.component';

import { roleGuard } from './core/auth/guards/role.guard';
import { guestGuard } from './core/auth/guards/guest.guard';

import { DashboardComponent } from './features/claimant/pages/dashboard/dashboard.component';

import { MyClaimsComponent } from './features/claimant/pages/my-claims/my-claims.component';

import { ClaimSubmissionComponent } from './features/claimant/pages/claim-submission/claim-submission.component';

import { ClaimDetailsComponent } from './features/claimant/pages/claim-details/claim-details.component';

import { WorklistComponent } from './features/officer/pages/worklist/worklist.component';

import { ClaimReviewComponent } from './features/officer/pages/claim-review/claim-review.component';

import { DashboardComponent as ManagerDashboardComponent } from './features/manager/pages/dashboard/dashboard.component';

import {
  SettlementManagementComponent
} from './features/shared/pages/settlement-management/settlement-management.component';

export const routes: Routes = [

  {
    path: 'login',
    component: LoginComponent,
    canActivate: [guestGuard],
    title: 'Sign In | Claims Portal'
  },

  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'login'
  },

  {
    path: '',
    component: WorkspaceLayoutComponent,
    children: [

      // CLAIMANT ROUTES

      {
        path: 'claimant/dashboard',
        component: DashboardComponent,
        canActivate: [roleGuard],
        title: 'Claimant Dashboard',
        data: {
          roles: ['Claimant']
        }
      },

      {
        path: 'claimant/claims',
        component: MyClaimsComponent,
        canActivate: [roleGuard],
        title: 'My Claims',
        data: {
          roles: ['Claimant']
        }
      },

      {
        path: 'claimant/claims/new',
        component: ClaimSubmissionComponent,
        canActivate: [roleGuard],
        title: 'Submit Claim',
        data: {
          roles: ['Claimant']
        }
      },

      {
        path: 'claimant/claims/:id',
        component: ClaimDetailsComponent,
        canActivate: [roleGuard],
        data: { roles: ['Claimant'] }
      },

      // CLAIMS OFFICER ROUTES

      {
        path: 'officer/worklist',
        component: WorklistComponent,
        canActivate: [roleGuard],
        data: { roles: ['ClaimsOfficer'] }
      },

      {
        path: 'officer/claims/:id',
        component: ClaimReviewComponent,
        canActivate: [roleGuard],
        data: { roles: ['ClaimsOfficer'] }
      },

      // MANAGER ROUTES

      {
        path: 'manager/dashboard',
        component: ManagerDashboardComponent,
        canActivate: [roleGuard],
        data: { roles: ['Manager'] }
      },

      {
        path: 'officer/settlements',
        component: SettlementManagementComponent,
        canActivate: [roleGuard],
        data: { roles: ['ClaimsOfficer'] }
      },
      {
        path: 'manager/settlements',
        component: SettlementManagementComponent,
        canActivate: [roleGuard],
        data: { roles: ['Manager'] }
      },

    ]
  },

  {
    path: '**',
    component: PagePlaceholderComponent,
    title: 'Page Not Found',
    data: {
      page: {
        section: 'ERROR',
        title: 'Page Not Found',
        description: 'The page you are looking for does not exist.',
        features: [
          'Check the URL and try again.'
        ]
      }
    }
  }

];
