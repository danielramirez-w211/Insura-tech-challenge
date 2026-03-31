import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule],
  template: `
    <div class="empty-state">
      <mat-icon class="empty-icon">{{ icon }}</mat-icon>
      <p class="empty-message">{{ message }}</p>
      @if (showRetry) {
        <button mat-stroked-button color="primary" (click)="retry.emit()">
          <mat-icon>refresh</mat-icon>
          Reintentar
        </button>
      }
    </div>
  `,
  styles: [`
    .empty-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      padding: 48px;
      gap: 16px;
      color: #666;
    }
    .empty-icon {
      font-size: 64px;
      width: 64px;
      height: 64px;
      opacity: 0.4;
    }
    .empty-message {
      font-size: 16px;
      margin: 0;
    }
  `],
})
export class EmptyStateComponent {
  @Input() message = 'No se encontraron resultados.';
  @Input() icon = 'inbox';
  @Input() showRetry = false;
  @Output() retry = new EventEmitter<void>();
}
