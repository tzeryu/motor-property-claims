import { Routes } from '@angular/router';

import { PagePlaceholderComponent } from './shared/ui/page-placeholder/page-placeholder.component';

import { WorkspaceLayoutComponent } from './core/layouts/workspace-layout/workspace-layout.component';

import { LoginComponent } from './features/auth/pages/login/login.component';

import { roleGuard } from './core/auth/guards/role.guard';
import { guestGuard } from './core/auth/guards/guest.guard';

import { DashboardComponent } from './features/claimant/pages/dashboard/dashboard.component';

import { MyClaimsComponent } from './features/claimant/pages/my-claims/my-claims.component';

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
        component: PagePlaceholderComponent,
        canActivate: [roleGuard],
        title: 'Submit Claim',
        data: {
          roles: ['Claimant'],
          page: {
            section: 'CLAIMANT PORTAL',
            title: 'Submit a New Claim',
            description: 'Provide policy and incident details to initiate a claim.',
            features: [
              'Select motor or property claim',
              'Enter policy information',
              'Provide incident details'
            ]
          }
        }
      },

      // CLAIMS OFFICER ROUTES

      {
        path: 'officer/worklist',
        component: PagePlaceholderComponent,
        canActivate: [roleGuard],
        title: 'Claims Worklist',
        data: {
          roles: ['ClaimsOfficer'],
          page: {
            section: 'CLAIMS OPERATIONS',
            title: 'Claims Worklist',
            description: 'Review submitted claims and manage your assigned workload.',
            features: [
              'View unassigned claims',
              'View claims assigned to you',
              'Start claim reviews'
            ]
          }
        }
      },

      {
        path: 'officer/claims/:id',
        component: PagePlaceholderComponent,
        canActivate: [roleGuard],
        title: 'Claim Review',
        data: {
          roles: ['ClaimsOfficer'],
          page: {
            section: 'CLAIMS OPERATIONS',
            title: 'Claim Review',
            description: 'Evaluate claim information and make an approval recommendation.',
            features: [
              'Review claimant and policy information',
              'Request additional information',
              'Approve or reject claims'
            ]
          }
        }
      },

      // MANAGER ROUTES

      {
        path: 'manager/dashboard',
        component: PagePlaceholderComponent,
        canActivate: [roleGuard],
        title: 'Management Overview',
        data: {
          roles: ['Manager'],
          page: {
            section: 'MANAGEMENT',
            title: 'Management Overview',
            description: 'Monitor claims performance and financial exposure.',
            features: [
              'View outstanding claim counts',
              'Review liability by currency',
              'Approve escalated claims'
            ]
          }
        }
      }

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
