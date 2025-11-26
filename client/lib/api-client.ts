import { config } from './config';
import type {
  ApiError,
  User,
  LoginRequest,
  RegisterRequest,
  AuthResponse,
  Product,
  AddProductRequest,
  Category,
  AddCategoryRequest,
  Cart,
  Order,
  Review,
  AddReviewRequest,
} from '@/types';

class ApiClient {
  private baseUrl: string;

  constructor() {
    this.baseUrl = config.apiUrl;
  }

  private getAuthToken(): string | null {
    if (typeof window === 'undefined') return null;
    return localStorage.getItem('token');
  }

  private async request<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<T> {
    const token = this.getAuthToken();
    const headers: HeadersInit = {
      'Content-Type': 'application/json',
      ...options.headers,
    };

    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    const response = await fetch(`${this.baseUrl}${endpoint}`, {
      ...options,
      headers,
    });

    if (!response.ok) {
      const error: ApiError = await response.json();
      throw error;
    }

    // Handle 204 No Content
    if (response.status === 204) {
      return {} as T;
    }

    return response.json();
  }

  // Auth endpoints
  async login(credentials: LoginRequest): Promise<AuthResponse> {
    const response = await this.request<AuthResponse>('/users/login', {
      method: 'POST',
      body: JSON.stringify(credentials),
    });

    // Store token in localStorage
    if (typeof window !== 'undefined' && response.token) {
      localStorage.setItem('token', response.token);
    }

    return response;
  }

  async register(data: RegisterRequest): Promise<string> {
    return this.request<string>('/users/register', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  logout(): void {
    if (typeof window !== 'undefined') {
      localStorage.removeItem('token');
    }
  }

  async verifyEmail(token: string): Promise<void> {
    await this.request('/users/verify-email', {
      method: 'POST',
      body: JSON.stringify({ token }),
    });
  }

  // User endpoints
  async getUserById(id: string): Promise<User> {
    return this.request<User>(`/users/${id}`);
  }

  async getCurrentUserPermissions(): Promise<string[]> {
    return this.request<string[]>('/users/permissions');
  }

  // Product endpoints
  async getProducts(): Promise<Product[]> {
    return this.request<Product[]>('/products');
  }

  async getProductById(id: string): Promise<Product> {
    return this.request<Product>(`/products/${id}`);
  }

  async addProduct(data: AddProductRequest): Promise<string> {
    return this.request<string>('/products', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  async updateProductPrice(
    id: string,
    amount: number,
    currency: string
  ): Promise<void> {
    await this.request(`/products/${id}/price`, {
      method: 'PUT',
      body: JSON.stringify({ amount, currency }),
    });
  }

  // Category endpoints
  async getCategories(): Promise<Category[]> {
    return this.request<Category[]>('/categories');
  }

  async getCategoryById(id: string): Promise<Category> {
    return this.request<Category>(`/categories/${id}`);
  }

  async addCategory(data: AddCategoryRequest): Promise<string> {
    return this.request<string>('/categories', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  // Cart endpoints
  async getCart(): Promise<Cart> {
    return this.request<Cart>('/cart');
  }

  async addToCart(productId: string, quantity: number): Promise<void> {
    await this.request('/cart/items', {
      method: 'POST',
      body: JSON.stringify({ productId, quantity }),
    });
  }

  async updateCartItem(itemId: string, quantity: number): Promise<void> {
    await this.request(`/cart/items/${itemId}`, {
      method: 'PUT',
      body: JSON.stringify({ quantity }),
    });
  }

  async removeFromCart(itemId: string): Promise<void> {
    await this.request(`/cart/items/${itemId}`, {
      method: 'DELETE',
    });
  }

  async clearCart(): Promise<void> {
    await this.request('/cart', {
      method: 'DELETE',
    });
  }

  // Order endpoints
  async getOrders(): Promise<Order[]> {
    return this.request<Order[]>('/orders');
  }

  async getOrderById(id: string): Promise<Order> {
    return this.request<Order>(`/orders/${id}`);
  }

  async createOrder(cartId: string): Promise<string> {
    return this.request<string>('/orders', {
      method: 'POST',
      body: JSON.stringify({ cartId }),
    });
  }

  // Review endpoints
  async getProductReviews(productId: string): Promise<Review[]> {
    return this.request<Review[]>(`/products/${productId}/reviews`);
  }

  async addReview(data: AddReviewRequest): Promise<string> {
    return this.request<string>('/reviews', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }
}

export const apiClient = new ApiClient();
