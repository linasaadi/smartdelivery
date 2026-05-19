export interface LoginDto {
  email: string;
  motDePasse: string;
}

export interface RegisterDto {
  email: string;
  motDePasse: string;
  nom: string;
  prenom: string;
  role: 'Admin' | 'Dispatcher' | 'Chauffeur';
  chauffeurId?: number;
  dispatcherId?: number;
}

export interface UtilisateurDto {
  id: string;
  email: string;
  nom: string;
  prenom: string;
  role: string;
  chauffeurId?: number;
  dispatcherId?: number;
}

export interface ChangerRoleDto {
  nouveauRole: string;
}

export interface TokenResponse {
  token: string;
  email: string;
  nom: string;
  prenom: string;
  role: 'Admin' | 'Dispatcher' | 'Chauffeur';
  expiration: string;
  chauffeurId?: number;
  dispatcherId?: number;
}

export type UserRole = 'Admin' | 'Dispatcher' | 'Chauffeur';
