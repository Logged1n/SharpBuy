const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'https://localhost:5001';

export interface ApiError {
  type: string;
  title: string;
  status: number;
  detail?: string;
  errors?: Record<string, string[]>;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AddressDto {
  line1: string;
  line2?: string | null;
  city: string;
  postalCode: string;
  country: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  phoneNumber: string;
  primaryAddress?: AddressDto | null;
  additionalAddresses?: AddressDto[] | null;
}

export interface Product {
  id: string;
  name: string;
  description: string;
  price: number;
  currency: string;
  stockQuantity: number;
  photoPaths?: string[];
}

export interface AddProductRequest {
  name: string;
  description: string;
  priceAmount: number;
  priceCurrency: string;
  stockQuantity: number;
}

class ApiClient {
  private baseUrl: string;

  constructor(baseUrl: string = API_BASE_URL) {
    this.baseUrl = baseUrl;
  }

  private getHeaders(includeAuth: boolean = false): HeadersInit {
    const headers: HeadersInit = {
      'Content-Type': 'application/json',
    };

    if (includeAuth && typeof window !== 'undefined') {
      const token = localStorage.getItem('auth_token');
      if (token) {
        headers['Authorization'] = `Bearer ${token}`;
      }
    }

    return headers;
  }

  private async handleResponse<T>(response: Response): Promise<T> {
    if (!response.ok) {
      const error: ApiError = await response.json().catch(() => ({
        type: 'Error',
        title: 'An error occurred',
        status: response.status,
        detail: response.statusText,
      }));
      throw error;
    }

    const contentType = response.headers.get('content-type');
    if (contentType && contentType.includes('application/json')) {
      return response.json();
    }

    return {} as T;
  }

  async login(data: LoginRequest): Promise<LoginResponse> {
    const response = await fetch(`${this.baseUrl}/users/login`, {
      method: 'POST',
      headers: this.getHeaders(),
      body: JSON.stringify(data),
    });

    return this.handleResponse<LoginResponse>(response);
  }

  async register(data: RegisterRequest): Promise<string> {
    const response = await fetch(`${this.baseUrl}/users/register`, {
      method: 'POST',
      headers: this.getHeaders(),
      body: JSON.stringify(data),
    });

    return this.handleResponse<string>(response);
  }

  async getUser(id: string): Promise<any> {
    const response = await fetch(`${this.baseUrl}/users/${id}`, {
      method: 'GET',
      headers: this.getHeaders(true),
    });

    return this.handleResponse(response);
  }

  async getPermissions(): Promise<string[]> {
    const response = await fetch(`${this.baseUrl}/users/permissions`, {
      method: 'GET',
      headers: this.getHeaders(true),
    });

    return this.handleResponse<string[]>(response);
  }

  async addProduct(data: AddProductRequest): Promise<string> {
    const response = await fetch(`${this.baseUrl}/products`, {
      method: 'POST',
      headers: this.getHeaders(true),
      body: JSON.stringify(data),
    });

    return this.handleResponse<string>(response);
  }
}

export const api = new ApiClient();
