import { Component, inject, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PoliciesCoreService } from '../core/service/policies.service';
import { Policy } from '../core/models/policy.model';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { StatusLabelPipe } from '../../../shared/pipes/status-label.pipe';

@Component({
  selector: 'app-policy-detail',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    StatusBadgeComponent,
    PageHeaderComponent,
    LoadingSpinnerComponent,
    StatusLabelPipe,
  ],
  templateUrl: './policy-detail.component.html',
  styleUrl: './policy-detail.component.css',
})
export class PolicyDetailComponent implements OnInit {
  @Input() id!: string;

  service  = inject(PoliciesCoreService);
  router   = inject(Router);
  snackBar = inject(MatSnackBar);

  policy: Policy | null = null;
  loading = false;

  ngOnInit(): void {
    this.loading = true;
    this.service.getById(this.id).subscribe({
      next: (p) => { this.policy = p as unknown as Policy; this.loading = false; },
      error: () => { this.loading = false; },
    });
  }

  activate(): void {
    if (!this.policy) return;
    this.service.activate(this.policy.id).subscribe({
      next: (updated) => {
        this.policy = updated as unknown as Policy;
        this.snackBar.open('Póliza activada exitosamente', 'Cerrar', { duration: 3000 });
      },
    });
  }
}
