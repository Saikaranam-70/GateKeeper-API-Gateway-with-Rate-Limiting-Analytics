import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiKeyService, ApiKey } from '../../services/api-key.service';
import { GatewayService, Gateway } from '../../services/gateway.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-api-keys',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './api-keys.component.html',
  styleUrls: ['./api-keys.component.css']
})
export class ApiKeysComponent implements OnInit {
  apiKeyService = inject(ApiKeyService);
  gatewayService = inject(GatewayService);
  toastService = inject(ToastService);

  apiKeys: ApiKey[] = [];
  gateways: Gateway[] = [];
  loading = true;

  // Key creation state
  createModalOpen = false;
  selectedGatewayId = '';
  label = '';
  expiryDate: string | null = null;
  generating = false;

  // Reveal raw key state
  rawKeyModalOpen = false;
  generatedRawKey = '';
  generatedKeyLabel = '';

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.loading = true;
    this.apiKeyService.getAll().subscribe({
      next: (keys) => {
        this.apiKeys = keys;
        
        // Load gateways to select during key creation
        this.gatewayService.getAll().subscribe({
          next: (gws) => {
            this.gateways = gws;
            this.loading = false;
          },
          error: () => {
            this.loading = false;
            this.toastService.showError('Failed to fetch gateways list.');
          }
        });
      },
      error: () => {
        this.loading = false;
        this.toastService.showError('Failed to load API keys.');
      }
    });
  }

  openCreateModal(): void {
    if (this.gateways.length === 0) {
      this.toastService.showError('Please configure at least one Gateway before generating API keys.');
      return;
    }
    this.selectedGatewayId = this.gateways[0].id || '';
    this.label = '';
    this.expiryDate = null;
    this.createModalOpen = true;
  }

  generateKey(): void {
    if (!this.label || !this.selectedGatewayId) {
      this.toastService.showError('Label and Gateway selection are required.');
      return;
    }

    this.generating = true;
    const formattedExpiry = this.expiryDate ? new Date(this.expiryDate).toISOString() : null;

    this.apiKeyService.generate(this.selectedGatewayId, this.label, formattedExpiry).subscribe({
      next: (res) => {
        this.generating = false;
        this.createModalOpen = false;
        this.toastService.showSuccess('API Key generated successfully!');
        
        // Setup raw key reveal modal
        this.generatedRawKey = res.rawApiKey || '';
        this.generatedKeyLabel = res.label;
        this.rawKeyModalOpen = true;

        this.loadData();
      },
      error: (err) => {
        this.generating = false;
        this.toastService.showError(err.error?.message || 'Failed to generate API Key.');
      }
    });
  }

  copyToClipboard(val: string): void {
    navigator.clipboard.writeText(val).then(() => {
      this.toastService.showSuccess('Token copied to clipboard!');
    }, () => {
      this.toastService.showError('Failed to copy to clipboard. Please copy manually.');
    });
  }

  closeRawKeyModal(): void {
    this.rawKeyModalOpen = false;
    this.generatedRawKey = '';
    this.generatedKeyLabel = '';
  }

  revokeKey(key: ApiKey): void {
    if (!key.id) return;
    if (confirm(`Are you sure you want to revoke the key "${key.label}"? Clients using this credentials token will be locked out immediately.`)) {
      this.apiKeyService.revoke(key.id).subscribe({
        next: () => {
          this.toastService.showSuccess('API Key revoked successfully.');
          this.loadData();
        },
        error: () => {
          this.toastService.showError('Failed to revoke API Key.');
        }
      });
    }
  }

  getGatewayName(id: string): string {
    const gw = this.gateways.find(g => g.id === id);
    return gw ? gw.name : 'Unknown Gateway';
  }
}
