import { Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { CreateAdvisorRequest, CreateLeaderRequest } from '../../../core/models/user.model';

export type UserFormMode = 'leader' | 'advisor';

@Component({
  selector: 'app-user-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
  ],
  templateUrl: './user-form.component.html',
  styleUrl: './user-form.component.css',
})
export class UserFormComponent implements OnInit {
  @Input() mode: UserFormMode = 'leader';
  @Input() saving = false;
  @Output() submitted = new EventEmitter<CreateLeaderRequest | CreateAdvisorRequest>();
  @Output() cancelled = new EventEmitter<void>();

  private fb = inject(FormBuilder);

  form = this.fb.group({
    email:          ['', [Validators.required, Validators.email]],
    firstName:      ['', Validators.required],
    lastName:       ['', Validators.required],
    nationality:    [''],
    birthDate:      [''],
    yearsInCompany: [null as number | null],
    officeLocation: [''],
    workSchedule:   [''],
  });

  get isAdvisor() { return this.mode === 'advisor'; }

  ngOnInit() {}

  onSubmit() {
    if (this.form.invalid) return;
    const v = this.form.getRawValue();
    if (this.isAdvisor) {
      this.submitted.emit({
        email:          v.email!,
        firstName:      v.firstName!,
        lastName:       v.lastName!,
        nationality:    v.nationality    || undefined,
        birthDate:      v.birthDate      || undefined,
        yearsInCompany: v.yearsInCompany ?? undefined,
        officeLocation: v.officeLocation || undefined,
        workSchedule:   v.workSchedule   || undefined,
      } as CreateAdvisorRequest);
    } else {
      this.submitted.emit({
        email:          v.email!,
        firstName:      v.firstName!,
        lastName:       v.lastName!,
        officeLocation: v.officeLocation || undefined,
        workSchedule:   v.workSchedule   || undefined,
      } as CreateLeaderRequest);
    }
  }
}
