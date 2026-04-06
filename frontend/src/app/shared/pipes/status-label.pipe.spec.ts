import { StatusLabelPipe } from './status-label.pipe';

describe('StatusLabelPipe', () => {
  let pipe: StatusLabelPipe;

  beforeEach(() => {
    pipe = new StatusLabelPipe();
  });

  it('should create an instance', () => {
    expect(pipe).toBeTruthy();
  });

  describe('PolicyStatus values', () => {
    it('should transform "Pending" to "Pendiente"', () => {
      expect(pipe.transform('Pending')).toBe('Pendiente');
    });

    it('should transform "Active" to "Activa"', () => {
      expect(pipe.transform('Active')).toBe('Activa');
    });

    it('should transform "Suspended" to "Suspendida"', () => {
      expect(pipe.transform('Suspended')).toBe('Suspendida');
    });

    it('should transform "Expired" to "Vencida"', () => {
      expect(pipe.transform('Expired')).toBe('Vencida');
    });

    it('should transform "Cancelled" to "Cancelada"', () => {
      expect(pipe.transform('Cancelled')).toBe('Cancelada');
    });
  });

  describe('PolicyType values', () => {
    it('should transform "Life" to "Vida"', () => {
      expect(pipe.transform('Life')).toBe('Vida');
    });

    it('should transform "Health" to "Salud"', () => {
      expect(pipe.transform('Health')).toBe('Salud');
    });

    it('should transform "Vehicle" to "Vehículo"', () => {
      expect(pipe.transform('Vehicle')).toBe('Vehículo');
    });

    it('should transform "Home" to "Hogar"', () => {
      expect(pipe.transform('Home')).toBe('Hogar');
    });

    it('should transform "Travel" to "Viaje"', () => {
      expect(pipe.transform('Travel')).toBe('Viaje');
    });
  });

  describe('ClaimStatus values', () => {
    it('should transform "Registered" to "Registrado"', () => {
      expect(pipe.transform('Registered')).toBe('Registrado');
    });

    it('should transform "Approved" to "Aprobado"', () => {
      expect(pipe.transform('Approved')).toBe('Aprobado');
    });

    it('should transform "Rejected" to "Rechazado"', () => {
      expect(pipe.transform('Rejected')).toBe('Rechazado');
    });

    it('should transform "Appealed" to "Apelado"', () => {
      expect(pipe.transform('Appealed')).toBe('Apelado');
    });

    it('should transform "Paid" to "Pagado"', () => {
      expect(pipe.transform('Paid')).toBe('Pagado');
    });
  });

  describe('NotificationStatus values', () => {
    it('should transform "Sent" to "Enviado"', () => {
      expect(pipe.transform('Sent')).toBe('Enviado');
    });

    it('should transform "Failed" to "Fallido"', () => {
      expect(pipe.transform('Failed')).toBe('Fallido');
    });
  });

  describe('NotificationEvent values', () => {
    it('should transform "PolicyActivated" to "Póliza Activada"', () => {
      expect(pipe.transform('PolicyActivated')).toBe('Póliza Activada');
    });

    it('should transform "PolicyExpiringSoon" to "Póliza por Vencer"', () => {
      expect(pipe.transform('PolicyExpiringSoon')).toBe('Póliza por Vencer');
    });

    it('should transform "ClaimRegistered" to "Siniestro Registrado"', () => {
      expect(pipe.transform('ClaimRegistered')).toBe('Siniestro Registrado');
    });

    it('should transform "ClaimStatusChanged" to "Estado de Siniestro"', () => {
      expect(pipe.transform('ClaimStatusChanged')).toBe('Estado de Siniestro');
    });
  });

  describe('fallback for unknown values', () => {
    it('should return the original value when it has no mapping', () => {
      expect(pipe.transform('UnknownStatus')).toBe('UnknownStatus');
    });

    it('should return empty string when given empty string', () => {
      expect(pipe.transform('')).toBe('');
    });

    it('should return the raw value for any unmapped string', () => {
      expect(pipe.transform('SomeRandomValue')).toBe('SomeRandomValue');
    });
  });
});
