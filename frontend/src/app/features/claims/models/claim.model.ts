export type ClaimStatus =
  | 'Registered'
  | 'Approved'
  | 'Rejected'
  | 'Appealed'
  | 'Paid';

export interface ClaimStatusHistoryDto {
  status: ClaimStatus;
  changedAt: string;
  responsibleUser: string;
  observations?: string;
}

export interface ClaimDto {
  id: string;
  claimNumber: string;
  policyId: string;
  policyNumber: string;
  description: string;
  claimAmount: number;
  status: ClaimStatus;
  statusHistory: ClaimStatusHistoryDto[];
  createdAt: string;
}

export interface ClaimFilters {
  status?: ClaimStatus;
  policyId?: string;
  page: number;
  pageSize: number;
}

export interface CreateClaimRequest {
  policyId: string;
  description: string;
  claimAmount: number;
}

export interface ClaimActionRequest {
  responsibleUser: string;
  observations?: string;
}
