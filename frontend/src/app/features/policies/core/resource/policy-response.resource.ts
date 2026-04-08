export interface PolicyResponse {
  id: string;
  policyNumber: string;
  status: string;
  type: string;
  insured: {
    firstName: string;
    lastName: string;
    documentType: string;
    documentId: string;
    birthDate: string;
    email: string;
    phone: string;
  };
  coveragePeriod: {
    startDate: string;
    endDate: string;
  };
  insuredAmount: number;
  monthlyPremium?: number;
  createdAt: string;
  travelPlan?: {
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
  };
  healthPlan?: {
    planId: string;
    planName: string;
    baseAmount: number;
    ageFactorPercentage: number;
    ageFactorAmount: number;
    finalAmount: number;
  };
}

export interface PagedPolicyResponse {
  items: PolicyResponse[];
  totalCount: number;
  page: number;
  pageSize: number;
}
