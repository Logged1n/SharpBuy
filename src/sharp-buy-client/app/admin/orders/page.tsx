'use client';

import { useEffect, useState } from 'react';
import { api, Order, OrderStatus } from '@/lib/api';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Loader2, Package, Eye } from 'lucide-react';
import { AdminRoute } from '@/components/admin-route';

function OrdersContent() {
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedOrder, setSelectedOrder] = useState<Order | null>(null);
  const [updating, setUpdating] = useState(false);

  useEffect(() => {
    loadOrders();
  }, []);

  const loadOrders = async () => {
    try {
      setLoading(true);
      const result = await api.getOrders(1, 50);
      setOrders(result.items);
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
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Orders Management</h1>
          <p className="text-muted-foreground mt-2">View and manage all customer orders</p>
        </div>
      </div>

      {orders.length === 0 ? (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-12">
            <Package className="h-12 w-12 text-muted-foreground mb-4" />
            <p className="text-muted-foreground">No orders found</p>
          </CardContent>
        </Card>
      ) : (
        <div className="grid gap-4">
          {orders.map((order) => (
            <Card key={order.id}>
              <CardHeader>
                <div className="flex items-center justify-between">
                  <div>
                    <CardTitle className="text-base">Order #{order.id.slice(0, 8)}</CardTitle>
                    <CardDescription>
                      {new Date(order.createdAt).toLocaleString()} • {order.items.length} items
                    </CardDescription>
                  </div>
                  <div className="flex items-center gap-2">
                    <Badge variant={getStatusBadgeVariant(order.status)}>
                      {getStatusDisplayName(order.status)}
                    </Badge>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => setSelectedOrder(order)}
                    >
                      <Eye className="h-4 w-4 mr-2" />
                      View
                    </Button>
                  </div>
                </div>
              </CardHeader>
              <CardContent>
                <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                  <div>
                    <p className="text-sm text-muted-foreground">Total Amount</p>
                    <p className="font-medium">
                      {order.totalCurrency} {order.totalAmount.toFixed(2)}
                    </p>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">User ID</p>
                    <p className="font-mono text-xs">{order.userId.slice(0, 16)}...</p>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">Last Modified</p>
                    <p className="text-sm">{new Date(order.modifiedAt).toLocaleString()}</p>
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}

      {/* Order Details Modal */}
      {selectedOrder && (
        <div
          className="fixed inset-0 bg-black/50 z-50 flex items-center justify-center p-4"
          onClick={() => setSelectedOrder(null)}
        >
          <Card
            className="max-w-3xl w-full max-h-[90vh] overflow-y-auto"
            onClick={(e) => e.stopPropagation()}
          >
            <CardHeader>
              <div className="flex items-center justify-between">
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
              {/* Order Info */}
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <p className="text-sm text-muted-foreground">Created</p>
                  <p className="font-medium">{new Date(selectedOrder.createdAt).toLocaleString()}</p>
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">Last Modified</p>
                  <p className="font-medium">{new Date(selectedOrder.modifiedAt).toLocaleString()}</p>
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">User ID</p>
                  <p className="font-mono text-sm">{selectedOrder.userId}</p>
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">Total</p>
                  <p className="font-medium text-lg">
                    {selectedOrder.totalCurrency} {selectedOrder.totalAmount.toFixed(2)}
                  </p>
                </div>
              </div>

              {/* Order Items */}
              <div>
                <h3 className="font-semibold mb-3">Order Items</h3>
                <div className="space-y-2">
                  {selectedOrder.items.map((item) => (
                    <div
                      key={item.id}
                      className="flex justify-between items-center p-3 bg-muted rounded-md"
                    >
                      <div>
                        <p className="font-medium">{item.productName}</p>
                        <p className="text-sm text-muted-foreground">
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

              {/* Close Button */}
              <Button
                variant="outline"
                className="w-full"
                onClick={() => setSelectedOrder(null)}
              >
                Close
              </Button>
            </CardContent>
          </Card>
        </div>
      )}
    </div>
  );
}

export default function OrdersManagementPage() {
  return (
    <AdminRoute>
      <div className="container mx-auto px-4 py-8">
        <OrdersContent />
      </div>
    </AdminRoute>
  );
}
