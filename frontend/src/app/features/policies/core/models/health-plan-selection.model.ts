export interface HealthPlan {
  planId: string;
  planName: string;
  baseAmount: number;
}

export interface HealthPlanSelection {
  planId: string;
  planName: string;
  baseAmount: number;
  ageFactorPercentage: number;
  ageFactorAmount: number;
  finalAmount: number;
}

export interface HealthPlanCalculation extends HealthPlanSelection {
  insuredAge: number;
}
