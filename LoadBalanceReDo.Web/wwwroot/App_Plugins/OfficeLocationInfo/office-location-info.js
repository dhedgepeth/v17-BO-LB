import { html, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/document';

export class OfficeLocationInfoEditor extends UmbLitElement {

  static properties = {
    value: { type: Object },
    _loading: { type: Boolean, state: true },
    _error: { type: String, state: true },
  };

  _loading = false;
  _error = null;
  _geoAlias = 'officeLocation';
  _lastLookup = null;
  _workspaceContext = null;

  connectedCallback() {
    super.connectedCallback();

    // Read the configured geo property alias from settings
    if (this.config) {
      const alias = this.config.find(c => c.alias === 'geoPropertyAlias');
      if (alias?.value) this._geoAlias = alias.value;
    }

    // Get the workspace context for reading property values
    this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (context) => {
      this._workspaceContext = context;

      // Try auto-detect on load if we don't already have a value
      if (!this.value?.timezone) {
        this.#detectFromContext();
      }
    });
  }

  #getCoordinatesFromContext() {
    if (!this._workspaceContext) return null;

    let raw = null;
    try {
      raw = this._workspaceContext.getPropertyValue(this._geoAlias);
    } catch (e) {
      console.warn('[OfficeLocationInfo] getPropertyValue failed:', e);
    }

    if (!raw) return null;

    try {
      const coords = typeof raw === 'string' ? JSON.parse(raw) : raw;
      if (coords?.latitude && coords?.longitude) return coords;
    } catch { }

    return null;
  }

  #detectFromContext() {
    const coords = this.#getCoordinatesFromContext();
    if (!coords) return;

    const key = `${coords.latitude.toFixed(4)},${coords.longitude.toFixed(4)}`;
    if (key === this._lastLookup && this.value?.timezone) return;
    this._lastLookup = key;

    this.#resolve(coords.latitude, coords.longitude);
  }

  async #resolve(lat, lng) {
    this._loading = true;
    this._error = null;

    try {
      const response = await fetch(
        `/api/geolookup/resolve?lat=${lat}&lng=${lng}`,
        { credentials: 'same-origin' }
      );

      if (!response.ok) {
        const body = await response.text();
        throw new Error(`HTTP ${response.status}: ${body}`);
      }

      const data = await response.json();

      this.value = {
        city: data.city || null,
        country: data.country || null,
        timezone: data.timezone || null,
        utcOffset: data.utcOffset || null,
      };

      this.dispatchEvent(new UmbPropertyValueChangeEvent());
    } catch (e) {
      this._error = `Lookup failed: ${e.message}`;
    } finally {
      this._loading = false;
    }
  }

  #onDetect() {
    this._lastLookup = null;
    this._error = null;
    this.#detectFromContext();
  }

  render() {
    if (this._loading) {
      return html`<div class="info-card">
        <uui-loader-bar></uui-loader-bar>
        <span>Resolving location...</span>
      </div>`;
    }

    if (this._error) {
      return html`<div class="info-card error">
        <span>${this._error}</span>
        <uui-button look="secondary" label="Retry" @click=${this.#onDetect}>Retry</uui-button>
      </div>`;
    }

    if (!this.value?.timezone) {
      return html`<div class="info-card empty">
        <span>Set a location on the map, then click Detect.</span>
        <uui-button look="primary" label="Detect" @click=${this.#onDetect}>Detect from location</uui-button>
      </div>`;
    }

    return html`
      <div class="info-card">
        <div class="info-row">
          <span class="info-label">City</span>
          <span class="info-value">${this.value.city || '—'}</span>
        </div>
        <div class="info-row">
          <span class="info-label">Country</span>
          <span class="info-value">${this.value.country || '—'}</span>
        </div>
        <div class="info-row">
          <span class="info-label">Timezone</span>
          <span class="info-value">${this.value.timezone}</span>
        </div>
        <div class="info-row">
          <span class="info-label">UTC Offset</span>
          <span class="info-value">${this.value.utcOffset || '—'}</span>
        </div>
        <div class="info-actions">
          <uui-button look="secondary" label="Refresh" @click=${this.#onDetect}>Refresh</uui-button>
        </div>
      </div>
    `;
  }

  static styles = css`
    .info-card {
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      padding: 1rem;
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
      background: var(--uui-color-surface-alt);
    }
    .info-card.empty {
      color: var(--uui-color-text-alt);
      font-style: italic;
    }
    .info-card.error {
      color: var(--uui-color-danger);
    }
    .info-row {
      display: flex;
      gap: 1rem;
      align-items: baseline;
    }
    .info-label {
      width: 90px;
      font-weight: 600;
      font-size: 0.85em;
      color: var(--uui-color-text-alt);
      text-transform: uppercase;
    }
    .info-value {
      font-size: 0.95em;
    }
    .info-actions {
      margin-top: 0.25rem;
    }
  `;
}

customElements.define('office-location-info', OfficeLocationInfoEditor);
