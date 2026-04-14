export type DocumentType = 'CC' | 'CE' | 'TI' | 'PP' | 'RC';

export interface DocumentTypeOption {
  code: DocumentType;
  label: string;
}

export const DOCUMENT_TYPE_OPTIONS: DocumentTypeOption[] = [
  { code: 'CC', label: 'Cédula de Ciudadanía' },
  { code: 'CE', label: 'Cédula de Extranjería' },
  { code: 'TI', label: 'Tarjeta de Identidad' },
  { code: 'PP', label: 'Pasaporte' },
  { code: 'RC', label: 'Registro Civil' },
];

export interface InsuredPerson {
  firstName: string;
  lastName: string;
  documentType: DocumentType;
  documentId: string;
  birthDate: string;
  email: string;
  phone: string;
  gender: 'Masculino' | 'Femenino';
  address: string;
  cityName: string;
  postalCode: string;
  department: string;
}
