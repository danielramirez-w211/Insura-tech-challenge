import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { UsersService } from '../../../core/services/users.service';
import { PageHeaderComponent } from '../../../../../shared/components/page-header/page-header.component';
import { LoadingSpinnerComponent } from '../../../../../shared/components/loading-spinner/loading-spinner.component';
import { AdvisorSummary } from '../../../core/models/user.model';

@Component({
  selector: 'app-leader-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatChipsModule,
    MatSnackBarModule,
    MatTooltipModule,
    PageHeaderComponent,
    LoadingSpinnerComponent,
  ],
  templateUrl: './leader-dashboard.component.html',
  styleUrl: './leader-dashboard.component.css',
})
export class LeaderDashboardComponent implements OnInit {
  private svc   = inject(UsersService);
  private snack = inject(MatSnackBar);

  loading    = signal(true);
  dataSource = new MatTableDataSource<AdvisorSummary>();
  displayedColumns = ['advisorCode', 'name', 'email', 'salesCount', 'status', 'actions'];

  ngOnInit() { this.load(); }

  load() {
    this.loading.set(true);
    this.svc.getMyAdvisors().subscribe({
      next: (list) => {
        this.dataSource.data = list;
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  toggle(advisor: AdvisorSummary) {
    const next = !advisor.isActive;
    this.svc.toggleStatus(advisor.id, next).subscribe({
      next: () => {
        advisor.isActive = next;
        this.dataSource.data = [...this.dataSource.data];
        const msg = next ? 'Asesor activado' : 'Asesor desactivado';
        this.snack.open(msg, 'Cerrar', { duration: 3000 });
      },
      error: () => this.snack.open('Error al cambiar el estado', 'Cerrar', { duration: 3000 }),
    });
  }
}
