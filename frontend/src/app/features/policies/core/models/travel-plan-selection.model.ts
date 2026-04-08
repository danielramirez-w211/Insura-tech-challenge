export type TripType = 'Nacional' | 'Internacional';
export type Continent = 'America' | 'Europe' | 'Africa' | 'Asia' | 'Oceania';

export interface TravelPlanSelection {
  tripType: TripType;
  continent?: Continent;
  durationDays: number;
  basePriceUsd?: number;
  basePriceCop: number;
  dailyIncrementCop: number;
  totalPriceCop: number;
  trmUsed?: number;
  trmDate?: string;
  calculatedAt: string;
}
