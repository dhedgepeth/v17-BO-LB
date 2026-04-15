import { html, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';

const DAYS = [
  { day: 1, name: 'Monday' },
  { day: 2, name: 'Tuesday' },
  { day: 3, name: 'Wednesday' },
  { day: 4, name: 'Thursday' },
  { day: 5, name: 'Friday' },
  { day: 6, name: 'Saturday' },
  { day: 0, name: 'Sunday' },
];

function defaultValue() {
  return DAYS.map(d => ({
    day: d.day,
    name: d.name,
    isOpen: d.day >= 1 && d.day <= 5,
    open: '09:00',
    close: '17:00',
  }));
}

export class WeeklyHoursEditor extends UmbLitElement {

  static properties = {
    value: { type: Array },
  };

  connectedCallback() {
    super.connectedCallback();
    if (!this.value?.length) {
      this.value = defaultValue();
      this.dispatchEvent(new UmbPropertyValueChangeEvent());
    }
  }

  #onToggle(index, e) {
    this.value = this.value.map((entry, i) =>
      i === index ? { ...entry, isOpen: e.target.checked } : entry
    );
    this.dispatchEvent(new UmbPropertyValueChangeEvent());
  }

  #onTimeChange(index, field, e) {
    this.value = this.value.map((entry, i) =>
      i === index ? { ...entry, [field]: e.target.value } : entry
    );
    this.dispatchEvent(new UmbPropertyValueChangeEvent());
  }

  render() {
    if (!this.value?.length) return html``;

    return html`
      <div class="weekly-hours">
        ${this.value.map((entry, i) => html`
          <div class="day-row">
            <span class="day-name">${entry.name}</span>
            <uui-toggle
              .checked=${entry.isOpen}
              @change=${(e) => this.#onToggle(i, e)}>
              ${entry.isOpen ? 'Open' : 'Closed'}
            </uui-toggle>
            ${entry.isOpen ? html`
              <div class="time-inputs">
                <label>Opens
                  <input type="time"
                    .value=${entry.open}
                    @change=${(e) => this.#onTimeChange(i, 'open', e)} />
                </label>
                <span class="separator">&mdash;</span>
                <label>Closes
                  <input type="time"
                    .value=${entry.close}
                    @change=${(e) => this.#onTimeChange(i, 'close', e)} />
                </label>
              </div>
            ` : html`
              <span class="closed-label">Closed all day</span>
            `}
          </div>
        `)}
      </div>
    `;
  }

  static styles = css`
    .weekly-hours {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }
    .day-row {
      display: flex;
      align-items: center;
      gap: 1rem;
      padding: 0.5rem 0;
      border-bottom: 1px solid var(--uui-color-border);
    }
    .day-name {
      width: 100px;
      font-weight: 600;
    }
    .time-inputs {
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }
    .time-inputs label {
      display: flex;
      align-items: center;
      gap: 0.25rem;
      font-size: 0.9em;
    }
    .separator {
      color: var(--uui-color-text-alt);
    }
    .closed-label {
      color: var(--uui-color-text-alt);
      font-style: italic;
    }
    input[type="time"] {
      padding: 4px 8px;
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      font-family: inherit;
    }
  `;
}

customElements.define('weekly-hours-editor', WeeklyHoursEditor);
