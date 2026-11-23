import { Component, inject } from '@angular/core';

import {
  RouterLink,
  RouterLinkActive,
  RouterOutlet
} from '@angular/router';

import { AsyncPipe } from '@angular/common';

import { AuthService } from '../../auth/services/auth.service';

@Component({
  selector: 'app-workspace-layout',
  standalone: true,
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    AsyncPipe
  ],
  templateUrl: './workspace-layout.component.html',
  styleUrl: './workspace-layout.component.scss'
})
export class WorkspaceLayoutComponent {

  readonly authService = inject(AuthService);

  readonly currentUser$ = this.authService.currentUser$;

  logout(): void {
    this.authService.logout();
  }
}
