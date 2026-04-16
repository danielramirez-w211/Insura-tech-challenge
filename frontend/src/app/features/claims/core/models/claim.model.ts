export type ClaimStatus =
  | 'PendingApproval'
  | 'Registered'
  | 'UnderInvestigation'
  | 'Approved'
  | 'Rejected'
  | 'Appealed'
  | 'Paid';

export interface ClaimStatusHistory {
  status: ClaimStatus;
  changedAt: string;
  responsibleUser: string;
  observations?: string;
}

export interface Claim {
  id: string;
  claimNumber: string;
  policyId: string;
  policyNumber: string;
  description: string;
  claimAmount: number;
  status: ClaimStatus;
  statusHistory: ClaimStatusHistory[];
  createdAt: string;
  createdByAdvisorId?: string;
  rejectionReason?: string;
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
