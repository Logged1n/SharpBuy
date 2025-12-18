'use client';

import { useState } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { Input } from '@/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  api,
  Granularity,
  SalesAnalyticsResponse,
  ProductAnalyticsResponse,
  CustomerAnalyticsResponse,
  OrderAnalyticsResponse,
  PdfJobResponse,
  PdfJobStatus
} from '@/lib/api';
import { Download, TrendingUp, Package, Users, ShoppingCart, Calendar } from 'lucide-react';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';

type ReportType = 'sales' | 'products' | 'customers' | 'orders';

const PRESET_RANGES = {
  '7days': { label: 'Last 7 Days', days: 7 },
  '30days': { label: 'Last 30 Days', days: 30 },
  '90days': { label: 'Last 90 Days', days: 90 },
  'thisMonth': { label: 'This Month', days: 0 }, // special handling
  'lastMonth': { label: 'Last Month', days: 0 }, // special handling
  'thisYear': { label: 'This Year', days: 0 }, // special handling
};

export function AdminAnalyticsTab() {
  const [reportType, setReportType] = useState<ReportType>('sales');
  const [granularity, setGranularity] = useState<Granularity>('Day');
  const [preset, setPreset] = useState('30days');
  const [customStartDate, setCustomStartDate] = useState('');
  const [customEndDate, setCustomEndDate] = useState('');
  const [useCustomRange, setUseCustomRange] = useState(false);

  const [salesData, setSalesData] = useState<SalesAnalyticsResponse | null>(null);
  const [productData, setProductData] = useState<ProductAnalyticsResponse | null>(null);
  const [customerData, setCustomerData] = useState<CustomerAnalyticsResponse | null>(null);
  const [orderData, setOrderData] = useState<OrderAnalyticsResponse | null>(null);

  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');
  const [isDownloading, setIsDownloading] = useState(false);
  const [pdfJobStatus, setPdfJobStatus] = useState<PdfJobStatus | null>(null);

  const getDateRange = (): { startDate: string; endDate: string } => {
    if (useCustomRange) {
      return {
        startDate: new Date(customStartDate).toISOString(),
        endDate: new Date(customEndDate).toISOString(),
      };
    }

    const end = new Date();
    let start = new Date();

    if (preset === 'thisMonth') {
      start = new Date(end.getFullYear(), end.getMonth(), 1);
    } else if (preset === 'lastMonth') {
      start = new Date(end.getFullYear(), end.getMonth() - 1, 1);
      end.setDate(0); // Last day of previous month
    } else if (preset === 'thisYear') {
      start = new Date(end.getFullYear(), 0, 1);
    } else {
      const days = PRESET_RANGES[preset as keyof typeof PRESET_RANGES].days;
      start.setDate(end.getDate() - days);
    }

    return {
      startDate: start.toISOString(),
      endDate: end.toISOString(),
    };
  };

  const loadAnalytics = async () => {
    setIsLoading(true);
    setError('');

    try {
      const { startDate, endDate } = getDateRange();
      const request = { startDate, endDate, granularity };

      switch (reportType) {
        case 'sales':
          const sales = await api.getSalesAnalytics(request);
          setSalesData(sales);
          break;
        case 'products':
          const products = await api.getProductAnalytics(request);
          setProductData(products);
          break;
        case 'customers':
          const customers = await api.getCustomerAnalytics(request);
          setCustomerData(customers);
          break;
        case 'orders':
          const orders = await api.getOrderAnalytics(request);
          setOrderData(orders);
          break;
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load analytics');
    } finally {
      setIsLoading(false);
    }
  };

  const downloadReport = async () => {
    setIsDownloading(true);
    setError('');
    setPdfJobStatus(null);

    try {
      const { startDate, endDate } = getDateRange();
      const request = { startDate, endDate, granularity };

      let jobResponse: PdfJobResponse;
      let filename: string;

      switch (reportType) {
        case 'sales':
          jobResponse = await api.downloadSalesReport(request);
          filename = `sales-report-${new Date().toISOString().split('T')[0]}.pdf`;
          break;
        case 'products':
          jobResponse = await api.downloadProductReport(request);
          filename = `product-report-${new Date().toISOString().split('T')[0]}.pdf`;
          break;
        case 'customers':
          jobResponse = await api.downloadCustomerReport(request);
          filename = `customer-report-${new Date().toISOString().split('T')[0]}.pdf`;
          break;
        case 'orders':
          jobResponse = await api.downloadOrderReport(request);
          filename = `order-report-${new Date().toISOString().split('T')[0]}.pdf`;
          break;
        default:
          return;
      }

      const finalStatus = await api.pollPdfJobUntilComplete(
        jobResponse.jobId,
        (status) => setPdfJobStatus(status)
      );

      if (finalStatus.state === 'Failed') {
        throw new Error(finalStatus.errorMessage || 'PDF generation failed');
      }

      if (finalStatus.state === 'Completed' && finalStatus.pdfCacheKey) {
        const blob = await api.downloadPdf(finalStatus.pdfCacheKey);

        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        document.body.removeChild(a);

        setPdfJobStatus(null);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to download report');
    } finally {
      setIsDownloading(false);
    }
  };

  return (
    <div className="space-y-6">
      {/* Filters */}
      <Card>
        <CardHeader>
          <CardTitle>Analytics Filters</CardTitle>
          <CardDescription>Select report type, date range, and granularity</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            {/* Report Type */}
            <div className="space-y-2">
              <Label htmlFor="reportType">Report Type</Label>
              <Select value={reportType} onValueChange={(value) => setReportType(value as ReportType)}>
                <SelectTrigger id="reportType">
                  <SelectValue placeholder="Select report type" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="sales">
                    <div className="flex items-center gap-2">
                      <TrendingUp className="h-4 w-4" />
                      Sales Analytics
                    </div>
                  </SelectItem>
                  <SelectItem value="products">
                    <div className="flex items-center gap-2">
                      <Package className="h-4 w-4" />
                      Product Analytics
                    </div>
                  </SelectItem>
                  <SelectItem value="customers">
                    <div className="flex items-center gap-2">
                      <Users className="h-4 w-4" />
                      Customer Analytics
                    </div>
                  </SelectItem>
                  <SelectItem value="orders">
                    <div className="flex items-center gap-2">
                      <ShoppingCart className="h-4 w-4" />
                      Order Analytics
                    </div>
                  </SelectItem>
                </SelectContent>
              </Select>
            </div>

            {/* Granularity */}
            <div className="space-y-2">
              <Label htmlFor="granularity">Granularity</Label>
              <Select value={granularity} onValueChange={(value) => setGranularity(value as Granularity)}>
                <SelectTrigger id="granularity">
                  <SelectValue placeholder="Select granularity" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="Day">Daily</SelectItem>
                  <SelectItem value="Week">Weekly</SelectItem>
                  <SelectItem value="Month">Monthly</SelectItem>
                </SelectContent>
              </Select>
            </div>

            {/* Date Range Preset */}
            <div className="space-y-2">
              <Label htmlFor="preset">Date Range</Label>
              <Select value={preset} onValueChange={(value) => { setPreset(value); setUseCustomRange(false); }}>
                <SelectTrigger id="preset">
                  <SelectValue placeholder="Select range" />
                </SelectTrigger>
                <SelectContent>
                  {Object.entries(PRESET_RANGES).map(([key, { label }]) => (
                    <SelectItem key={key} value={key}>{label}</SelectItem>
                  ))}
                  <SelectItem value="custom">Custom Range</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>

          {/* Custom Date Range */}
          {(preset === 'custom' || useCustomRange) && (
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 pt-2">
              <div className="space-y-2">
                <Label htmlFor="startDate">Start Date</Label>
                <Input
                  id="startDate"
                  type="date"
                  value={customStartDate}
                  onChange={(e) => { setCustomStartDate(e.target.value); setUseCustomRange(true); }}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="endDate">End Date</Label>
                <Input
                  id="endDate"
                  type="date"
                  value={customEndDate}
                  onChange={(e) => { setCustomEndDate(e.target.value); setUseCustomRange(true); }}
                />
              </div>
            </div>
          )}

          {/* Action Buttons */}
          <div className="flex gap-4 pt-4">
            <Button onClick={loadAnalytics} disabled={isLoading} className="flex-1">
              {isLoading ? 'Loading...' : 'Generate Report'}
            </Button>
            <Button
              onClick={downloadReport}
              disabled={isDownloading}
              variant="outline"
              className="flex items-center gap-2"
            >
              <Download className="h-4 w-4" />
              {isDownloading ? 'Downloading...' : 'Download PDF'}
            </Button>
          </div>

          {error && (
            <div className="text-sm text-red-600 bg-red-50 p-3 rounded-md">
              {error}
            </div>
          )}

          {pdfJobStatus && (
            <div className="text-sm bg-blue-50 p-3 rounded-md">
              <p className="font-medium">PDF Generation Status: {pdfJobStatus.state}</p>
              {pdfJobStatus.state === 'Pending' && (
                <p className="text-gray-600 mt-1">Your PDF report is queued for generation...</p>
              )}
              {pdfJobStatus.state === 'Processing' && (
                <p className="text-gray-600 mt-1">Generating PDF report, please wait...</p>
              )}
            </div>
          )}
        </CardContent>
      </Card>

      {/* Results */}
      {salesData && reportType === 'sales' && (
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <Card>
            <CardHeader className="pb-3">
              <CardDescription>Total Revenue</CardDescription>
              <CardTitle className="text-2xl">${salesData.totalRevenue.toFixed(2)}</CardTitle>
            </CardHeader>
          </Card>
          <Card>
            <CardHeader className="pb-3">
              <CardDescription>Total Orders</CardDescription>
              <CardTitle className="text-2xl">{salesData.totalOrders}</CardTitle>
            </CardHeader>
          </Card>
          <Card>
            <CardHeader className="pb-3">
              <CardDescription>Avg Order Value</CardDescription>
              <CardTitle className="text-2xl">${salesData.averageOrderValue.toFixed(2)}</CardTitle>
            </CardHeader>
          </Card>
          <Card>
            <CardHeader className="pb-3">
              <CardDescription>Growth</CardDescription>
              <CardTitle className={`text-2xl ${salesData.growthPercentage >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                {salesData.growthPercentage >= 0 ? '+' : ''}{salesData.growthPercentage.toFixed(2)}%
              </CardTitle>
            </CardHeader>
          </Card>
        </div>
      )}

      {productData && reportType === 'products' && (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Card>
            <CardHeader className="pb-3">
              <CardDescription>Total Products Sold</CardDescription>
              <CardTitle className="text-2xl">{productData.totalProductsSold}</CardTitle>
            </CardHeader>
          </Card>
          <Card>
            <CardHeader className="pb-3">
              <CardDescription>Total Revenue</CardDescription>
              <CardTitle className="text-2xl">${productData.totalRevenue.toFixed(2)}</CardTitle>
            </CardHeader>
          </Card>
        </div>
      )}

      {productData && reportType === 'products' && productData.topProducts.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>Top 10 Products</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="relative overflow-x-auto">
              <table className="w-full text-sm">
                <thead className="text-xs uppercase bg-gray-50">
                  <tr>
                    <th className="px-6 py-3 text-left">Rank</th>
                    <th className="px-6 py-3 text-left">Product</th>
                    <th className="px-6 py-3 text-right">Quantity Sold</th>
                    <th className="px-6 py-3 text-right">Revenue</th>
                  </tr>
                </thead>
                <tbody>
                  {productData.topProducts.map((product, index) => (
                    <tr key={product.productId} className="border-b hover:bg-gray-50">
                      <td className="px-6 py-4 font-medium">{index + 1}</td>
                      <td className="px-6 py-4">{product.productName}</td>
                      <td className="px-6 py-4 text-right">{product.quantitySold}</td>
                      <td className="px-6 py-4 text-right">${product.revenue.toFixed(2)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </CardContent>
        </Card>
      )}

      {customerData && reportType === 'customers' && (
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <Card>
            <CardHeader className="pb-3">
              <CardDescription>Total Customers</CardDescription>
              <CardTitle className="text-2xl">{customerData.totalCustomers}</CardTitle>
            </CardHeader>
          </Card>
          <Card>
            <CardHeader className="pb-3">
              <CardDescription>New Customers</CardDescription>
              <CardTitle className="text-2xl">{customerData.newCustomers}</CardTitle>
            </CardHeader>
          </Card>
          <Card>
            <CardHeader className="pb-3">
              <CardDescription>Returning Customers</CardDescription>
              <CardTitle className="text-2xl">{customerData.returningCustomers}</CardTitle>
            </CardHeader>
          </Card>
          <Card>
            <CardHeader className="pb-3">
              <CardDescription>Avg Customer Value</CardDescription>
              <CardTitle className="text-2xl">${customerData.averageCustomerValue.toFixed(2)}</CardTitle>
            </CardHeader>
          </Card>
        </div>
      )}

      {orderData && reportType === 'orders' && (
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <Card>
            <CardHeader className="pb-3">
              <CardDescription>Total Orders</CardDescription>
              <CardTitle className="text-2xl">{orderData.totalOrders}</CardTitle>
            </CardHeader>
          </Card>
          <Card>
            <CardHeader className="pb-3">
              <CardDescription>Completed</CardDescription>
              <CardTitle className="text-2xl text-green-600">{orderData.completedOrders}</CardTitle>
            </CardHeader>
          </Card>
          <Card>
            <CardHeader className="pb-3">
              <CardDescription>Pending</CardDescription>
              <CardTitle className="text-2xl text-yellow-600">{orderData.pendingOrders}</CardTitle>
            </CardHeader>
          </Card>
          <Card>
            <CardHeader className="pb-3">
              <CardDescription>Completion Rate</CardDescription>
              <CardTitle className="text-2xl">{orderData.completionRate.toFixed(2)}%</CardTitle>
            </CardHeader>
          </Card>
        </div>
      )}
    </div>
  );
}
