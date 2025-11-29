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

// Backend returns token as plain string
export type LoginResponse = string;

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

// Money value object (matches backend exactly)
export interface Money {
  amount: number;
  currency: string;
}

// Paging
export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

// Product types
export interface Product {
  id: string;
  name: string;
  description: string;
  priceAmount: number;
  priceCurrency: string;
  stockQuantity: number;
  mainPhotoPath: string;
  photoPaths?: string[];
  categories?: CategoryInfo[];
}

export interface CategoryInfo {
  id: string;
  name: string;
}

export interface ProductListItem {
  id: string;
  name: string;
  description: string;
  priceAmount: number;
  priceCurrency: string;
  stockQuantity: number;
  mainPhotoPath: string;
}

export interface AddProductRequest {
  name: string;
  description: string;
  quantity: number;
  price: Money; // Changed to use Money type
  categoryIds: string[];
  mainPhotoPath: string;
}

export interface UpdateProductRequest {
  name: string;
  description: string;
  priceAmount: number;
  priceCurrency: string;
}

// Category types
export interface Category {
  id: string;
  name: string;
  productCount: number;
}

export interface CategoryListItem {
  id: string;
  name: string;
  productCount: number;
}

export interface AddCategoryRequest {
  name: string;
}

export interface UpdateCategoryRequest {
  name: string;
}

// Cart types
export interface CartItem {
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  currency: string;
  mainPhotoPath: string;
}

export interface Cart {
  id: string;
  ownerId: string;
  items: CartItem[];
  totalAmount: number;
  currency: string;
}

export interface AddItemToCartRequest {
  productId: string;
  quantity: number;
}

export interface UpdateCartItemQuantityRequest {
  quantity: number;
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

    // For 204 No Content responses
    return {} as T;
  }

  // Auth
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

  // Products
  async getProducts(page: number = 1, pageSize: number = 100): Promise<PagedResult<ProductListItem>> {
    const response = await fetch(`${this.baseUrl}/products?page=${page}&pageSize=${pageSize}`, {
      method: 'GET',
      headers: this.getHeaders(),
    });

    return this.handleResponse<PagedResult<ProductListItem>>(response);
  }

  async getProduct(id: string): Promise<Product> {
    const response = await fetch(`${this.baseUrl}/products/${id}`, {
      method: 'GET',
      headers: this.getHeaders(),
    });

    return this.handleResponse<Product>(response);
  }

  async addProduct(data: AddProductRequest): Promise<string> {
    const response = await fetch(`${this.baseUrl}/products`, {
      method: 'POST',
      headers: this.getHeaders(true),
      body: JSON.stringify(data),
    });

    return this.handleResponse<string>(response);
  }

  async updateProduct(id: string, data: UpdateProductRequest): Promise<void> {
    const response = await fetch(`${this.baseUrl}/products/${id}`, {
      method: 'PUT',
      headers: this.getHeaders(true),
      body: JSON.stringify(data),
    });

    return this.handleResponse<void>(response);
  }

  async deleteProduct(id: string): Promise<void> {
    const response = await fetch(`${this.baseUrl}/products/${id}`, {
      method: 'DELETE',
      headers: this.getHeaders(true),
    });

    return this.handleResponse<void>(response);
  }

  // Categories
  async getCategories(page: number = 1, pageSize: number = 100): Promise<PagedResult<CategoryListItem>> {
    const response = await fetch(`${this.baseUrl}/categories?page=${page}&pageSize=${pageSize}`, {
      method: 'GET',
      headers: this.getHeaders(),
    });

    return this.handleResponse<PagedResult<CategoryListItem>>(response);
  }

  async getCategory(id: string): Promise<Category> {
    const response = await fetch(`${this.baseUrl}/categories/${id}`, {
      method: 'GET',
      headers: this.getHeaders(),
    });

    return this.handleResponse<Category>(response);
  }

  async addCategory(data: AddCategoryRequest): Promise<string> {
    const response = await fetch(`${this.baseUrl}/categories`, {
      method: 'POST',
      headers: this.getHeaders(true),
      body: JSON.stringify(data),
    });

    return this.handleResponse<string>(response);
  }

  async updateCategory(id: string, data: UpdateCategoryRequest): Promise<void> {
    const response = await fetch(`${this.baseUrl}/categories/${id}`, {
      method: 'PUT',
      headers: this.getHeaders(true),
      body: JSON.stringify(data),
    });

    return this.handleResponse<void>(response);
  }

  async deleteCategory(id: string): Promise<void> {
    const response = await fetch(`${this.baseUrl}/categories/${id}`, {
      method: 'DELETE',
      headers: this.getHeaders(true),
    });

    return this.handleResponse<void>(response);
  }

  // Cart
  async getCart(): Promise<Cart> {
    const response = await fetch(`${this.baseUrl}/carts`, {
      method: 'GET',
      headers: this.getHeaders(true),
    });

    return this.handleResponse<Cart>(response);
  }

  async addItemToCart(data: AddItemToCartRequest): Promise<void> {
    const response = await fetch(`${this.baseUrl}/carts/items`, {
      method: 'POST',
      headers: this.getHeaders(true),
      body: JSON.stringify(data),
    });

    return this.handleResponse<void>(response);
  }

  async updateCartItemQuantity(productId: string, data: UpdateCartItemQuantityRequest): Promise<void> {
    const response = await fetch(`${this.baseUrl}/carts/items/${productId}`, {
      method: 'PUT',
      headers: this.getHeaders(true),
      body: JSON.stringify(data),
    });

    return this.handleResponse<void>(response);
  }

  async removeCartItem(productId: string): Promise<void> {
    const response = await fetch(`${this.baseUrl}/carts/items/${productId}`, {
      method: 'DELETE',
      headers: this.getHeaders(true),
    });

    return this.handleResponse<void>(response);
  }

  async clearCart(): Promise<void> {
    const response = await fetch(`${this.baseUrl}/carts`, {
      method: 'DELETE',
      headers: this.getHeaders(true),
    });

    return this.handleResponse<void>(response);
  }
}

export const api = new ApiClient();
