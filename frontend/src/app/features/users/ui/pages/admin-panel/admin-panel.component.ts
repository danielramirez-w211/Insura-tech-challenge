import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { UsersService } from '../../../core/services/users.service';
import { PageHeaderComponent } from '../../../../../shared/components/page-header/page-header.component';
import { LoadingSpinnerComponent } from '../../../../../shared/components/loading-spinner/loading-spinner.component';
import { UserFormComponent } from '../../blocks/user-form/user-form.component';
import { CreateLeaderRequest, UserDetail } from '../../../core/models/user.model';

@Component({
  selector: 'app-admin-panel',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatDialogModule,
    MatSnackBarModule,
    MatTooltipModule,
    PageHeaderComponent,
    LoadingSpinnerComponent,
    UserFormComponent,
  ],
  templateUrl: './admin-panel.component.html',
  styleUrl: './admin-panel.component.css',
})
export class AdminPanelComponent implements OnInit {
  private svc   = inject(UsersService);
  private snack = inject(MatSnackBar);

  loading          = signal(true);
  showForm         = signal(false);
  saving           = signal(false);
  dataSource       = new MatTableDataSource<UserDetail>();
  displayedColumns = ['name', 'email', 'status', 'actions'];

  ngOnInit() { this.load(); }

  load() {
    this.loading.set(true);
    this.svc.getLeaders().subscribe({
      next: (list) => {
        this.dataSource.data = list;
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  onCreateLeader(body: CreateLeaderRequest) {
    this.saving.set(true);
    this.svc.createLeader(body).subscribe({
      next: (created) => {
        this.dataSource.data = [created, ...this.dataSource.data];
        this.saving.set(false);
        this.showForm.set(false);
        this.snack.open('Líder creado. Revisa la consola del backend para la contraseña temporal.', 'Cerrar', { duration: 5000 });
      },
      error: (err) => {
        this.saving.set(false);
        const msg = err?.error?.detail ?? 'Error al crear el líder';
        this.snack.open(msg, 'Cerrar', { duration: 4000 });
      },
    });
  }

  toggle(leader: UserDetail) {
    const next = !leader.isActive;
    this.svc.toggleStatus(leader.id, next).subscribe({
      next: () => {
        leader.isActive = next;
        this.dataSource.data = [...this.dataSource.data];
        this.snack.open(next ? 'Líder activado' : 'Líder desactivado', 'Cerrar', { duration: 3000 });
      },
      error: () => this.snack.open('Error al cambiar el estado', 'Cerrar', { duration: 3000 }),
    });
  }
}
