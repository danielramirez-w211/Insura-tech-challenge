import { Component, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ClientsService } from '../../../core/service/clients.service';
import { ClientSummary } from '../../../core/models/client.model';
import { PageHeaderComponent } from '../../../../../shared/components/page-header/page-header.component';
import { LoadingSpinnerComponent } from '../../../../../shared/components/loading-spinner/loading-spinner.component';

@Component({
  selector: 'app-clients-list',
  standalone: true,
  imports: [
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatTooltipModule,
    PageHeaderComponent,
    LoadingSpinnerComponent,
  ],
  templateUrl: './clients-list.component.html',
  styleUrl: './clients-list.component.css',
})
export class ClientsListComponent implements OnInit {
  private readonly svc    = inject(ClientsService);
  private readonly router = inject(Router);

  loading    = signal(true);
  error      = signal<string | null>(null);
  dataSource = new MatTableDataSource<ClientSummary>();

  displayedColumns = ['name', 'documentType', 'documentId', 'city', 'policyCount', 'actions'];

  ngOnInit(): void {
    this.svc.getMyClients().subscribe({
      next: data => {
        this.dataSource.data = data;
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Error al cargar los clientes. Intenta de nuevo.');
        this.loading.set(false);
      },
    });
  }

  viewPolicies(client: ClientSummary): void {
    this.router.navigate(['/policies'], { queryParams: { documentId: client.documentId } });
  }
}
