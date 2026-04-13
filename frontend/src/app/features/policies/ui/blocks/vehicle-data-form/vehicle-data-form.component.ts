import { Component, inject, input, output } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIcon, MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';

export interface VehicleDataFormValue {
  commercialValue: number;
  vehicleYear: number;
  brand: string;
}

function vehicleYearValidator(control: AbstractControl): ValidationErrors | null {
  const year = control.value as number | null;
  if (year === null || year === undefined) return null;
  const currentYear = new Date().getFullYear();
  return year < 1900 || year > currentYear ? { vehicleYear: true } : null;
}

@Component({
  selector: 'app-vehicle-data-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatIcon,
    MatSelectModule,
  ],
  templateUrl: './vehicle-data-form.component.html',
  styleUrl: './vehicle-data-form.component.css',
})
export class VehicleDataFormComponent {
  loading = input(false);
  quoteRequested = output<VehicleDataFormValue>();

  private fb = inject(FormBuilder);

  readonly VEHICLE_BRANDS = [
    'BMW', 'BYD', 'Chevrolet', 'Ford', 'Honda',
    'Hyundai', 'Jeep', 'Nissan', 'Renault', 'Subaru', 'Toyota',
  ] as const;

  readonly currentYear = new Date().getFullYear();

  form = this.fb.group({
    commercialValue: [null as number | null, [Validators.required, Validators.min(1)]],
    vehicleYear:     [null as number | null, [Validators.required, vehicleYearValidator]],
    brand:           ['', Validators.required],
  });

  onQuote(): void {
    if (this.form.invalid) return;
    const v = this.form.value;
    this.quoteRequested.emit({
      commercialValue: v.commercialValue!,
      vehicleYear:     v.vehicleYear!,
      brand:           v.brand!,
    });
  }
}
