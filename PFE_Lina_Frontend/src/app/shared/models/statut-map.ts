/** Backend returns enum values as integers — map them to their string names. */
const LIVRAISON_MAP: Record<number, string> = {
  0: 'EnAttente',
  1: 'EnCours',
  2: 'Livree',
  3: 'Annulee',
  4: 'EnRetard',
  5: 'Refusee',
};

const CAMION_MAP: Record<number, string> = {
  0: 'Disponible',
  1: 'EnRoute',
  2: 'EnMaintenance',
  3: 'HorsService',
};

export function normalizeStatut(raw: string | number): string {
  if (typeof raw === 'number') {
    return LIVRAISON_MAP[raw] ?? CAMION_MAP[raw] ?? String(raw);
  }
  return raw;
}
