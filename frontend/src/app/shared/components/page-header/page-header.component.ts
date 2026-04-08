import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-page-header',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule],
  templateUrl: './page-header.component.html',
  styleUrl: './page-header.component.css',

})
export class PageHeaderComponent {
  @Input({ required: true }) title!: string;
  @Input() actionLabel?: string;
  @Input() actionIcon = 'add';
  @Output() action = new EventEmitter<void>();
}
