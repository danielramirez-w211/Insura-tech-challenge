import { Component, computed, inject, input, output, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCheckboxModule } from '@angular/material/checkbox';
import {
  HomeCoverage,
  HomeDataFormValue,
  HomePlanPackage,
  HomePropertyType,
  HOME_COVERAGE_OPTIONS,
  HOME_PROPERTY_TYPE_OPTIONS,
} from '../../../core/models/home-plan-selection.model';

function constructionYearValidator(control: AbstractControl): ValidationErrors | null {
  const year = control.value as number | null;
  if (year === null || year === undefined) return null;
  const currentYear = new Date().getFullYear();
  return year < 1900 || year > currentYear
    ? { constructionYear: true }
    : null;
}

@Component({
  selector: 'app-home-data-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatCheckboxModule,
  ],
  templateUrl: './home-data-form.component.html',
  styleUrl: './home-data-form.component.css',
})
export class HomeDataFormComponent {
  loading          = input(false);
  packages         = input<HomePlanPackage[]>([]);
  quoteRequested   = output<HomeDataFormValue>();

  private fb = inject(FormBuilder);

  readonly COVERAGE_OPTIONS        = HOME_COVERAGE_OPTIONS;
  readonly PROPERTY_TYPE_OPTIONS   = HOME_PROPERTY_TYPE_OPTIONS;
  readonly currentYear             = new Date().getFullYear();

  // FireExplosion siempre incluida, no se puede deseleccionar
  selectedCoverages = signal<HomeCoverage[]>(['FireExplosion']);
  selectedPackageId = signal<string | null>(null);

  form = this.fb.group({
    propertyValue:    [null as number | null, [Validators.required, Validators.min(1)]],
    constructionYear: [null as number | null, [Validators.required, constructionYearValidator]],
    stratum:          [null as number | null, [Validators.required, Validators.min(1), Validators.max(6)]],
    occupants:        [null as number | null, [Validators.required, Validators.min(1)]],
    propertyType:     ['' as HomePropertyType | '', Validators.required],
  });

  isCoverageSelected(coverage: HomeCoverage): boolean {
    return this.selectedCoverages().includes(coverage);
  }

  toggleCoverage(coverage: HomeCoverage): void {
    if (coverage === 'FireExplosion') return; // Siempre incluida
    const current = this.selectedCoverages();
    if (current.includes(coverage)) {
      this.selectedCoverages.set(current.filter(c => c !== coverage));
    } else {
      this.selectedCoverages.set([...current, coverage]);
    }
    this.selectedPackageId.set(null); // Selección manual rompe el paquete
  }

  onPackageSelect(pkg: HomePlanPackage): void {
    this.selectedPackageId.set(pkg.packageId);
    // Garantizar que FireExplosion siempre esté
    const coverages: HomeCoverage[] = pkg.coverages.includes('FireExplosion')
      ? [...pkg.coverages]
      : ['FireExplosion', ...pkg.coverages];
    this.selectedCoverages.set(coverages);
  }

  onQuote(): void {
    if (this.form.invalid || this.selectedCoverages().length === 0) return;
    const v = this.form.value;
    this.quoteRequested.emit({
      propertyValue:     v.propertyValue!,
      constructionYear:  v.constructionYear!,
      stratum:           v.stratum!,
      occupants:         v.occupants!,
      propertyType:      v.propertyType as HomePropertyType,
      selectedCoverages: this.selectedCoverages(),
      packageId:         this.selectedPackageId(),
    });
  }
}
