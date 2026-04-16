import { Component, computed, inject } from '@angular/core';
import { RouterModule } from '@angular/router';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatBadgeModule } from '@angular/material/badge';
import { NotificationsCoreService } from '../../../features/notifications/core/service/notifications.service';
import { AuthService } from '../../../core/services/auth.service';

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
  private auth         = inject(AuthService);

  readonly navItems = computed<NavItem[]>(() => {
    const role = this.auth.role();
    const items: NavItem[] = [];

    if (role === 'Admin') {
      items.push({ label: 'Panel Admin',    icon: 'admin_panel_settings', route: '/users/admin' });
    }

    if (role === 'Leader') {
      items.push({ label: 'Mi Equipo',      icon: 'group',                route: '/users/team' });
    }

    items.push({ label: 'Dashboard',        icon: 'dashboard',            route: '/dashboard' });
    items.push({ label: 'Pólizas',          icon: 'policy',               route: '/policies' });
    items.push({ label: 'Siniestros',       icon: 'report_problem',       route: '/claims' });
    items.push({ label: 'Notificaciones',   icon: 'notifications',        route: '/notifications' });
    items.push({ label: 'Mi Perfil',        icon: 'account_circle',       route: '/users/profile' });

    return items;
  });
}
