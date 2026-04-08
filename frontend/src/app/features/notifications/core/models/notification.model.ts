export type NotificationStatus = 'Pending' | 'Sent' | 'Failed';
export type NotificationEvent =
  | 'PolicyActivated'
  | 'PolicyExpiringSoon'
  | 'ClaimRegistered'
  | 'ClaimStatusChanged';

export interface Notification {
  id: string;
  event: NotificationEvent;
  recipientId: string;
  recipientName: string;
  subject: string;
  body: string;
  status: NotificationStatus;
  retryCount: number;
  lastAttemptAt?: string;
  createdAt: string;
}

export interface NotificationFilters {
  status?: NotificationStatus;
  page: number;
  pageSize: number;
}
