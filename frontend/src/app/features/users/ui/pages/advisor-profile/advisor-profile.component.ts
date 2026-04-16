import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDividerModule } from '@angular/material/divider';
import { UsersService } from '../../../core/services/users.service';
import { PageHeaderComponent } from '../../../../../shared/components/page-header/page-header.component';
import { LoadingSpinnerComponent } from '../../../../../shared/components/loading-spinner/loading-spinner.component';
import { UserDetail } from '../../../core/models/user.model';

@Component({
  selector: 'app-advisor-profile',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatSnackBarModule,
    MatDividerModule,
    PageHeaderComponent,
    LoadingSpinnerComponent,
  ],
  templateUrl: './advisor-profile.component.html',
  styleUrl: './advisor-profile.component.css',
})
export class AdvisorProfileComponent implements OnInit {
  private svc     = inject(UsersService);
  private fb      = inject(FormBuilder);
  private snack   = inject(MatSnackBar);

  loading  = signal(true);
  saving   = signal(false);
  user     = signal<UserDetail | null>(null);
  editMode = signal(false);

  form = this.fb.group({
    firstName:      ['', Validators.required],
    lastName:       ['', Validators.required],
    nationality:    [''],
    birthDate:      [''],
    yearsInCompany: [null as number | null],
    officeLocation: [''],
    workSchedule:   [''],
  });

  ngOnInit() {
    this.svc.getMe().subscribe({
      next: (u) => {
        this.user.set(u);
        this.form.patchValue({
          firstName:      u.profile?.firstName ?? '',
          lastName:       u.profile?.lastName  ?? '',
          nationality:    u.profile?.nationality    ?? '',
          birthDate:      u.profile?.birthDate      ?? '',
          yearsInCompany: u.profile?.yearsInCompany ?? null,
          officeLocation: u.profile?.officeLocation ?? '',
          workSchedule:   u.profile?.workSchedule   ?? '',
        });
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  onSave() {
    if (this.form.invalid || this.saving()) return;
    this.saving.set(true);
    const val = this.form.getRawValue();
    this.svc.updateProfile({
      firstName:      val.firstName!,
      lastName:       val.lastName!,
      nationality:    val.nationality   || undefined,
      birthDate:      val.birthDate     || undefined,
      yearsInCompany: val.yearsInCompany ?? undefined,
      officeLocation: val.officeLocation || undefined,
      workSchedule:   val.workSchedule   || undefined,
    }).subscribe({
      next: (updated) => {
        this.user.set(updated);
        this.saving.set(false);
        this.editMode.set(false);
        this.snack.open('Perfil actualizado correctamente', 'Cerrar', { duration: 3000 });
      },
      error: () => {
        this.saving.set(false);
        this.snack.open('Error al actualizar el perfil', 'Cerrar', { duration: 3000 });
      },
    });
  }
}
