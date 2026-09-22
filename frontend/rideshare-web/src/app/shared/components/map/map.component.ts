import { AfterViewInit, Component, ElementRef, OnDestroy, effect, input, output, viewChild } from '@angular/core';
import * as L from 'leaflet';

export interface MapMarker {
  id: string;
  lat: number;
  lng: number;
  label: string;
  color: 'blue' | 'green' | 'red';
}

// Leaflet's default marker icons reference local image paths that break once bundled —
// point them at the package's own CDN-hosted assets instead of fighting the bundler.
const DEFAULT_ICON = L.icon({
  iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
  iconRetinaUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png',
  shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
  iconSize: [25, 41],
  iconAnchor: [12, 41]
});

const COLOR_ICON: Record<MapMarker['color'], L.Icon> = {
  blue: DEFAULT_ICON,
  green: L.icon({
    iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-green.png',
    shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
    iconSize: [25, 41],
    iconAnchor: [12, 41]
  }),
  red: L.icon({
    iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-red.png',
    shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
    iconSize: [25, 41],
    iconAnchor: [12, 41]
  })
};

/** Free, no-API-key map (OpenStreetMap tiles via Leaflet) — click to pick a point, or just display markers. */
@Component({
  selector: 'app-map',
  standalone: true,
  template: `<div #mapHost class="map-host"></div>`,
  styles: [
    `
      .map-host {
        width: 100%;
        height: 100%;
        min-height: 280px;
        border-radius: 12px;
        overflow: hidden;
      }
    `
  ]
})
export class MapComponent implements AfterViewInit, OnDestroy {
  readonly center = input<{ lat: number; lng: number }>({ lat: 40.7128, lng: -74.006 });
  readonly zoom = input(13);
  readonly markers = input<MapMarker[]>([]);
  readonly clickable = input(false);

  readonly mapClick = output<{ lat: number; lng: number }>();

  private readonly mapHost = viewChild.required<ElementRef<HTMLDivElement>>('mapHost');
  private map: L.Map | null = null;
  private leafletMarkers = new Map<string, L.Marker>();

  constructor() {
    effect(() => this.syncMarkers(this.markers()));
  }

  ngAfterViewInit(): void {
    this.map = L.map(this.mapHost().nativeElement).setView([this.center().lat, this.center().lng], this.zoom());

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; OpenStreetMap contributors',
      maxZoom: 19
    }).addTo(this.map);

    if (this.clickable()) {
      this.map.on('click', (e: L.LeafletMouseEvent) => this.mapClick.emit({ lat: e.latlng.lat, lng: e.latlng.lng }));
    }

    this.syncMarkers(this.markers());
  }

  panTo(lat: number, lng: number): void {
    this.map?.panTo([lat, lng]);
  }

  ngOnDestroy(): void {
    this.map?.remove();
  }

  private syncMarkers(markers: MapMarker[]): void {
    if (!this.map) return;

    const currentIds = new Set(markers.map((m) => m.id));
    for (const [id, marker] of this.leafletMarkers) {
      if (!currentIds.has(id)) {
        marker.remove();
        this.leafletMarkers.delete(id);
      }
    }

    for (const marker of markers) {
      const existing = this.leafletMarkers.get(marker.id);
      if (existing) {
        existing.setLatLng([marker.lat, marker.lng]);
        existing.setPopupContent(marker.label);
      } else {
        const leafletMarker = L.marker([marker.lat, marker.lng], { icon: COLOR_ICON[marker.color] })
          .addTo(this.map)
          .bindPopup(marker.label);
        this.leafletMarkers.set(marker.id, leafletMarker);
      }
    }
  }
}
