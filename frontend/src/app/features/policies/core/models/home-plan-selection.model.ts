export type HomeCoverage =
  | 'FireExplosion'
  | 'Theft'
  | 'Plumbing'
  | 'AestheticDamage'
  | 'ElectricalDamage'
  | 'GlassBreakage'
  | 'CivilLiability'
  | 'Uninhabitability'
  | 'LegalDefense'
  | 'HomeAssistance'
  | 'WaterDamageExpert';

export type HomePropertyType = 'House' | 'Apartment' | 'CommercialPremises';

export interface HomeCoverageOption {
  value: HomeCoverage;
  label: string;
  description: string;
}

export const HOME_COVERAGE_OPTIONS: HomeCoverageOption[] = [
  { value: 'FireExplosion',     label: 'Incendio y Explosión',       description: 'Incluida en la prima base' },
  { value: 'Theft',             label: 'Robo y Hurto',               description: '+12% del valor base' },
  { value: 'Plumbing',          label: 'Fontanería/Daños por Agua',  description: '+5% del valor base' },
  { value: 'AestheticDamage',   label: 'Pintura/Daños Estéticos',    description: '+3% del valor base' },
  { value: 'ElectricalDamage',  label: 'Daños Eléctricos',           description: '+7% del valor base' },
  { value: 'GlassBreakage',     label: 'Rotura de Cristales',        description: '$50.000 COP fijo' },
  { value: 'CivilLiability',    label: 'Responsabilidad Civil',      description: 'Escalado por estrato ($100k–$250k)' },
  { value: 'Uninhabitability',  label: 'Inhabitabilidad',            description: '+4% del valor base' },
  { value: 'LegalDefense',      label: 'Defensa Jurídica',           description: '$30.000 COP fijo' },
  { value: 'HomeAssistance',    label: 'Asistencia en el Hogar',     description: '$45.000 COP fijo' },
  { value: 'WaterDamageExpert', label: 'Daños por Agua/Peritaje',    description: '$65.000 COP fijo' },
];

export const HOME_PROPERTY_TYPE_OPTIONS: { value: HomePropertyType; label: string }[] = [
  { value: 'House',              label: 'Casa' },
  { value: 'Apartment',          label: 'Apartamento' },
  { value: 'CommercialPremises', label: 'Local Comercial' },
];

export interface HomePlanPackage {
  packageId:   string;
  packageName: string;
  coverages:   HomeCoverage[];
}

export interface HomeQuotationRequest {
  propertyValue:     number;
  constructionYear:  number;
  stratum:           number;
  occupants:         number;
  propertyType:      HomePropertyType;
  selectedCoverages: HomeCoverage[];
}

export interface HomeQuotationResult {
  propertyValue:      number;
  constructionYear:   number;
  propertyAge:        number;
  stratum:            number;
  occupants:          number;
  propertyType:       HomePropertyType;
  baseMonthlyPremium: number;
  selectedCoverages:  HomeCoverage[];
  appliedMultipliers: string[];
  finalMonthlyPremium: number;
}

export interface HomeDataFormValue {
  propertyValue:     number;
  constructionYear:  number;
  stratum:           number;
  occupants:         number;
  propertyType:      HomePropertyType;
  selectedCoverages: HomeCoverage[];
  packageId:         string | null;
}
