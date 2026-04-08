import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { NotificationsCoreService } from '../../../core/service/notifications.service';
import { Notification, NotificationFilters, NotificationStatus } from '../../../core/models/notification.model';
import { StatusBadgeComponent } from '../../../../../shared/components/status-badge/status-badge.component';
import { PageHeaderComponent } from '../../../../../shared/components/page-header/page-header.component';
import { LoadingSpinnerComponent } from '../../../../../shared/components/loading-spinner/loading-spinner.component';
import { StatusLabelPipe } from '../../../../../shared/pipes/status-label.pipe';

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatSelectModule,
    MatFormFieldModule,
    MatButtonModule,
    MatIconModule,
    StatusBadgeComponent,
    PageHeaderComponent,
    LoadingSpinnerComponent,
    StatusLabelPipe,
  ],
  templateUrl: './notifications.component.html',
  styleUrl: './notifications.component.css',
})
export class NotificationsComponent implements OnInit {
  service = inject(NotificationsCoreService);
  snackBar = inject(MatSnackBar);

  displayedColumns = ['event', 'recipient', 'subject', 'status', 'retryCount', 'createdAt', 'actions'];
  dataSource = new MatTableDataSource<Notification>();

  statusOptions: NotificationStatus[] = ['Pending', 'Sent', 'Failed'];
  filters: NotificationFilters = { page: 1, pageSize: 10 };

  ngOnInit() {
    this.loadNotifications();
  }

  loadNotifications() {
    this.service.loading.set(true);
    this.service.getNotifications(this.filters).subscribe({
      next: (result) => {
        this.dataSource.data = result.items;
        this.service.notifications.set(result.items);
        this.service.totalCount.set(result.totalCount);
        this.service.loading.set(false);
      },
      error: () => this.service.loading.set(false),
    });
  }

  onFilterChange() {
    this.filters.page = 1;
    this.loadNotifications();
  }

  onPageChange(event: PageEvent) {
    this.filters.page = event.pageIndex + 1;
    this.filters.pageSize = event.pageSize;
    this.loadNotifications();
  }

  retry(notification: Notification) {
    this.service.retryNotification(notification.id).subscribe({
      next: (updated) => {
        const data = this.dataSource.data.map((n) =>
          n.id === updated.id ? updated : n
        );
        this.dataSource.data = data;
        this.service.notifications.set(data);
        this.snackBar.open('Reintento programado', 'Cerrar', { duration: 3000 });
      },
    });
  }
}

