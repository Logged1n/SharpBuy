import type {
  LoginRequest,
  LoginResponse,
  RegisterRequest,
  User,
  VerifyEmailRequest,
  Product,
  AddProductRequest,
  Category,
  AddCategoryRequest,
  ApiError,
} from "@/types";

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";

class ApiClient {
  private baseUrl: string;

  constructor(baseUrl: string = API_BASE_URL) {
    this.baseUrl = baseUrl;
  }

  private getAuthToken(): string | null {
    if (typeof window === "undefined") return null;
    return localStorage.getItem("token");
  }

  private async request<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<T> {
    const token = this.getAuthToken();
    const headers: HeadersInit = {
      "Content-Type": "application/json",
      ...options.headers,
    };

    if (token) {
      headers["Authorization"] = `Bearer ${token}`;
    }

    const response = await fetch(`${this.baseUrl}${endpoint}`, {
      ...options,
      headers,
    });

    if (!response.ok) {
      const error: ApiError = await response.json().catch(() => ({
        type: "Error",
        title: "An error occurred",
        status: response.status,
      }));
      throw error;
    }

    // Handle 204 No Content
    if (response.status === 204) {
      return {} as T;
    }

    return response.json();
  }

  // Auth endpoints
  async register(data: RegisterRequest): Promise<string> {
    return this.request<string>("/users/register", {
      method: "POST",
      body: JSON.stringify(data),
    });
  }

  async login(data: LoginRequest): Promise<LoginResponse> {
    return this.request<LoginResponse>("/users/login", {
      method: "POST",
      body: JSON.stringify(data),
    });
  }

  async verifyEmail(data: VerifyEmailRequest): Promise<void> {
    return this.request<void>("/users/verify-email", {
      method: "POST",
      body: JSON.stringify(data),
    });
  }

  // User endpoints
  async getUser(id: string): Promise<User> {
    return this.request<User>(`/users/${id}`);
  }

  async getUserPermissions(): Promise<string[]> {
    return this.request<string[]>("/users/permissions");
  }

  // Product endpoints
  async getProducts(): Promise<Product[]> {
    return this.request<Product[]>("/products");
  }

  async getProduct(id: string): Promise<Product> {
    return this.request<Product>(`/products/${id}`);
  }

  async addProduct(data: AddProductRequest): Promise<string> {
    return this.request<string>("/products", {
      method: "POST",
      body: JSON.stringify(data),
    });
  }

  // Category endpoints
  async getCategories(): Promise<Category[]> {
    return this.request<Category[]>("/categories");
  }

  async addCategory(data: AddCategoryRequest): Promise<string> {
    return this.request<string>("/categories", {
      method: "POST",
      body: JSON.stringify(data),
    });
  }
}

export const apiClient = new ApiClient();
