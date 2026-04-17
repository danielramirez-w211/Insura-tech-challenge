import { InsuredPerson } from './insured-person.model';
import { CoveragePeriod } from './coverage-period.model';
import { TravelPlanSelection } from './travel-plan-selection.model';
import { HealthPlanSelection } from './health-plan-selection.model';

export type PolicyStatus = 'Pending' | 'Active' | 'Suspended' | 'Expired' | 'Cancelled';
export type PolicyType = 'Life' | 'Health' | 'Vehicle' | 'Home' | 'Travel';

export interface Policy {
  id: string;
  policyNumber: string;
  status: PolicyStatus;
  type: PolicyType;
  insured: InsuredPerson;
  coveragePeriod: CoveragePeriod;
  insuredAmount: number;
  monthlyPremium?: number;
  createdAt: string;
  travelPlan?: TravelPlanSelection;
  healthPlan?: HealthPlanSelection;
}

export interface PolicyFilters {
  status?: PolicyStatus;
  type?: PolicyType;
  documentId?: string;
  insuredSearch?: string;
  insuredDocumentType?: string;
  page: number;
  pageSize: number;
}
