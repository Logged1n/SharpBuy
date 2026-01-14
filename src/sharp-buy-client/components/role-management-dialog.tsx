'use client';

import { useState } from 'react';
import { UserListItem } from '@/lib/api';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Shield } from 'lucide-react';

interface RoleManagementDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  user: UserListItem;
  availableRoles: string[];
  onAddRole: (userId: string, roleName: string) => Promise<void>;
}

export function RoleManagementDialog({
  open,
  onOpenChange,
  user,
  availableRoles,
  onAddRole,
}: RoleManagementDialogProps) {
  const [selectedRole, setSelectedRole] = useState<string>('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedRole) return;

    setIsSubmitting(true);
    try {
      await onAddRole(user.id, selectedRole);
      setSelectedRole('');
    } finally {
      setIsSubmitting(false);
    }
  };

  // Filter out roles the user already has
  const assignableRoles = availableRoles.filter(role => !user.roles.includes(role));

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[425px]">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <Shield className="h-5 w-5" />
            Add Role
          </DialogTitle>
          <DialogDescription>
            Add a new role to {user.firstName} {user.lastName} ({user.email})
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit}>
          <div className="space-y-4 py-4">
            {/* Current Roles Display */}
            <div className="space-y-2">
              <Label className="text-sm font-medium">Current Roles</Label>
              {user.roles.length === 0 ? (
                <p className="text-sm text-muted-foreground">No roles assigned yet</p>
              ) : (
                <div className="flex flex-wrap gap-2">
                  {user.roles.map(role => (
                    <span
                      key={role}
                      className="px-2 py-1 text-xs font-medium rounded-md bg-primary/10 text-primary"
                    >
                      {role}
                    </span>
                  ))}
                </div>
              )}
            </div>

            {/* Role Selection */}
            <div className="space-y-2">
              <Label htmlFor="role">Select Role to Add</Label>
              {assignableRoles.length === 0 ? (
                <p className="text-sm text-muted-foreground">
                  User already has all available roles
                </p>
              ) : (
                <Select value={selectedRole} onValueChange={setSelectedRole}>
                  <SelectTrigger id="role" className="w-full">
                    <SelectValue placeholder="Choose a role" />
                  </SelectTrigger>
                  <SelectContent>
                    {assignableRoles.map(role => (
                      <SelectItem key={role} value={role}>
                        {role}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              )}
            </div>

            {/* Role Descriptions */}
            {selectedRole && (
              <div className="p-3 bg-muted rounded-md space-y-1">
                <p className="text-sm font-medium">{selectedRole} Role</p>
                <p className="text-xs text-muted-foreground">
                  {selectedRole === 'Admin' && 'Full system access including user management and all administrative functions.'}
                  {selectedRole === 'SalesManager' && 'Manage products, categories, and view all orders.'}
                  {selectedRole === 'Salesman' && 'Add and update products and categories.'}
                  {selectedRole === 'Client' && 'Standard user access for shopping and orders.'}
                </p>
              </div>
            )}
          </div>

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => {
                onOpenChange(false);
                setSelectedRole('');
              }}
              disabled={isSubmitting}
            >
              Cancel
            </Button>
            <Button
              type="submit"
              disabled={!selectedRole || isSubmitting || assignableRoles.length === 0}
            >
              {isSubmitting ? 'Adding...' : 'Add Role'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
