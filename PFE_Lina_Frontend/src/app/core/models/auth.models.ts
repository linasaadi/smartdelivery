export interface LoginDto {
  email: string;
  motDePasse: string;
}

export interface RegisterDto {
  email: string;
  motDePasse: string;
  nom: string;
  prenom: string;
  role: 'Dispatcher' | 'Chauffeur';
}

export interface TokenResponse {
  token: string;
  email: string;
  nom: string;
  prenom: string;
  role: 'Admin' | 'Dispatcher' | 'Chauffeur';
  expiration: string;
}

export type UserRole = 'Admin' | 'Dispatcher' | 'Chauffeur';
