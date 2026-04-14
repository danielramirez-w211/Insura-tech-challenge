import { Component, EventEmitter, inject, Input, OnInit, Output, signal } from '@angular/core';
import {
  ReactiveFormsModule,
  FormBuilder,
  Validators,
  ValidatorFn,
  AbstractControl,
  ValidationErrors,
} from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { InsuredPerson, DocumentType, DOCUMENT_TYPE_OPTIONS } from '../../../core/models/insured-person.model';
import { CityOption } from '../../../core/models/city.model';
import { CitiesService } from '../../../core/service/cities.service';
import { ThousandsSeparatorDirective } from '../../../../../shared/directives/thousands-separator.directive';

export interface InsuredFormValue {
  value: InsuredPerson;
  valid: boolean;
}

function documentIdValidator(type: DocumentType): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value as string;
    if (!value) return { required: true };
    if (['CC', 'TI', 'RC'].includes(type)) {
      const raw = value.replace(/\./g, ''); // strip thousands separators
      return /^\d{1,10}$/.test(raw) ? null : { invalidFormat: true };
    }
    if (['CE', 'PP'].includes(type)) {
      return /^[A-Za-z0-9]{1,11}$/.test(value) ? null : { invalidFormat: true };
    }
    return null;
  };
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
    MatSelectModule,
    ThousandsSeparatorDirective,
  ],
  templateUrl: './insured-form.component.html',
  styleUrl: './insured-form.component.css',
})
export class InsuredFormComponent implements OnInit {
  @Input() initialValue?: Partial<InsuredPerson & { birthDate: Date }>;
  @Output() formChange = new EventEmitter<InsuredFormValue>();
  @Output() next = new EventEmitter<void>();

  private readonly fb = inject(FormBuilder);
  private readonly citiesService = inject(CitiesService);

  readonly DOCUMENT_TYPE_OPTIONS = DOCUMENT_TYPE_OPTIONS;

  cities = signal<CityOption[]>([]);

  private selectedPostalCode = '';
  private selectedDepartment = '';

  form = this.fb.group({
    firstName:    ['', Validators.required],
    lastName:     ['', Validators.required],
    documentType: ['', Validators.required],
    documentId:   ['', Validators.required],
    birthDate:    [null as Date | null, Validators.required],
    email:        ['', [Validators.required, Validators.email]],
    phone:        [''],
    gender:       ['', Validators.required],
    city:         ['', Validators.required],
    address:      ['', Validators.required],
  });

  ngOnInit(): void {
    if (this.initialValue) {
      this.form.patchValue(this.initialValue as any);
    }

    this.citiesService.loadCities().subscribe(cities => this.cities.set(cities));

    this.form.get('documentType')!.valueChanges.subscribe(type => {
      this.form.get('documentId')!.setValue('');
      this.form.get('documentId')!.setValidators([documentIdValidator(type as DocumentType)]);
      this.form.get('documentId')!.updateValueAndValidity();
    });

    this.form.valueChanges.subscribe(() => this.emitChange());
  }

  onNext(): void {
    if (this.form.valid) this.next.emit();
  }

  onCitySelected(cityName: string): void {
    const city = this.cities().find(c => c.name === cityName);
    if (city) {
      this.selectedPostalCode = city.postalCode;
      this.selectedDepartment = city.department;
    }
  }

  private emitChange(): void {
    const v = this.form.value;
    this.formChange.emit({
      value: {
        firstName:    v.firstName ?? '',
        lastName:     v.lastName ?? '',
        documentType: (v.documentType ?? '') as DocumentType,
        documentId:   v.documentId ?? '',
        birthDate:    v.birthDate ? this.toDateStr(v.birthDate) : '',
        email:        v.email ?? '',
        phone:        v.phone ?? '',
        gender:       (v.gender ?? '') as 'Masculino' | 'Femenino',
        address:      v.address ?? '',
        cityName:     v.city ?? '',
        postalCode:   this.selectedPostalCode,
        department:   this.selectedDepartment,
      },
      valid: this.form.valid,
    });
  }

  private toDateStr(d: Date): string {
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
  }
}
