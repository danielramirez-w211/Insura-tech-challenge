export type PolicyStatus = 'Pending' | 'Active' | 'Suspended' | 'Expired' | 'Cancelled';
export type PolicyType = 'Life' | 'Health' | 'Vehicle' | 'Home' | 'Travel';

export interface InsuredDto {
  name: string;
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

export interface CreatePolicyRequest {
  type: PolicyType;
  insured: InsuredDto;
  coveragePeriod: CoveragePeriodDto;
  insuredAmount: number;
  monthlyPremium: number;
  /** Solo para type === 'Health'. El backend calcula insuredAmount automáticamente. */
  healthPlanId?: string;
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
