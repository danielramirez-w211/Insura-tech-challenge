import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { NotificationsService } from './notifications.service';
import { NotificationDto } from '../models/notification.model';
import { PagedResult } from '../../../core/models/api-response.model';

const BASE_URL = 'http://localhost:5000/api/v1/notifications';

const mockNotification = (id: string, status: NotificationDto['status']): NotificationDto => ({
  id,
  event: 'PolicyActivated',
  recipientId: 'user-1',
  recipientName: 'John Doe',
  subject: 'Policy Activated',
  body: 'Your policy has been activated.',
  status,
  retryCount: 0,
  createdAt: '2025-01-01T00:00:00Z',
});

const mockSentNotification = mockNotification('notif-1', 'Sent');
const mockFailedNotification1 = mockNotification('notif-2', 'Failed');
const mockFailedNotification2 = mockNotification('notif-3', 'Failed');
const mockPendingNotification = mockNotification('notif-4', 'Pending');

const mockPagedResult: PagedResult<NotificationDto> = {
  items: [mockSentNotification],
  totalCount: 1,
  page: 1,
  pageSize: 10,
};

describe('NotificationsService', () => {
  let service: NotificationsService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        NotificationsService,
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    service = TestBed.inject(NotificationsService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('initial signal state', () => {
    it('should have empty notifications signal initially', () => {
      expect(service.notifications()).toEqual([]);
    });

    it('should have loading signal as false initially', () => {
      expect(service.loading()).toBeFalse();
    });

    it('should have totalCount signal as 0 initially', () => {
      expect(service.totalCount()).toBe(0);
    });

    it('should have failedCount computed signal as 0 initially', () => {
      expect(service.failedCount()).toBe(0);
    });
  });

  describe('getNotifications()', () => {
    it('should make GET request to /api/v1/notifications', () => {
      service.getNotifications({ page: 1, pageSize: 10 }).subscribe();
      const req = httpMock.expectOne((r) => r.url === BASE_URL);
      expect(req.request.method).toBe('GET');
      req.flush(mockPagedResult);
    });

    it('should include page and pageSize as query params', () => {
      service.getNotifications({ page: 2, pageSize: 5 }).subscribe();
      const req = httpMock.expectOne((r) => r.url === BASE_URL);
      expect(req.request.params.get('page')).toBe('2');
      expect(req.request.params.get('pageSize')).toBe('5');
      req.flush(mockPagedResult);
    });

    it('should include optional status filter when provided', () => {
      service.getNotifications({ page: 1, pageSize: 10, status: 'Failed' }).subscribe();
      const req = httpMock.expectOne((r) => r.url === BASE_URL);
      expect(req.request.params.get('status')).toBe('Failed');
      req.flush(mockPagedResult);
    });

    it('should not include undefined status in query params', () => {
      service.getNotifications({ page: 1, pageSize: 10, status: undefined }).subscribe();
      const req = httpMock.expectOne((r) => r.url === BASE_URL);
      expect(req.request.params.has('status')).toBeFalse();
      req.flush(mockPagedResult);
    });

    it('should return the paged result observable', (done) => {
      service.getNotifications({ page: 1, pageSize: 10 }).subscribe((result) => {
        expect(result.items.length).toBe(1);
        expect(result.totalCount).toBe(1);
        done();
      });
      const req = httpMock.expectOne((r) => r.url === BASE_URL);
      req.flush(mockPagedResult);
    });

    it('should return an empty list when no notifications exist', (done) => {
      const emptyResult: PagedResult<NotificationDto> = { items: [], totalCount: 0, page: 1, pageSize: 10 };
      service.getNotifications({ page: 1, pageSize: 10 }).subscribe((result) => {
        expect(result.totalCount).toBe(0);
        expect(result.items).toEqual([]);
        done();
      });
      const req = httpMock.expectOne((r) => r.url === BASE_URL);
      req.flush(emptyResult);
    });
  });

  describe('retryNotification()', () => {
    it('should make PUT request to /api/v1/notifications/{id}/retry', () => {
      service.retryNotification('notif-2').subscribe();
      const req = httpMock.expectOne(`${BASE_URL}/notif-2/retry`);
      expect(req.request.method).toBe('PUT');
      req.flush({ ...mockFailedNotification1, status: 'Sent' });
    });

    it('should send an empty body to the retry endpoint', () => {
      service.retryNotification('notif-2').subscribe();
      const req = httpMock.expectOne(`${BASE_URL}/notif-2/retry`);
      expect(req.request.body).toEqual({});
      req.flush({ ...mockFailedNotification1, status: 'Sent' });
    });

    it('should return the updated notification', (done) => {
      service.retryNotification('notif-2').subscribe((notification) => {
        expect(notification.status).toBe('Sent');
        done();
      });
      const req = httpMock.expectOne(`${BASE_URL}/notif-2/retry`);
      req.flush({ ...mockFailedNotification1, status: 'Sent' });
    });
  });

  describe('failedCount computed signal', () => {
    it('should return 0 when notifications signal is empty', () => {
      expect(service.failedCount()).toBe(0);
    });

    it('should return the count of Failed notifications', () => {
      service.notifications.set([
        mockFailedNotification1,
        mockFailedNotification2,
        mockSentNotification,
        mockPendingNotification,
      ]);
      expect(service.failedCount()).toBe(2);
    });

    it('should return 0 when there are no Failed notifications', () => {
      service.notifications.set([mockSentNotification, mockPendingNotification]);
      expect(service.failedCount()).toBe(0);
    });

    it('should return the total count when all notifications are Failed', () => {
      service.notifications.set([mockFailedNotification1, mockFailedNotification2]);
      expect(service.failedCount()).toBe(2);
    });

    it('should update reactively when notifications signal changes', () => {
      service.notifications.set([mockSentNotification]);
      expect(service.failedCount()).toBe(0);

      service.notifications.set([mockFailedNotification1, mockSentNotification]);
      expect(service.failedCount()).toBe(1);
    });
  });
});
