export type PolicyStatus = 'Pending' | 'Active' | 'Suspended' | 'Expired' | 'Cancelled';
export type PolicyType = 'Life' | 'Health' | 'Vehicle' | 'Home' | 'Travel';

export interface InsuredDto {
  firstName: string;
  lastName : string;
  documentType: string;
  documentId: string;
  birthDate: string;
  email: string;
  phone: string;
}

export interface CoveragePeriodDto {
  startDate: string;
  endDate: string;
}

export interface PolicyDto {
  id: string;
  policyNumber: string;
  status: PolicyStatus;
  type: PolicyType;
  insured: InsuredDto;
  coveragePeriod: CoveragePeriodDto;
  insuredAmount: number;
  createdAt: string;
}

export interface PolicyFilters {
  status?: PolicyStatus;
  type?: PolicyType;
  documentId?: string;
  startDate?: string;
  endDate?: string;
  page: number;
  pageSize: number;
}

export type TripType   = 'Nacional' | 'Internacional';
export type Continent  = 'America' | 'Europe' | 'Africa' | 'Asia' | 'Oceania';

export interface CreatePolicyRequest {
  type: PolicyType;
  insured: InsuredDto;
  coveragePeriod: CoveragePeriodDto;
  insuredAmount: number;
  monthlyPremium: number;
  /** Solo para type === 'Health'. */
  healthPlanId?: string;
  /** Solo para type === 'Travel'. */
  tripType?: TripType;
  /** Solo para type === 'Travel' e Internacional. */
  continent?: Continent;
  /** Solo para type === 'Travel'. Días de cobertura (1-180). */
  durationDays?: number;
}

export interface HealthPlanDto {
  planId: string;
  planName: string;
  baseAmount: number;
}

export interface HealthPlanCalculationDto {
  planId: string;
  planName: string;
  baseAmount: number;
  ageFactorPercentage: number;
  ageFactorAmount: number;
  finalAmount: number;
  insuredAge: number;
}

export interface HealthPlanSelectionDto {
  planId: string;
  planName: string;
  baseAmount: number;
  ageFactorPercentage: number;
  ageFactorAmount: number;
  finalAmount: number;
}

export interface TravelPlanCalculationDto {
  tripType: string;
  continent?: string;
  durationDays: number;
  basePriceUsd?: number;
  basePriceCop: number;
  dailyIncrementCop: number;
  totalPriceCop: number;
  trmUsed?: number;
  trmDate?: string;
  calculatedAt: string;
}

export interface TravelPlanSelectionDto {
  tripType: string;
  continent?: string;
  durationDays: number;
  basePriceUsd?: number;
  basePriceCop: number;
  dailyIncrementCop: number;
  totalPriceCop: number;
  trmUsed?: number;
  trmDate?: string;
  calculatedAt: string;
}
