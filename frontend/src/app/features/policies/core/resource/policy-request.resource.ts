import { PolicyType } from '../models/policy.model';
import { TripType, Continent } from '../models/travel-plan-selection.model';

export interface InsuredPersonRequest {
  firstName: string;
  lastName: string;
  documentType: string;
  documentId: string;
  birthDate: string;
  email: string;
  phone: string;
  gender: 'Masculino' | 'Femenino';
  address: string;
  cityName: string;
  postalCode: string;
  department: string;
}

export interface CoveragePeriodRequest {
  startDate: string;
  endDate: string;
}

export interface CreatePolicyRequest {
  type: PolicyType;
  insured: InsuredPersonRequest;
  coveragePeriod: CoveragePeriodRequest;
  insuredAmount: number;
  monthlyPremium: number;
  healthPlanId?: string;
  lifePlanId?: string;
  vehiclePlanId?: string;
  vehicleCommercialValue?: number;
  vehicleYear?: number;
  vehicleBrand?: string;
  tripType?: TripType;
  continent?: Continent;
  durationDays?: number;
}
