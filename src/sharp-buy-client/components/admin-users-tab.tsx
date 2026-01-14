'use client';

import { useState, useEffect } from 'react';
import { api, ApiError, UserListItem } from '@/lib/api';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Users, Shield, AlertCircle, CheckCircle, UserPlus, UserMinus } from 'lucide-react';
import { RoleManagementDialog } from '@/components/role-management-dialog';

const AVAILABLE_ROLES = ['Admin', 'SalesManager', 'Salesman', 'Client'];

export function AdminUsersTab() {
  const [users, setUsers] = useState<UserListItem[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [selectedUser, setSelectedUser] = useState<UserListItem | null>(null);
  const [showRoleDialog, setShowRoleDialog] = useState(false);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  useEffect(() => {
    loadUsers();
  }, [page]);

  const loadUsers = async () => {
    setIsLoading(true);
    setError('');

    try {
      const data = await api.getAllUsers(page, 20);
      setUsers(data.items);
      setTotalPages(data.totalPages);
    } catch (err) {
      const apiError = err as ApiError;
      setError(apiError.detail || apiError.title || 'Failed to load users.');
    } finally {
      setIsLoading(false);
    }
  };

  const handleAddRole = async (userId: string, roleName: string) => {
    setError('');
    setSuccess('');

    try {
      await api.addRoleToUser(userId, roleName);
      setSuccess(`Role "${roleName}" added successfully!`);
      await loadUsers();
      setShowRoleDialog(false);
      setSelectedUser(null);
    } catch (err) {
      const apiError = err as ApiError;
      setError(apiError.detail || apiError.title || 'Failed to add role.');
    }
  };

  const handleRemoveRole = async (userId: string, roleName: string) => {
    if (!confirm(`Are you sure you want to remove the "${roleName}" role from this user?`)) {
      return;
    }

    setError('');
    setSuccess('');

    try {
      await api.removeRoleFromUser(userId, roleName);
      setSuccess(`Role "${roleName}" removed successfully!`);
      await loadUsers();
    } catch (err) {
      const apiError = err as ApiError;
      setError(apiError.detail || apiError.title || 'Failed to remove role.');
    }
  };

  const getRoleColor = (role: string) => {
    switch (role) {
      case 'Admin':
        return 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200';
      case 'SalesManager':
        return 'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-200';
      case 'Salesman':
        return 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200';
      case 'Client':
        return 'bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-200';
      default:
        return 'bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-200';
    }
  };

  return (
    <div className="space-y-4">
      {/* Success/Error Messages */}
      {success && (
        <div className="flex items-center gap-2 p-3 text-sm text-green-600 bg-green-50 dark:bg-green-950 rounded-md">
          <CheckCircle className="h-4 w-4" />
          <span>{success}</span>
        </div>
      )}

      {error && (
        <div className="flex items-center gap-2 p-3 text-sm text-destructive bg-destructive/10 rounded-md">
          <AlertCircle className="h-4 w-4" />
          <span>{error}</span>
        </div>
      )}

      <Card>
        <CardHeader>
          <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
            <div className="space-y-1">
              <CardTitle className="flex items-center gap-2">
                <Users className="h-5 w-5" />
                User Management
              </CardTitle>
              <CardDescription>Manage user roles and permissions</CardDescription>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <div className="text-center py-12 text-muted-foreground">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto mb-4"></div>
              <p>Loading users...</p>
            </div>
          ) : users.length === 0 ? (
            <div className="text-center py-12 text-muted-foreground">
              <Users className="h-12 w-12 mx-auto mb-4 opacity-50" />
              <p>No users found.</p>
            </div>
          ) : (
            <div className="space-y-3">
              {users.map(user => (
                <div key={user.id} className="flex flex-col sm:flex-row items-start sm:items-center justify-between p-4 border rounded-lg bg-card gap-4">
                  <div className="flex-1 w-full sm:w-auto">
                    <div className="flex items-center gap-2 mb-1">
                      <h4 className="font-semibold">{user.firstName} {user.lastName}</h4>
                      {user.emailConfirmed && (
                        <CheckCircle className="h-4 w-4 text-green-600" title="Email verified" />
                      )}
                    </div>
                    <p className="text-sm text-muted-foreground mb-2">{user.email}</p>
                    {user.phoneNumber && (
                      <p className="text-xs text-muted-foreground mb-2">{user.phoneNumber}</p>
                    )}

                    {/* Roles */}
                    <div className="flex flex-wrap gap-1 mt-2">
                      {user.roles.length === 0 ? (
                        <span className="text-xs text-muted-foreground">No roles assigned</span>
                      ) : (
                        user.roles.map(role => (
                          <div
                            key={role}
                            className={`flex items-center gap-1 px-2 py-1 rounded-md text-xs font-medium ${getRoleColor(role)}`}
                          >
                            <Shield className="h-3 w-3" />
                            <span>{role}</span>
                            <button
                              onClick={() => handleRemoveRole(user.id, role)}
                              className="ml-1 hover:opacity-70 transition-opacity"
                              title={`Remove ${role} role`}
                            >
                              <UserMinus className="h-3 w-3" />
                            </button>
                          </div>
                        ))
                      )}
                    </div>
                  </div>

                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => {
                      setSelectedUser(user);
                      setShowRoleDialog(true);
                      setError('');
                      setSuccess('');
                    }}
                    className="w-full sm:w-auto"
                  >
                    <UserPlus className="h-4 w-4 mr-2" />
                    Add Role
                  </Button>
                </div>
              ))}
            </div>
          )}

          {/* Pagination */}
          {totalPages > 1 && (
            <div className="flex items-center justify-center gap-2 mt-6">
              <Button
                variant="outline"
                size="sm"
                onClick={() => setPage(p => Math.max(1, p - 1))}
                disabled={page === 1 || isLoading}
              >
                Previous
              </Button>
              <span className="text-sm text-muted-foreground">
                Page {page} of {totalPages}
              </span>
              <Button
                variant="outline"
                size="sm"
                onClick={() => setPage(p => Math.min(totalPages, p + 1))}
                disabled={page === totalPages || isLoading}
              >
                Next
              </Button>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Role Management Dialog */}
      {selectedUser && (
        <RoleManagementDialog
          open={showRoleDialog}
          onOpenChange={setShowRoleDialog}
          user={selectedUser}
          availableRoles={AVAILABLE_ROLES}
          onAddRole={handleAddRole}
        />
      )}
    </div>
  );
}
