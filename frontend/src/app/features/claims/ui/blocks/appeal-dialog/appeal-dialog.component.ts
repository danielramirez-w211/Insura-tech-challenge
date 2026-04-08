import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-appeal-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
  ],
  templateUrl: './appeal-dialog.component.html',
  styleUrl: './appeal-dialog.component.css',
})
export class AppealDialogComponent {
  dialogRef = inject(MatDialogRef<AppealDialogComponent>);
  fb = inject(FormBuilder);

  form = this.fb.group({
    responsibleUser: ['', Validators.required],
    observations: ['', Validators.required],
  });
}
