import { Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

interface PlaceholderPage {
  section: string;
  title: string;
  description: string;
  features: string[];
}

@Component({
  selector: 'app-page-placeholder',
  standalone: true,
  templateUrl: './page-placeholder.component.html',
  styleUrl: './page-placeholder.component.scss'
})
export class PagePlaceholderComponent {
  private readonly route = inject(ActivatedRoute);

  readonly page = this.route.snapshot.data['page'] as PlaceholderPage;
}
