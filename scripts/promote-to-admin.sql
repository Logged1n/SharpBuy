-- Promote a user to Admin role (PostgreSQL)
-- Replace 'your-email@example.com' with your actual email

-- Remove existing Client role and add Admin role
WITH user_info AS (
    SELECT id FROM asp_net_users WHERE email = 'your-email@example.com' LIMIT 1
),
admin_role AS (
    SELECT id FROM asp_net_roles WHERE name = 'Admin' LIMIT 1
)
DELETE FROM asp_net_user_roles
WHERE user_id = (SELECT id FROM user_info);

-- Add Admin role
WITH user_info AS (
    SELECT id FROM asp_net_users WHERE email = 'your-email@example.com' LIMIT 1
),
admin_role AS (
    SELECT id FROM asp_net_roles WHERE name = 'Admin' LIMIT 1
)
INSERT INTO asp_net_user_roles (user_id, role_id)
SELECT user_info.id, admin_role.id
FROM user_info, admin_role;

-- Verify
SELECT u.email, r.name as role
FROM asp_net_users u
JOIN asp_net_user_roles ur ON u.id = ur.user_id
JOIN asp_net_roles r ON ur.role_id = r.id
WHERE u.email = 'your-email@example.com';
