// ─── Enums ───────────────────────────────────────────────────────────────────
export type StatutLivraison = 'EnAttente' | 'EnCours' | 'Livree' | 'Annulee' | 'EnRetard';
export type StatutCamion = 'Disponible' | 'EnRoute' | 'EnMaintenance' | 'HorsService';

// ─── Pagination ──────────────────────────────────────────────────────────────
export interface PageDonnees<T> {
  elements: T[];
  totalElements: number;
  page: number;
  taillePage: number;
  totalPages: number;
  aPrecedent: boolean;
  aSuivant: boolean;
}

// ─── Destination ─────────────────────────────────────────────────────────────
export interface Destination {
  id: number;
  adresse: string;
  ville: string;
  codePostal: string;
  pays?: string;
  latitude: number;
  longitude: number;
}

export interface DestinationFormDto {
  adresse: string;
  ville: string;
  codePostal: string;
  pays?: string;
  latitude: number;
  longitude: number;
}

// ─── Chauffeur ────────────────────────────────────────────────────────────────
export interface ChauffeurResumeDto {
  id: number;
  nomComplet: string;
  telephone: string;
  estDisponible: boolean;
}

export interface ChauffeurDetailDto {
  id: number;
  nom: string;
  prenom: string;
  nomComplet: string;
  numeroPermis: string;
  telephone: string;
  email?: string;
  dateEmbauche: string;
  estDisponible: boolean;
  camions: CamionResumeDto[];
}

export interface ChauffeurFormDto {
  nom: string;
  prenom: string;
  numeroPermis: string;
  telephone: string;
  email?: string;
  dateEmbauche: string;
}

// ─── Camion ───────────────────────────────────────────────────────────────────
export interface CamionResumeDto {
  id: number;
  immatriculation: string;
  marque: string;
  modele: string;
  statut: StatutCamion;
  nomChauffeur?: string;
}

export interface CamionDetailDto {
  id: number;
  immatriculation: string;
  marque: string;
  modele: string;
  capaciteKg: number;
  capaciteM3: number;
  statut: StatutCamion;
  latitudeActuelle?: number;
  longitudeActuelle?: number;
  chauffeur?: ChauffeurResumeDto;
}

export interface CamionFormDto {
  immatriculation: string;
  marque: string;
  modele: string;
  capaciteKg: number;
  capaciteM3: number;
  chauffeurId?: number;
}

// ─── Produit ──────────────────────────────────────────────────────────────────
export interface Produit {
  id: number;
  nom: string;
  description?: string;
  poidsKg: number;
  volumeM3: number;
  prixUnitaire: number;
}

export interface ProduitFormDto {
  nom: string;
  description?: string;
  poidsKg: number;
  volumeM3: number;
  prixUnitaire: number;
}

// ─── Tracking & Anomalie ──────────────────────────────────────────────────────
export interface PointTrackingDto {
  id: number;
  latitude: number;
  longitude: number;
  vitesseKmh: number;
  horodatage: string;
  adresse?: string;
  livraisonId: number;
}

export interface AnomalieDto {
  type: string;
  description: string;
  dateDetection: string;
  estResolue: boolean;
  dateResolution?: string;
  retardMinutes?: number;
  livraisonId: number;
}

// ─── Livraison ────────────────────────────────────────────────────────────────
export interface LivraisonProduitDto {
  produitId: number;
  nomProduit?: string;
  quantite: number;
  prixTotal: number;
}

export interface LivraisonResumeDto {
  id: number;
  reference: string;
  statut: StatutLivraison;
  nomChauffeur?: string;
  immatriculationCamion?: string;
  destination: string;
  dateLivraisonPrevue?: string;
  dateLivraisonReelle?: string;
  eta?: string;
  cout: number;
  nombreAnomaliesOuvertes: number;
}

export interface LivraisonDetailDto {
  id: number;
  reference: string;
  statut: StatutLivraison;
  camion?: CamionResumeDto;
  destination: Destination;
  produits: LivraisonProduitDto[];
  anomalies: AnomalieDto[];
  dateCreation: string;
  dateLivraisonPrevue?: string;
  dateLivraisonReelle?: string;
  eta?: string;
  cout: number;
  notes?: string;
}

export interface CreerLivraisonDto {
  camionId: number;
  destinationId: number;
  dateLivraisonPrevue: string;
  produits: { produitId: number; quantite: number; nomProduit?: null; prixTotal?: 0 }[];
  notes?: string;
}

export interface ModifierStatutDto {
  nouveauStatut: StatutLivraison;
}

// ─── Dashboard KPIs ───────────────────────────────────────────────────────────
export interface PerformanceChauffeurDto {
  chauffeurId: number;
  nomComplet: string;
  totalLivraisons: number;
  livraisonsReussies: number;
  livraisonsEnRetard: number;
  tauxReussite: number;
}

export interface KpiAdminDto {
  totalLivraisons: number;
  enAttente: number;
  enCours: number;
  livrees: number;
  annulees: number;
  enRetard: number;
  totalChauffeurs: number;
  chauffeursDisponibles: number;
  totalCamions: number;
  camionsDisponibles: number;
  tauxReussite: number;
  topChauffeurs: PerformanceChauffeurDto[];
  livraisonsEnRetard: LivraisonResumeDto[];
}

export interface KpiDispatcherDto {
  livraisonsEnAttente: number;
  livraisonsEnCours: number;
  camionsDisponibles: number;
  chauffeursDisponibles: number;
  urgentesEnRetard: LivraisonResumeDto[];
}

// ─── Route / Navigation ───────────────────────────────────────────────────────
export interface RouteRequest {
  depart: { latitude: number; longitude: number };
  destination: { latitude: number; longitude: number };
}

export interface MultiStopRequest {
  depart: { latitude: number; longitude: number };
  arrets: Array<{ latitude: number; longitude: number; nom?: string }>;
}

export interface RouteResult {
  distanceKm: number;
  dureeEstimeeMinutes: number;
  eta: string;
  facteurTrafic?: number;
  ordreOptimal?: Array<{ latitude: number; longitude: number; nom?: string; ordre: number }>;
}

export interface FacteurTrafic {
  heure: number;
  facteur: number;
  zone?: string;
}

// ─── Compatibilité — types legacy utilisés dans tracking/routes ───────────────
export interface Camion extends CamionDetailDto {}
export interface Chauffeur extends ChauffeurDetailDto {}
export interface PointTracking extends PointTrackingDto {}
export interface Anomalie extends AnomalieDto { id: number; }
export interface Livraison extends LivraisonDetailDto {}
export interface CreateLivraisonDto extends CreerLivraisonDto {}
export interface UpdateStatutDto { statut: StatutLivraison; }
