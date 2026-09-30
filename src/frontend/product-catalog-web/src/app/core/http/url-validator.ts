import { AbstractControl, ValidationErrors } from '@angular/forms';

export function httpUrlValidator(control: AbstractControl): ValidationErrors | null {
  if (!control.value) return null;
  try {
    const url = new URL(String(control.value));
    return ['http:', 'https:'].includes(url.protocol) && !url.username && !url.password
      ? null
      : { httpUrl: true };
  } catch {
    return { httpUrl: true };
  }
}

export function nonBlankValidator(control: AbstractControl): ValidationErrors | null {
  return String(control.value ?? '').trim() ? null : { required: true };
}

export function finitePriceValidator(control: AbstractControl): ValidationErrors | null {
  return control.value === null ||
    control.value === '' ||
    (typeof control.value === 'number' && Number.isFinite(control.value))
    ? null
    : { finitePrice: true };
}
