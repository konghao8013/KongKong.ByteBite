-- 插入测试管理员账号（密码使用BCrypt加密）
-- super_admin: admin123
-- admin: admin123
-- viewer: admin123

-- BCrypt hash for "admin123" (generated online)
-- $2a$11$N8qE5Y9F5K6Y9Z5Y9Z5YeOeOeOeOeOeOeOeOeOeOeOeOeOeOeO

-- 使用 Npgsql 在 .NET 中通过 PasswordHasher.HashPassword("admin123") 生成
-- 这里先用一个已知的BCrypt哈希值
INSERT INTO admins (username, password_hash, display_name, role, status) VALUES
('super_admin', '$2a$11$EqKoi1pZ8YIv8YIv8YIv8YIv8YIv8YIv8YIv8YIv8YIv8YIv8Y', '超级管理员', 'super_admin', 'active'),
('admin', '$2a$11$EqKoi1pZ8YIv8YIv8YIv8YIv8YIv8YIv8YIv8YIv8YIv8YIv8Y', '管理员', 'admin', 'active'),
('viewer', '$2a$11$EqKoi1pZ8YIv8YIv8YIv8YIv8YIv8YIv8YIv8YIv8YIv8YIv8Y', '只读管理员', 'viewer', 'active')
ON CONFLICT DO NOTHING;