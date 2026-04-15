// Shape real del backend — campos planos (flat), no anidados.
// El mapping a Policy (con insured/coveragePeriod anidados)
// ocurre en mapResponseToPolicy dentro de PoliciesState.

export interface PolicyResponse {
  id: string;
  policyNumber: string;
  status: string;
  type: string;

  // Asegurado — campos planos
  insuredFirstName: string;
  insuredLastName: string;
  insuredDocumentType: string;
  insuredDocumentId: string;
  insuredAge: number;
  insuredGender?: string;
  insuredAddress?: string;
  insuredCityName?: string;
  insuredPostalCode?: string;
  insuredDepartment?: string;

  // Vigencia — campos planos
  coverageStartDate: string;
  coverageEndDate: string;

  monthlyPremium: number;
  insuredAmount: number;
  availableInsuredAmount: number;
  cancellationReason?: string;
  createdAt: string;
  updatedAt?: string;

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
