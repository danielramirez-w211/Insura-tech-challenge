import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { PolicyStatus, PolicyType } from '../../../core/models/policy.model';
import { StatusLabelPipe } from '../../../../../shared/pipes/status-label.pipe';

@Component({
  selector: 'app-policy-filter-bar',
  standalone: true,
  imports: [FormsModule, MatFormFieldModule, MatSelectModule, StatusLabelPipe],
  templateUrl: './policy-filter-bar.component.html',
  styleUrl: './policy-filter-bar.component.css',
})
export class PolicyFilterBarComponent {
  @Input() selectedType: PolicyType | null = null;
  @Input() selectedStatus: PolicyStatus | null = null;

  @Output() typeFilterChange = new EventEmitter<PolicyType | null>();
  @Output() statusFilterChange = new EventEmitter<PolicyStatus | null>();

  readonly typeOptions: PolicyType[] = ['Life', 'Health', 'Vehicle', 'Home', 'Travel'];
  readonly statusOptions: PolicyStatus[] = ['Pending', 'Active', 'Suspended', 'Expired', 'Cancelled'];

  onTypeChange(value: PolicyType | undefined): void {
    this.typeFilterChange.emit(value ?? null);
  }

  onStatusChange(value: PolicyStatus | undefined): void {
    this.statusFilterChange.emit(value ?? null);
  }
}
