export interface LifePlan {
  planId: string;
  planName: string;
  annualPremium: number;
  monthlyPremium: number;
  deathBenefit: number;
  funeralExpenses: number;
  burialExpenses: number;
  beneficiaryCompensation: number;
}

export interface LifePlanCalculation extends LifePlan {
  insuredAge: number;
  durationDays: number;
}
