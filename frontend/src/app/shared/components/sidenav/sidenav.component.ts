import { Component, inject } from '@angular/core';
import { RouterModule } from '@angular/router';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatBadgeModule } from '@angular/material/badge';
import { NotificationsService } from '../../../features/notifications/services/notifications.service';

interface NavItem {
  label: string;
  icon: string;
  route: string;
}

@Component({
  selector: 'app-sidenav',
  standalone: true,
  imports: [RouterModule, MatListModule, MatIconModule, MatBadgeModule],
  template: `
    <mat-nav-list>
      @for (item of navItems; track item.route) {
        <a mat-list-item [routerLink]="item.route" routerLinkActive="active-link">
          @if (item.route === '/notifications' && notificationsService.failedCount() > 0) {
            <mat-icon
              matListItemIcon
              [matBadge]="notificationsService.failedCount()"
              matBadgeColor="warn"
              matBadgeSize="small">
              {{ item.icon }}
            </mat-icon>
          } @else {
            <mat-icon matListItemIcon>{{ item.icon }}</mat-icon>
          }
          <span matListItemTitle>{{ item.label }}</span>
        </a>
      }
    </mat-nav-list>
  `,
  styles: [`
    :host { display: block; }
    .active-link { background-color: rgba(63,81,181,0.1); }
  `],
})
export class SidenavComponent {
  notificationsService = inject(NotificationsService);

  navItems: NavItem[] = [
    { label: 'Dashboard',       icon: 'dashboard',       route: '/dashboard' },
    { label: 'Pólizas',         icon: 'policy',          route: '/policies' },
    { label: 'Siniestros',      icon: 'report_problem',  route: '/claims' },
    { label: 'Notificaciones',  icon: 'notifications',   route: '/notifications' },
  ];
}
