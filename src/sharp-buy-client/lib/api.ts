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
  photoPaths: string[];
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

// Order types
export enum OrderStatus {
  Open = 0,
  Confirmed = 1,
  Shipped = 2,
  Arrived = 3,
  Collected = 4,
  Completed = 5,
  Returning = 6,
  Cancelled = 7,
}

export interface OrderItem {
  id: string;
  productId: string;
  productName: string;
  unitPriceAmount: number;
  unitPriceCurrency: string;
  quantity: number;
  totalPriceAmount: number;
  totalPriceCurrency: string;
}

export interface OrderAddress {
  line1: string;
  line2?: string | null;
  city: string;
  postalCode: string;
  country: string;
}

export interface Order {
  id: string;
  userId: string;
  userFirstName: string;
  userLastName: string;
  userEmail: string;
  createdAt: string;
  modifiedAt: string;
  completedAt?: string | null;
  status: OrderStatus;
  shippingAddressId?: string | null;
  billingAddressId?: string | null;
  shippingAddress?: OrderAddress | null;
  billingAddress?: OrderAddress | null;
  totalAmount: number;
  totalCurrency: string;
  items: OrderItem[];
}

export interface UpdateOrderStatusRequest {
  newStatus: OrderStatus;
}

// Review types
export interface Review {
  id: string;
  score: number;
  title: string;
  description?: string | null;
  createdAt: string;
  userName: string;
}

export interface AddReviewRequest {
  productId: string;
  score: number;
  title: string;
  description?: string | null;
}

// Analytics types
export type Granularity = 'Day' | 'Week' | 'Month';

export interface AnalyticsRequest {
  startDate: string;
  endDate: string;
  granularity: Granularity;
}

export type PdfJobState = 'Pending' | 'Processing' | 'Completed' | 'Failed';

export interface PdfJobStatus {
  jobId: string;
  state: PdfJobState;
  pdfCacheKey: string | null;
  errorMessage: string | null;
  createdAt: string;
  completedAt: string | null;
}

export interface PdfJobResponse {
  jobId: string;
  message: string;
  statusUrl: string;
}

export interface SalesDataPoint {
  date: string;
  revenue: number;
  orderCount: number;
}

export interface SalesAnalyticsResponse {
  totalRevenue: number;
  totalOrders: number;
  averageOrderValue: number;
  growthPercentage: number;
  dataPoints: SalesDataPoint[];
}

export interface ProductPerformance {
  productId: string;
  productName: string;
  quantitySold: number;
  revenue: number;
  period: string | null;
}

export interface ProductAnalyticsResponse {
  totalProductsSold: number;
  totalRevenue: number;
  topProducts: ProductPerformance[];
  productPerformanceByPeriod: ProductPerformance[];
}

export interface CustomerDataPoint {
  date: string;
  newCustomers: number;
  returningCustomers: number;
}

export interface CustomerAnalyticsResponse {
  totalCustomers: number;
  newCustomers: number;
  returningCustomers: number;
  averageCustomerValue: number;
  dataPoints: CustomerDataPoint[];
}

export interface OrderDataPoint {
  date: string;
  completed: number;
  pending: number;
  cancelled: number;
}

export interface OrderAnalyticsResponse {
  totalOrders: number;
  completedOrders: number;
  pendingOrders: number;
  cancelledOrders: number;
  completionRate: number;
  dataPoints: OrderDataPoint[];
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

  async getUserProfile(): Promise<{
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    addressId?: string | null;
    address?: AddressDto | null;
  }> {
    const response = await fetch(`${this.baseUrl}/users/profile`, {
      method: 'GET',
      headers: this.getHeaders(true),
    });

    return this.handleResponse(response);
  }

  async updateUserProfile(data: {
    email: string;
    firstName: string;
    lastName: string;
    address?: AddressDto | null;
  }): Promise<void> {
    const response = await fetch(`${this.baseUrl}/users/profile`, {
      method: 'PUT',
      headers: this.getHeaders(true),
      body: JSON.stringify(data),
    });

    return this.handleResponse<void>(response);
  }

  // Products
  async getProducts(
    page: number = 1,
    pageSize: number = 100,
    searchTerm?: string,
    categoryIds?: string[]
  ): Promise<PagedResult<ProductListItem>> {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
    });

    if (searchTerm) {
      params.append('searchTerm', searchTerm);
    }

    if (categoryIds && categoryIds.length > 0) {
      categoryIds.forEach(id => params.append('categoryIds', id));
    }

    const response = await fetch(`${this.baseUrl}/products?${params.toString()}`, {
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

  async addProduct(data: AddProductRequest, imageFile?: File): Promise<string> {
    const formData = new FormData();
    formData.append('name', data.name);
    formData.append('description', data.description);
    formData.append('quantity', data.quantity.toString());
    formData.append('priceAmount', data.price.amount.toString());
    formData.append('priceCurrency', data.price.currency);
    formData.append('categoryIds', data.categoryIds.join(','));

    if (imageFile) {
      formData.append('image', imageFile);
    }

    const token = typeof window !== 'undefined' ? localStorage.getItem('auth_token') : null;
    const headers: HeadersInit = {};
    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    const response = await fetch(`${this.baseUrl}/products`, {
      method: 'POST',
      headers,
      body: formData,
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

  async uploadProductImage(file: File): Promise<{ path: string }> {
    const formData = new FormData();
    formData.append('file', file);

    const token = typeof window !== 'undefined' ? localStorage.getItem('auth_token') : null;
    const headers: HeadersInit = {};
    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    const response = await fetch(`${this.baseUrl}/products/upload-image`, {
      method: 'POST',
      headers,
      body: formData,
    });

    return this.handleResponse<{ path: string }>(response);
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

  // Orders
  async createPaymentIntent(): Promise<{ clientSecret: string }> {
    const response = await fetch(`${this.baseUrl}/orders/payment-intent`, {
      method: 'POST',
      headers: this.getHeaders(true),
    });

    return this.handleResponse<{ clientSecret: string }>(response);
  }

  async placeOrder(data: {
    shippingAddressId?: string | null;
    billingAddressId?: string | null;
    shippingAddress?: OrderAddress | null;
    billingAddress?: OrderAddress | null;
    paymentIntentId: string;
  }): Promise<string> {
    const response = await fetch(`${this.baseUrl}/orders`, {
      method: 'POST',
      headers: this.getHeaders(true),
      body: JSON.stringify(data),
    });

    return this.handleResponse<string>(response);
  }

  async getOrders(page: number = 1, pageSize: number = 20): Promise<PagedResult<Order>> {
    const response = await fetch(`${this.baseUrl}/orders?page=${page}&pageSize=${pageSize}`, {
      method: 'GET',
      headers: this.getHeaders(true),
    });

    return this.handleResponse<PagedResult<Order>>(response);
  }

  async getOrder(id: string): Promise<Order> {
    const response = await fetch(`${this.baseUrl}/orders/${id}`, {
      method: 'GET',
      headers: this.getHeaders(true),
    });

    return this.handleResponse<Order>(response);
  }

  async updateOrderStatus(id: string, data: UpdateOrderStatusRequest): Promise<void> {
    const response = await fetch(`${this.baseUrl}/orders/${id}/status`, {
      method: 'PUT',
      headers: this.getHeaders(true),
      body: JSON.stringify(data),
    });

    return this.handleResponse<void>(response);
  }

  // Reviews
  async getProductReviews(productId: string): Promise<Review[]> {
    const response = await fetch(`${this.baseUrl}/products/${productId}/reviews`, {
      method: 'GET',
      headers: this.getHeaders(),
    });

    return this.handleResponse<Review[]>(response);
  }

  async addReview(data: AddReviewRequest): Promise<string> {
    const response = await fetch(`${this.baseUrl}/reviews`, {
      method: 'POST',
      headers: this.getHeaders(true),
      body: JSON.stringify(data),
    });

    return this.handleResponse<string>(response);
  }

  // Analytics
  async getSalesAnalytics(request: AnalyticsRequest): Promise<SalesAnalyticsResponse> {
    const response = await fetch(`${this.baseUrl}/analytics/sales`, {
      method: 'POST',
      headers: this.getHeaders(true),
      body: JSON.stringify(request),
    });

    return this.handleResponse<SalesAnalyticsResponse>(response);
  }

  async getProductAnalytics(request: AnalyticsRequest): Promise<ProductAnalyticsResponse> {
    const response = await fetch(`${this.baseUrl}/analytics/products`, {
      method: 'POST',
      headers: this.getHeaders(true),
      body: JSON.stringify(request),
    });

    return this.handleResponse<ProductAnalyticsResponse>(response);
  }

  async getCustomerAnalytics(request: AnalyticsRequest): Promise<CustomerAnalyticsResponse> {
    const response = await fetch(`${this.baseUrl}/analytics/customers`, {
      method: 'POST',
      headers: this.getHeaders(true),
      body: JSON.stringify(request),
    });

    return this.handleResponse<CustomerAnalyticsResponse>(response);
  }

  async getOrderAnalytics(request: AnalyticsRequest): Promise<OrderAnalyticsResponse> {
    const response = await fetch(`${this.baseUrl}/analytics/orders`, {
      method: 'POST',
      headers: this.getHeaders(true),
      body: JSON.stringify(request),
    });

    return this.handleResponse<OrderAnalyticsResponse>(response);
  }

  async downloadSalesReport(request: AnalyticsRequest): Promise<PdfJobResponse> {
    const response = await fetch(`${this.baseUrl}/analytics/reports/sales`, {
      method: 'POST',
      headers: this.getHeaders(true),
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      throw new Error('Failed to start sales report generation');
    }

    return response.json();
  }

  async downloadProductReport(request: AnalyticsRequest): Promise<PdfJobResponse> {
    const response = await fetch(`${this.baseUrl}/analytics/reports/products`, {
      method: 'POST',
      headers: this.getHeaders(true),
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      throw new Error('Failed to start product report generation');
    }

    return response.json();
  }

  async downloadCustomerReport(request: AnalyticsRequest): Promise<PdfJobResponse> {
    const response = await fetch(`${this.baseUrl}/analytics/reports/customers`, {
      method: 'POST',
      headers: this.getHeaders(true),
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      throw new Error('Failed to start customer report generation');
    }

    return response.json();
  }

  async downloadOrderReport(request: AnalyticsRequest): Promise<PdfJobResponse> {
    const response = await fetch(`${this.baseUrl}/analytics/reports/orders`, {
      method: 'POST',
      headers: this.getHeaders(true),
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      throw new Error('Failed to start order report generation');
    }

    return response.json();
  }

  async getPdfJobStatus(jobId: string): Promise<PdfJobStatus> {
    const response = await fetch(`${this.baseUrl}/analytics/pdf-jobs/${jobId}`, {
      method: 'GET',
      headers: this.getHeaders(true),
    });

    if (!response.ok) {
      throw new Error('Failed to get PDF job status');
    }

    return response.json();
  }

  async downloadPdf(cacheKey: string): Promise<Blob> {
    const response = await fetch(`${this.baseUrl}/analytics/pdf/${encodeURIComponent(cacheKey)}`, {
      method: 'GET',
      headers: this.getHeaders(true),
    });

    if (!response.ok) {
      throw new Error('Failed to download PDF');
    }

    return response.blob();
  }

  async pollPdfJobUntilComplete(
    jobId: string,
    onProgress?: (status: PdfJobStatus) => void,
    maxAttempts: number = 60,
    intervalMs: number = 1000
  ): Promise<PdfJobStatus> {
    for (let attempt = 0; attempt < maxAttempts; attempt++) {
      const status = await this.getPdfJobStatus(jobId);

      if (onProgress) {
        onProgress(status);
      }

      if (status.state === 'Completed' || status.state === 'Failed') {
        return status;
      }

      await new Promise(resolve => setTimeout(resolve, intervalMs));
    }

    throw new Error('PDF generation timeout');
  }
}

export const api = new ApiClient();
