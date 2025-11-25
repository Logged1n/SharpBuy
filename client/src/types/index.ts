// API Response Types
export interface ApiError {
  type: string;
  title: string;
  status: number;
  errors?: Record<string, string[]>;
}

// User Types
export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  emailVerified: boolean;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  token: string;
}

export interface VerifyEmailRequest {
  email: string;
  token: string;
}

// Product Types
export interface Product {
  id: string;
  name: string;
  description: string;
  priceAmount: number;
  priceCurrency: string;
  stockQuantity: number;
  photoPaths?: string[];
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
}

export interface AddCategoryRequest {
  name: string;
}

// Cart Types
export interface CartItem {
  id: string;
  productId: string;
  productName: string;
  quantity: number;
  priceAmount: number;
  priceCurrency: string;
}

export interface Cart {
  id: string;
  userId: string;
  items: CartItem[];
  totalAmount: number;
  totalCurrency: string;
}

// Order Types
export interface Order {
  id: string;
  userId: string;
  status: OrderStatus;
  totalAmount: number;
  totalCurrency: string;
  items: OrderItem[];
  createdAt: string;
}

export interface OrderItem {
  id: string;
  productId: string;
  productName: string;
  quantity: number;
  priceAmount: number;
  priceCurrency: string;
}

export enum OrderStatus {
  Pending = "Pending",
  Confirmed = "Confirmed",
  Shipped = "Shipped",
  Delivered = "Delivered",
  Cancelled = "Cancelled",
}

// Address Types
export interface Address {
  street: string;
  city: string;
  state: string;
  country: string;
  zipCode: string;
}
