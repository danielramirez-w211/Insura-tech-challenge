export interface UserProfile {
  firstName:       string;
  lastName:        string;
  nationality?:    string;
  birthDate?:      string;
  yearsInCompany?: number;
  photoUrl?:       string;
  officeLocation?: string;
  workSchedule?:   string;
}

export interface UserDetail {
  id:          string;
  email:       string;
  role:        'Admin' | 'Leader' | 'Advisor';
  isActive:    boolean;
  advisorCode: string | null;
  leaderId:    string | null;
  profile:     UserProfile;
}

export interface AdvisorSummary {
  id:          string;
  email:       string;
  advisorCode: string;
  isActive:    boolean;
  firstName:   string;
  lastName:    string;
  salesCount:  number;
}

export interface CreateLeaderRequest {
  email:           string;
  firstName:       string;
  lastName:        string;
  officeLocation?: string;
  workSchedule?:   string;
}

export interface CreateAdvisorRequest {
  email:           string;
  firstName:       string;
  lastName:        string;
  nationality?:    string;
  birthDate?:      string;
  yearsInCompany?: number;
  officeLocation?: string;
  workSchedule?:   string;
}

export interface UpdateProfileRequest {
  firstName:       string;
  lastName:        string;
  nationality?:    string;
  birthDate?:      string;
  yearsInCompany?: number;
  photoUrl?:       string;
  officeLocation?: string;
  workSchedule?:   string;
}
