import { HttpErrorResponse } from '@angular/common/http';

export interface ProblemDetails {
  title?: string;
  detail?: string;
  status?: number;
  errors?: Record<string, string[]>;
}

export function fieldErrors(error: unknown): Record<string, string[]> {
  if (!(error instanceof HttpErrorResponse) || typeof error.error !== 'object' || !error.error)
    return {};
  const errors: unknown = (error.error as ProblemDetails).errors;
  if (typeof errors !== 'object' || !errors) return {};
  return Object.fromEntries(
    Object.entries(errors)
      .filter(
        ([, value]) => Array.isArray(value) && value.every((item) => typeof item === 'string'),
      )
      .map(([key, value]) => [key.toLowerCase(), value as string[]]),
  );
}

export function apiErrorMessage(error: unknown): string {
  if (!(error instanceof HttpErrorResponse)) return 'Something went wrong. Please try again.';
  switch (error.status) {
    case 0:
      return 'Cannot reach the API. Check that the backend is running, then try again.';
    case 401:
      return 'Your session has expired or the credentials are invalid. Please sign in.';
    case 403:
      return 'You do not have permission to perform this action.';
    case 404:
      return 'The requested resource was not found.';
    case 409:
      return 'The data changed during this request. Reload or retry; existing products are not overwritten.';
    case 422:
      return 'Only HTTP/HTTPS listing pages on books.toscrape.com are supported.';
    case 502:
      return 'The source could not be loaded or parsed. Please try again later.';
    default:
      return error.status === 400
        ? 'Check the form fields and try again.'
        : 'The server could not complete the request. Please try again.';
  }
}
