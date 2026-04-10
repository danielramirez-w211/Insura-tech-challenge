export interface VehiclePlan {
  planId: string;
  planName: string;
  priceMultiplier: number;
  coverages: string[];
  assistances: string[];
}

export interface VehiclePlanOption {
  planId: string;
  planName: string;
  monthlyPremium: number;
  annualPremiumWithDiscount: number;
  coverages: string[];
  assistances: string[];
}

export interface VehicleQuotationResult {
  commercialValue: number;
  vehicleYear: number;
  brand: string;
  vehicleAge: number;
  ageCategory: 'Nuevo' | 'Usado Reciente' | 'Usado Antiguo';
  technicalRate: number;
  hasBrandSurcharge: boolean;
  baseMonthlyPremium: number;
  plans: VehiclePlanOption[];
}
