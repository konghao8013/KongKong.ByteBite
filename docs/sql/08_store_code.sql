-- =============================================
-- 迁移脚本：添加 store_code 字段 + 初始化店铺码
-- 日期：2026-05-25
-- 执行方式：psql -h 192.168.3.22 -U konghao -d kongkong_bytebite -f 08_store_code.sql
-- =============================================

-- Step 1: 添加 store_code 列
ALTER TABLE stores ADD COLUMN IF NOT EXISTS store_code VARCHAR(6);

-- Step 2: 为现有店铺生成 6 位 Base36 店铺码
-- 字符集: 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ (Base36)
-- 编码规则: 按 created_at 排序，第1个店铺=000001, 第2个=000002, 第36个=000010
DO $$
DECLARE
    rec RECORD;
    row_num INTEGER;
    val BIGINT;
    chars CONSTANT TEXT := '0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ';
    code TEXT;
    digit INTEGER;
BEGIN
    row_num := 0;
    FOR rec IN SELECT id FROM stores WHERE store_code IS NULL ORDER BY created_at LOOP
        row_num := row_num + 1;
        val := row_num;
        code := '';
        FOR i IN 1..6 LOOP
            digit := val % 36;
            code := SUBSTRING(chars FROM digit + 1 FOR 1) || code;
            val := val / 36;
        END LOOP;
        UPDATE stores SET store_code = code WHERE id = rec.id;
        RAISE NOTICE 'Store % → store_code: %', rec.id, code;
    END LOOP;
END $$;

-- Step 3: 添加 NOT NULL + UNIQUE 约束
ALTER TABLE stores ALTER COLUMN store_code SET NOT NULL;
CREATE UNIQUE INDEX IF NOT EXISTS idx_stores_store_code ON stores(store_code);

-- 验证
SELECT id, name, store_code, '/A/' || store_code AS share_url FROM stores ORDER BY created_at;
