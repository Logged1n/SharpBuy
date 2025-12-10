'use client';

import { useState, useEffect } from 'react';
import { api, Order, OrderStatus } from '@/lib/api';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Loader2, Package, X, ChevronLeft, ChevronRight } from 'lucide-react';

export function AdminOrdersTab() {
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedOrder, setSelectedOrder] = useState<Order | null>(null);
  const [updating, setUpdating] = useState(false);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const pageSize = 10;

  useEffect(() => {
    loadOrders();
  }, [page]);

  const loadOrders = async () => {
    try {
      setLoading(true);
      const result = await api.getOrders(page, pageSize);
      setOrders(result.items);
      setTotalPages(result.totalPages);
    } catch (err: any) {
      setError(err.detail || 'Failed to load orders');
    } finally {
      setLoading(false);
    }
  };

  const getStatusBadgeVariant = (status: OrderStatus): 'default' | 'secondary' | 'destructive' | 'outline' => {
    switch (status) {
      case OrderStatus.Confirmed:
        return 'default';
      case OrderStatus.Shipped:
        return 'secondary';
      case OrderStatus.Completed:
        return 'default';
      case OrderStatus.Cancelled:
        return 'destructive';
      default:
        return 'outline';
    }
  };

  const getStatusDisplayName = (status: OrderStatus): string => {
    return OrderStatus[status];
  };

  const handleStatusChange = async (orderId: string, newStatus: OrderStatus) => {
    try {
      setUpdating(true);
      await api.updateOrderStatus(orderId, { newStatus });
      await loadOrders();
      if (selectedOrder?.id === orderId) {
        const updatedOrder = await api.getOrder(orderId);
        setSelectedOrder(updatedOrder);
      }
    } catch (err: any) {
      alert(err.detail || 'Failed to update order status');
    } finally {
      setUpdating(false);
    }
  };

  const formatAddress = (address: Order['shippingAddress']) => {
    if (!address) return 'N/A';
    return `${address.line1}${address.line2 ? ', ' + address.line2 : ''}, ${address.city}, ${address.postalCode}, ${address.country}`;
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <Loader2 className="h-8 w-8 animate-spin" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="p-4 bg-destructive/10 border border-destructive rounded-md">
        <p className="text-sm text-destructive">{error}</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {orders.length === 0 ? (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-12">
            <Package className="h-12 w-12 text-muted-foreground mb-4" />
            <p className="text-muted-foreground">No orders found</p>
          </CardContent>
        </Card>
      ) : (
        <>
          <div className="grid gap-4">
            {orders.map((order) => (
              <Card key={order.id} className="cursor-pointer hover:bg-muted/50 transition-colors" onClick={() => setSelectedOrder(order)}>
                <CardHeader>
                  <div className="flex items-start justify-between gap-4">
                    <div className="flex-1 min-w-0">
                      <CardTitle className="text-base truncate">
                        Order #{order.id.slice(0, 8)}
                      </CardTitle>
                      <CardDescription>
                        {order.userFirstName} {order.userLastName} • {new Date(order.createdAt).toLocaleString()}
                      </CardDescription>
                    </div>
                    <Badge variant={getStatusBadgeVariant(order.status)}>
                      {getStatusDisplayName(order.status)}
                    </Badge>
                  </div>
                </CardHeader>
                <CardContent>
                  <div className="grid grid-cols-1 md:grid-cols-3 gap-4 text-sm">
                    <div>
                      <p className="text-muted-foreground">Total Amount</p>
                      <p className="font-medium">
                        {order.totalCurrency} {order.totalAmount.toFixed(2)}
                      </p>
                    </div>
                    <div className="md:col-span-2">
                      <p className="text-muted-foreground">Email</p>
                      <p className="font-medium truncate">{order.userEmail}</p>
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>

          {/* Pagination */}
          {totalPages > 1 && (
            <div className="flex items-center justify-center gap-2">
              <Button
                variant="outline"
                size="sm"
                onClick={() => setPage(p => Math.max(1, p - 1))}
                disabled={page === 1}
              >
                <ChevronLeft className="h-4 w-4" />
                Previous
              </Button>
              <span className="text-sm text-muted-foreground">
                Page {page} of {totalPages}
              </span>
              <Button
                variant="outline"
                size="sm"
                onClick={() => setPage(p => Math.min(totalPages, p + 1))}
                disabled={page === totalPages}
              >
                Next
                <ChevronRight className="h-4 w-4" />
              </Button>
            </div>
          )}
        </>
      )}

      {/* Order Details Modal */}
      {selectedOrder && (
        <div
          className="fixed inset-0 bg-black/50 z-50 flex items-center justify-center p-4"
          onClick={() => setSelectedOrder(null)}
        >
          <Card
            className="max-w-3xl w-full max-h-[90vh] overflow-y-auto relative"
            onClick={(e) => e.stopPropagation()}
          >
            <CardHeader className="relative">
              <Button
                variant="ghost"
                size="icon"
                className="absolute right-4 top-4"
                onClick={() => setSelectedOrder(null)}
              >
                <X className="h-4 w-4" />
              </Button>
              <div className="flex items-center justify-between pr-10">
                <div>
                  <CardTitle>Order Details</CardTitle>
                  <CardDescription>Order ID: {selectedOrder.id}</CardDescription>
                </div>
                <Badge variant={getStatusBadgeVariant(selectedOrder.status)}>
                  {getStatusDisplayName(selectedOrder.status)}
                </Badge>
              </div>
            </CardHeader>
            <CardContent className="space-y-6">
              {/* Customer Info */}
              <div>
                <h3 className="font-semibold mb-3">Customer Information</h3>
                <div className="grid grid-cols-2 gap-4 text-sm">
                  <div>
                    <p className="text-muted-foreground">Name</p>
                    <p className="font-medium">{selectedOrder.userFirstName} {selectedOrder.userLastName}</p>
                  </div>
                  <div>
                    <p className="text-muted-foreground">Email</p>
                    <p className="font-medium">{selectedOrder.userEmail}</p>
                  </div>
                </div>
              </div>

              {/* Addresses */}
              <div>
                <h3 className="font-semibold mb-3">Shipping & Billing</h3>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
                  <div>
                    <p className="text-muted-foreground mb-1">Shipping Address</p>
                    <p className="font-medium">{formatAddress(selectedOrder.shippingAddress)}</p>
                  </div>
                  <div>
                    <p className="text-muted-foreground mb-1">Billing Address</p>
                    <p className="font-medium">{formatAddress(selectedOrder.billingAddress)}</p>
                  </div>
                </div>
              </div>

              {/* Order Info */}
              <div>
                <h3 className="font-semibold mb-3">Order Information</h3>
                <div className="grid grid-cols-2 gap-4 text-sm">
                  <div>
                    <p className="text-muted-foreground">Created</p>
                    <p className="font-medium">{new Date(selectedOrder.createdAt).toLocaleString()}</p>
                  </div>
                  <div>
                    <p className="text-muted-foreground">Last Modified</p>
                    <p className="font-medium">{new Date(selectedOrder.modifiedAt).toLocaleString()}</p>
                  </div>
                  <div className="col-span-2">
                    <p className="text-muted-foreground">Total</p>
                    <p className="font-medium text-lg">
                      {selectedOrder.totalCurrency} {selectedOrder.totalAmount.toFixed(2)}
                    </p>
                  </div>
                </div>
              </div>

              {/* Order Items */}
              <div>
                <h3 className="font-semibold mb-3">Order Items</h3>
                <div className="space-y-2">
                  {selectedOrder.items.map((item) => (
                    <div
                      key={item.id}
                      className="flex justify-between items-center p-3 bg-muted rounded-md text-sm"
                    >
                      <div>
                        <p className="font-medium">{item.productName}</p>
                        <p className="text-muted-foreground">
                          Quantity: {item.quantity} × {item.unitPriceCurrency} {item.unitPriceAmount.toFixed(2)}
                        </p>
                      </div>
                      <p className="font-medium">
                        {item.totalPriceCurrency} {item.totalPriceAmount.toFixed(2)}
                      </p>
                    </div>
                  ))}
                </div>
              </div>

              {/* Status Update */}
              <div>
                <h3 className="font-semibold mb-3">Update Status</h3>
                <div className="grid grid-cols-2 sm:grid-cols-4 gap-2">
                  {Object.keys(OrderStatus)
                    .filter((key) => !isNaN(Number(OrderStatus[key as keyof typeof OrderStatus])))
                    .map((statusName) => {
                      const statusValue = OrderStatus[statusName as keyof typeof OrderStatus] as OrderStatus;
                      return (
                        <Button
                          key={statusName}
                          variant={selectedOrder.status === statusValue ? 'default' : 'outline'}
                          size="sm"
                          onClick={() => handleStatusChange(selectedOrder.id, statusValue)}
                          disabled={updating || selectedOrder.status === statusValue}
                        >
                          {statusName}
                        </Button>
                      );
                    })}
                </div>
              </div>
            </CardContent>
          </Card>
        </div>
      )}
    </div>
  );
}
