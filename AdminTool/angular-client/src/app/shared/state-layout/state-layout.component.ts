import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-state-layout',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './state-layout.component.html',
  styleUrls: ['./state-layout.component.scss']
})
export class StateLayoutComponent {
  @Input() loading = false;
  @Input() error: string | null = null;
  @Input() empty = false;
  @Input() hasCustomLoading = false;
  @Input() hasCustomError = false;
  @Input() hasCustomEmpty = false;
}
