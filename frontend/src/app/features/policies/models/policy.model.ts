export type PolicyStatus = 'Pending' | 'Active' | 'Suspended' | 'Expired' | 'Cancelled';
export type PolicyType = 'Life' | 'Health' | 'Vehicle' | 'Home' | 'Travel';

export interface InsuredDto {
  name: string;
  documentId: string;
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
}
