import { Component, EventEmitter, inject, Input, OnInit, Output } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';
import { InsuredPerson } from '../../../core/models/insured-person.model';

export interface InsuredFormValue {
  value: InsuredPerson;
  valid: boolean;
}

@Component({
  selector: 'app-insured-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatButtonModule,
  ],
  templateUrl: './insured-form.component.html',
  styleUrl: './insured-form.component.css',
})
export class InsuredFormComponent implements OnInit {
  @Input() initialValue?: Partial<InsuredPerson & { birthDate: Date }>;
  @Output() formChange = new EventEmitter<InsuredFormValue>();
  @Output() next = new EventEmitter<void>();

  private readonly fb = inject(FormBuilder);

  form = this.fb.group({
    firstName:    ['', Validators.required],
    lastName:     ['', Validators.required],
    documentType: ['', Validators.required],
    documentId:   ['', Validators.required],
    birthDate:    [null as Date | null, Validators.required],
    email:        ['', [Validators.required, Validators.email]],
    phone:        [''],
  });

  ngOnInit(): void {
    if (this.initialValue) {
      this.form.patchValue(this.initialValue as any);
    }
    this.form.valueChanges.subscribe(() => this.emitChange());
  }

  onNext(): void {
    if (this.form.valid) this.next.emit();
  }

  private emitChange(): void {
    const v = this.form.value;
    this.formChange.emit({
      value: {
        firstName:    v.firstName ?? '',
        lastName:     v.lastName ?? '',
        documentType: v.documentType ?? '',
        documentId:   v.documentId ?? '',
        birthDate:    v.birthDate ? this.toDateStr(v.birthDate) : '',
        email:        v.email ?? '',
        phone:        v.phone ?? '',
      },
      valid: this.form.valid,
    });
  }

  private toDateStr(d: Date): string {
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
  }
}
