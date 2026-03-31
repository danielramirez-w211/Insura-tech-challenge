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
import { NotificationsService } from '../services/notifications.service';
import { NotificationDto, NotificationFilters, NotificationStatus } from '../models/notification.model';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { StatusLabelPipe } from '../../../shared/pipes/status-label.pipe';

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
  template: `
    <app-page-header title="Centro de Notificaciones" />

    <div class="filters">
      <mat-form-field appearance="outline">
        <mat-label>Estado</mat-label>
        <mat-select [(ngModel)]="filters.status" (ngModelChange)="onFilterChange()">
          <mat-option [value]="undefined">Todos</mat-option>
          @for (s of statusOptions; track s) {
            <mat-option [value]="s">{{ s | statusLabel }}</mat-option>
          }
        </mat-select>
      </mat-form-field>
    </div>

    @if (service.loading()) {
      <app-loading-spinner [loading]="true" />
    } @else {
      <mat-table [dataSource]="dataSource">
        <ng-container matColumnDef="event">
          <mat-header-cell *matHeaderCellDef>Evento</mat-header-cell>
          <mat-cell *matCellDef="let row">{{ row.event | statusLabel }}</mat-cell>
        </ng-container>

        <ng-container matColumnDef="recipient">
          <mat-header-cell *matHeaderCellDef>Destinatario</mat-header-cell>
          <mat-cell *matCellDef="let row">{{ row.recipientName }}</mat-cell>
        </ng-container>

        <ng-container matColumnDef="subject">
          <mat-header-cell *matHeaderCellDef>Asunto</mat-header-cell>
          <mat-cell *matCellDef="let row">{{ row.subject }}</mat-cell>
        </ng-container>

        <ng-container matColumnDef="status">
          <mat-header-cell *matHeaderCellDef>Estado</mat-header-cell>
          <mat-cell *matCellDef="let row">
            <app-status-badge [status]="row.status" />
          </mat-cell>
        </ng-container>

        <ng-container matColumnDef="retryCount">
          <mat-header-cell *matHeaderCellDef>Reintentos</mat-header-cell>
          <mat-cell *matCellDef="let row">{{ row.retryCount }}</mat-cell>
        </ng-container>

        <ng-container matColumnDef="createdAt">
          <mat-header-cell *matHeaderCellDef>Fecha</mat-header-cell>
          <mat-cell *matCellDef="let row">{{ row.createdAt | date:'dd/MM/yyyy HH:mm' }}</mat-cell>
        </ng-container>

        <ng-container matColumnDef="actions">
          <mat-header-cell *matHeaderCellDef></mat-header-cell>
          <mat-cell *matCellDef="let row">
            @if (row.status === 'Failed') {
              <button mat-icon-button color="accent"
                (click)="retry(row)"
                aria-label="Reintentar notificación">
                <mat-icon>refresh</mat-icon>
              </button>
            }
          </mat-cell>
        </ng-container>

        <mat-header-row *matHeaderRowDef="displayedColumns" />
        <mat-row *matRowDef="let row; columns: displayedColumns;" />
      </mat-table>

      <mat-paginator
        [length]="service.totalCount()"
        [pageSize]="filters.pageSize"
        [pageSizeOptions]="[10, 25, 50]"
        (page)="onPageChange($event)"
        showFirstLastButtons />
    }
  `,
  styles: [`
    .filters { display: flex; gap: 16px; margin-bottom: 16px; }
    mat-table { width: 100%; }
  `],
})
export class NotificationsComponent implements OnInit {
  service = inject(NotificationsService);
  snackBar = inject(MatSnackBar);

  displayedColumns = ['event', 'recipient', 'subject', 'status', 'retryCount', 'createdAt', 'actions'];
  dataSource = new MatTableDataSource<NotificationDto>();

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

  retry(notification: NotificationDto) {
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
