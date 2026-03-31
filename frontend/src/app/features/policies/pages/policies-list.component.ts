import { Component, inject, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PoliciesService } from '../services/policies.service';
import { PolicyDto, PolicyFilters, PolicyStatus, PolicyType } from '../models/policy.model';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { StatusLabelPipe } from '../../../shared/pipes/status-label.pipe';

@Component({
  selector: 'app-policies-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatSelectModule,
    MatFormFieldModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    StatusBadgeComponent,
    PageHeaderComponent,
    LoadingSpinnerComponent,
    StatusLabelPipe,
  ],
  template: `
    <app-page-header
      title="Pólizas"
      actionLabel="Nueva Póliza"
      actionIcon="add"
      (action)="router.navigate(['/policies/new'])" />

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

      <mat-form-field appearance="outline">
        <mat-label>Tipo</mat-label>
        <mat-select [(ngModel)]="filters.type" (ngModelChange)="onFilterChange()">
          <mat-option [value]="undefined">Todos</mat-option>
          @for (t of typeOptions; track t) {
            <mat-option [value]="t">{{ t | statusLabel }}</mat-option>
          }
        </mat-select>
      </mat-form-field>
    </div>

    @if (service.loading()) {
      <app-loading-spinner [loading]="true" />
    } @else {
      <mat-table [dataSource]="dataSource">
        <ng-container matColumnDef="policyNumber">
          <mat-header-cell *matHeaderCellDef>Número</mat-header-cell>
          <mat-cell *matCellDef="let row">{{ row.policyNumber }}</mat-cell>
        </ng-container>

        <ng-container matColumnDef="insured">
          <mat-header-cell *matHeaderCellDef>Asegurado</mat-header-cell>
          <mat-cell *matCellDef="let row">{{ row.insured.name }}</mat-cell>
        </ng-container>

        <ng-container matColumnDef="type">
          <mat-header-cell *matHeaderCellDef>Tipo</mat-header-cell>
          <mat-cell *matCellDef="let row">{{ row.type | statusLabel }}</mat-cell>
        </ng-container>

        <ng-container matColumnDef="status">
          <mat-header-cell *matHeaderCellDef>Estado</mat-header-cell>
          <mat-cell *matCellDef="let row">
            <app-status-badge [status]="row.status" />
          </mat-cell>
        </ng-container>

        <ng-container matColumnDef="insuredAmount">
          <mat-header-cell *matHeaderCellDef>Monto</mat-header-cell>
          <mat-cell *matCellDef="let row">{{ row.insuredAmount | currency:'USD' }}</mat-cell>
        </ng-container>

        <ng-container matColumnDef="actions">
          <mat-header-cell *matHeaderCellDef></mat-header-cell>
          <mat-cell *matCellDef="let row">
            <button mat-icon-button [matMenuTriggerFor]="menu" aria-label="Acciones">
              <mat-icon>more_vert</mat-icon>
            </button>
            <mat-menu #menu>
              <button mat-menu-item (click)="router.navigate(['/policies', row.id])">
                <mat-icon>visibility</mat-icon> Ver detalle
              </button>
              @if (row.status === 'Pending') {
                <button mat-menu-item (click)="activate(row)">
                  <mat-icon>play_circle</mat-icon> Activar
                </button>
              }
              <button mat-menu-item (click)="viewClaims(row)">
                <mat-icon>report_problem</mat-icon> Ver siniestros
              </button>
            </mat-menu>
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
    .filters {
      display: flex;
      gap: 16px;
      margin-bottom: 16px;
      flex-wrap: wrap;
    }
    mat-form-field { min-width: 160px; }
    mat-table { width: 100%; }
  `],
})
export class PoliciesListComponent implements OnInit {
  service = inject(PoliciesService);
  router = inject(Router);
  snackBar = inject(MatSnackBar);

  displayedColumns = ['policyNumber', 'insured', 'type', 'status', 'insuredAmount', 'actions'];
  dataSource = new MatTableDataSource<PolicyDto>();

  statusOptions: PolicyStatus[] = ['Pending', 'Active', 'Suspended', 'Expired', 'Cancelled'];
  typeOptions: PolicyType[] = ['Life', 'Health', 'Vehicle', 'Home', 'Travel'];

  filters: PolicyFilters = { page: 1, pageSize: 10 };

  ngOnInit() {
    this.loadPolicies();
  }

  loadPolicies() {
    this.service.loading.set(true);
    this.service.getPolicies(this.filters).subscribe({
      next: (result) => {
        this.dataSource.data = result.items;
        this.service.totalCount.set(result.totalCount);
        this.service.loading.set(false);
      },
      error: () => this.service.loading.set(false),
    });
  }

  onFilterChange() {
    this.filters.page = 1;
    this.loadPolicies();
  }

  onPageChange(event: PageEvent) {
    this.filters.page = event.pageIndex + 1;
    this.filters.pageSize = event.pageSize;
    this.loadPolicies();
  }

  activate(policy: PolicyDto) {
    this.service.activatePolicy(policy.id).subscribe({
      next: (updated) => {
        const idx = this.dataSource.data.findIndex((p) => p.id === policy.id);
        if (idx !== -1) {
          const data = [...this.dataSource.data];
          data[idx] = updated;
          this.dataSource.data = data;
        }
        this.snackBar.open('Póliza activada exitosamente', 'Cerrar', { duration: 3000 });
      },
    });
  }

  viewClaims(policy: PolicyDto) {
    this.router.navigate(['/claims'], { queryParams: { policyId: policy.id } });
  }
}
