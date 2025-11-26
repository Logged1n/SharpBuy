// API Response Types
export interface ApiError {
  type: string;
  title: string;
  status: number;
  detail?: string;
  errors?: Record<string, string[]>;
}

// User Types
export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface AuthResponse {
  token: string;
}

// Product Types
export interface Product {
  id: string;
  name: string;
  description: string;
  price: number;
  currency: string;
  stockQuantity: number;
  photoPaths?: string[];
  categories?: Category[];
}

export interface AddProductRequest {
  name: string;
  description: string;
  priceAmount: number;
  priceCurrency: string;
  stockQuantity: number;
  photoPaths?: string[];
}

// Category Types
export interface Category {
  id: string;
  name: string;
  description?: string;
}

export interface AddCategoryRequest {
  name: string;
  description?: string;
}

// Cart Types
export interface Cart {
  id: string;
  userId: string;
  items: CartItem[];
}

export interface CartItem {
  id: string;
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  currency: string;
}

// Order Types
export enum OrderStatus {
  Pending = "Pending",
  Confirmed = "Confirmed",
  Shipped = "Shipped",
  Delivered = "Delivered",
  Cancelled = "Cancelled",
}

export interface Order {
  id: string;
  userId: string;
  status: OrderStatus;
  totalAmount: number;
  currency: string;
  items: OrderItem[];
  shippingAddress?: Address;
  createdAt: string;
}

export interface OrderItem {
  id: string;
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  currency: string;
}

// Address Types
export interface Address {
  street: string;
  city: string;
  state: string;
  country: string;
  postalCode: string;
}

// Review Types
export interface Review {
  id: string;
  productId: string;
  userId: string;
  userName: string;
  rating: number;
  comment?: string;
  createdAt: string;
}

export interface AddReviewRequest {
  productId: string;
  rating: number;
  comment?: string;
}
