export interface CurrentUser {
  id: string;
  email: string | null;
  roles: string[];
}

export interface LoginRequest {
  email: string;
  password: string;
}
