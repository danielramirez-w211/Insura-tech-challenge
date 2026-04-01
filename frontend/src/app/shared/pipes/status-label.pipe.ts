import { Pipe, PipeTransform } from '@angular/core';

const STATUS_LABELS: Record<string, string> = {
  // Policy
  Pending: 'Pendiente',
  Active: 'Activa',
  Suspended: 'Suspendida',
  Expired: 'Vencida',
  Cancelled: 'Cancelada',
  // Claim
  Registered: 'Registrado',
  Approved: 'Aprobado',
  Rejected: 'Rechazado',
  Appealed: 'Apelado',
  Paid: 'Pagado',
  // Notification
  Sent: 'Enviado',
  Failed: 'Fallido',
  // Events
  PolicyActivated: 'Póliza Activada',
  PolicyExpiringSoon: 'Póliza por Vencer',
  ClaimRegistered: 'Siniestro Registrado',
  ClaimStatusChanged: 'Estado de Siniestro',
  // Policy types
  Life: 'Vida',
  Health: 'Salud',
  Vehicle: 'Vehículo',
  Home: 'Hogar',
  Travel: 'Viaje',
};

@Pipe({ name: 'statusLabel', standalone: true })
export class StatusLabelPipe implements PipeTransform {
  transform(value: string): string {
    return STATUS_LABELS[value] ?? value;
  }
}
