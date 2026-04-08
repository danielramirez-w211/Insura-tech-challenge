import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { Claim } from '../../../core/models/claim.model';

@Component({
  selector: 'app-claim-actions',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule, MatCardModule],
  templateUrl: './claim-actions.component.html',
  styleUrl: './claim-actions.component.css',
})
export class ClaimActionsComponent {
  @Input({ required: true }) claim!: Claim;
  @Output() approve = new EventEmitter<void>();
  @Output() reject = new EventEmitter<void>();
  @Output() appeal = new EventEmitter<void>();
  @Output() pay = new EventEmitter<void>();
}
