import { Component, inject } from '@angular/core';
import { RouterModule } from '@angular/router';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatBadgeModule } from '@angular/material/badge';
import { NotificationsCoreService } from '../../../features/notifications/core/service/notifications.service';

interface NavItem {
  label: string;
  icon: string;
  route: string;
}

@Component({
  selector: 'app-sidenav',
  standalone: true,
  imports: [RouterModule, MatListModule, MatIconModule, MatBadgeModule],
  templateUrl: './sidenav.component.html',
  styleUrl: './sidenav.component.css',
})
export class SidenavComponent {
  notificationsService = inject(NotificationsCoreService);

  navItems: NavItem[] = [
    { label: 'Dashboard',       icon: 'dashboard',       route: '/dashboard' },
    { label: 'Pólizas',         icon: 'policy',          route: '/policies' },
    { label: 'Siniestros',      icon: 'report_problem',  route: '/claims' },
    { label: 'Notificaciones',  icon: 'notifications',   route: '/notifications' },
  ];
}
