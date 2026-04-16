import { Component, OnDestroy, OnInit, computed, inject, output, signal,} from '@angular/core';
import { RouterModule } from '@angular/router';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, filter, switchMap, takeUntil } from 'rxjs/operators';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatBadgeModule } from '@angular/material/badge';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDividerModule } from '@angular/material/divider';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { PoliciesCoreService } from '../../../features/policies/core/service/policies.service';
import { ClaimsCoreService } from '../../../features/claims/core/service/claims.service';
import { PolicyResponse } from '../../../features/policies/core/resource/policy-response.resource';
import {
  PolicySearchResult,
  SearchResultOverlayComponent,
} from './search-result-overlay/search-result-overlay.component';

type SearchMode = 'name' | 'document';

const DOC_TYPES = ['CC', 'CE', 'TI', 'PP', 'RC'] as const;

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [
    RouterModule,
    FormsModule,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatBadgeModule,
    MatTooltipModule,
    MatDividerModule,
    SearchResultOverlayComponent,
  ],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css',
})
export class HeaderComponent implements OnInit, OnDestroy {
  private readonly auth        = inject(AuthService);
  private readonly router      = inject(Router);
  private readonly policiesSvc = inject(PoliciesCoreService);
  private readonly claimsSvc   = inject(ClaimsCoreService);

  // ── Salida para toggle del sidenav ─────────────────────────────────────────
  readonly menuToggle = output<void>();

  // ── Estado de autenticación ────────────────────────────────────────────────
  readonly role        = this.auth.role;
  readonly currentUser = this.auth.currentUser;

  readonly userInitials = computed(() => {
    const u = this.currentUser();
    if (!u) return '';
    return (u.firstName.charAt(0) + u.lastName.charAt(0)).toUpperCase();
  });

  readonly displayName = computed(() => {
    const u = this.currentUser();
    return u ? `${u.firstName} ${u.lastName}` : '';
  });

  readonly roleLabel = computed(() => {
    const map: Record<string, string> = {
      Admin:   'Administrador',
      Leader:  'Líder',
      Advisor: 'Asesor',
    };
    return this.role() ? map[this.role()!] : '';
  });

  // ── Búsqueda (solo Asesor) ─────────────────────────────────────────────────
  readonly docTypes    = DOC_TYPES;
  readonly searchMode  = signal<SearchMode>('name');
  readonly nameQuery   = signal('');
  readonly docType     = signal('');
  readonly docId       = signal('');
  readonly results     = signal<PolicySearchResult[]>([]);
  readonly searching   = signal(false);
  readonly showOverlay = signal(false);

  private readonly nameSubject$ = new Subject<string>();
  private readonly destroy$     = new Subject<void>();

  // ── Badge de claims pendientes (solo Líder) ────────────────────────────────
  readonly pendingCount = signal(0);

  // ── Lifecycle ──────────────────────────────────────────────────────────────
  ngOnInit(): void {
    this.nameSubject$.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      filter(q => q.length >= 2),
      switchMap(q => {
        this.searching.set(true);
        return this.policiesSvc.getAll({ insuredSearch: q, page: 1, pageSize: 10 });
      }),
      takeUntil(this.destroy$),
    ).subscribe({
      next: res => {
        this.results.set(res.items.map(this.toSearchResult));
        this.searching.set(false);
        this.showOverlay.set(true);
      },
      error: () => this.searching.set(false),
    });

    if (this.role() === 'Leader') {
      this.loadPendingCount();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ── Búsqueda por nombre ────────────────────────────────────────────────────
  onNameInput(value: string): void {
    this.nameQuery.set(value);
    if (value.length < 2) {
      this.showOverlay.set(false);
      this.results.set([]);
      return;
    }
    this.nameSubject$.next(value);
  }

  // ── Búsqueda por documento (acción explícita) ──────────────────────────────
  onDocumentSearch(): void {
    const type = this.docType();
    const id   = this.docId();
    if (!type || !id) return;
    this.searching.set(true);
    this.policiesSvc.getAll({
      insuredDocumentType: type,
      documentId:          id,
      page:                1,
      pageSize:            20,
    }).subscribe({
      next: res => {
        this.results.set(res.items.map(this.toSearchResult));
        this.searching.set(false);
        this.showOverlay.set(true);
      },
      error: () => this.searching.set(false),
    });
  }

  // ── Cambio de modo de búsqueda ─────────────────────────────────────────────
  setSearchMode(mode: SearchMode): void {
    this.searchMode.set(mode);
    this.results.set([]);
    this.showOverlay.set(false);
    this.nameQuery.set('');
    this.docType.set('');
    this.docId.set('');
  }

  // ── Resultado seleccionado ─────────────────────────────────────────────────
  onResultClick(result: PolicySearchResult): void {
    this.showOverlay.set(false);
    this.router.navigate(['/policies', result.id]);
  }

  closeOverlay(): void {
    this.showOverlay.set(false);
  }

  // ── Logout ─────────────────────────────────────────────────────────────────
  logout(): void {
    this.auth.logout();
  }

  // ── Helpers ────────────────────────────────────────────────────────────────
  private toSearchResult(p: PolicyResponse): PolicySearchResult {
    return {
      id:           p.id,
      insuredName:  `${p.insuredFirstName} ${p.insuredLastName}`,
      documentType: p.insuredDocumentType,
      documentId:   p.insuredDocumentId,
      policyType:   p.type,
      status:       p.status,
    };
  }

  private loadPendingCount(): void {
    this.claimsSvc.getAll({ status: 'PendingApproval', page: 1, pageSize: 1 })
      .subscribe({ next: res => this.pendingCount.set(res.totalCount) });
  }
}