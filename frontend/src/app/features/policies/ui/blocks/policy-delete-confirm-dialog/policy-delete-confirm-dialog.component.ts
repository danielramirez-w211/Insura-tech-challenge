import { Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-policy-delete-confirm-dialog',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule],
  templateUrl: './policy-delete-confirm-dialog.component.html',
  styleUrl: './policy-delete-confirm-dialog.component.css',
})
export class PolicyDeleteConfirmDialogComponent {
  readonly data = inject<{ policyNumber: string }>(MAT_DIALOG_DATA);
}
