import { Directive, ElementRef, HostListener } from '@angular/core';
import { NgControl } from '@angular/forms';

@Directive({
  selector: '[appThousandsSeparator]',
  standalone: true,
})
export class ThousandsSeparatorDirective {
  constructor(private el: ElementRef, private control: NgControl) {}

  @HostListener('input', ['$event'])
  onInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    // Remove non-digit characters
    const raw = input.value.replace(/\D/g, '');
    // Limit to 10 digits
    const limited = raw.slice(0, 10);
    // Format with periods as thousands separator
    const formatted = limited.replace(/\B(?=(\d{3})+(?!\d))/g, '.');
    // Update input display
    input.value = formatted;
    // Update form control with raw numeric value (no separators)
    this.control.control?.setValue(limited, { emitEvent: true });
  }

  @HostListener('blur')
  onBlur(): void {
    const raw = (this.el.nativeElement as HTMLInputElement).value.replace(/\D/g, '');
    const formatted = raw.replace(/\B(?=(\d{3})+(?!\d))/g, '.');
    (this.el.nativeElement as HTMLInputElement).value = formatted;
  }
}
