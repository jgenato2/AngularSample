import { Component, EventEmitter, Input, Output, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-detail-actions-bar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './detail-actions-bar.component.html',
  styleUrls: ['./detail-actions-bar.component.scss'],
})
export class DetailActionsBarComponent {
  @Input() showBack = true;
  @Input() backLabel = 'Back';
  @Output() back = new EventEmitter<void>();

  @HostListener('document:keydown', ['$event'])
  onKeydown(event: KeyboardEvent) {
    if (event.key === 'Escape') {
      this.back.emit();
      event.stopPropagation();
      event.preventDefault();
    }
  }
}
