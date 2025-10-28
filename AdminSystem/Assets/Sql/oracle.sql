-- ========================================
-- Oracle: Setup Customer Data (English Cols)
-- ========================================

-- Clean existing data (safe delete)
DELETE FROM CUSTOMER_BANK_INFOS;
DELETE FROM CUSTOMER_CONTACTS;
DELETE FROM CUSTOMERS;

-- Reset identity (via sequence restart - assuming EF Core used IDENTITY)
-- Find sequence names (usually table_name + _SEQ)
-- Or drop/recreate tables if needed

-- Optional: Drop tables if recreating
/*
DROP TABLE CUSTOMER_BANK_INFOS CASCADE CONSTRAINTS;
DROP TABLE CUSTOMER_CONTACTS CASCADE CONSTRAINTS;
DROP TABLE CUSTOMERS CASCADE CONSTRAINTS;
DROP VIEW VW_CUSTOMERSUMMARY;
*/

-- Add Soft Delete & Classification (if tables exist)
-- Skip if using EF Core migrations (already created)

-- Add 客戶分類 (CATEGORY) and CHECK constraint
BEGIN
   EXECUTE IMMEDIATE 'ALTER TABLE CUSTOMERS ADD CATEGORY NVARCHAR2(50)';
EXCEPTION WHEN OTHERS THEN IF SQLCODE != -1430 THEN RAISE; END IF; -- Column exists
END;
/

BEGIN
   EXECUTE IMMEDIATE 'ALTER TABLE CUSTOMERS ADD CONSTRAINT CK_CATEGORY CHECK (CATEGORY IN (''一般'', ''VIP'', ''黑名單'', ''合作夥伴''))';
EXCEPTION WHEN OTHERS THEN IF SQLCODE != -2264 THEN RAISE; END IF; -- Constraint exists
END;
/

-- Create View: VW_CUSTOMERSUMMARY
CREATE OR REPLACE VIEW VW_CUSTOMERSUMMARY AS
SELECT 
    c.ID,
    c.NAME AS 客戶名稱,
    (SELECT COUNT(*) 
     FROM CUSTOMER_CONTACTS cc 
     WHERE cc.CUSTOMER_ID = c.ID AND cc.IS_DELETED = 0) AS 聯絡人數量,
    (SELECT COUNT(*) 
     FROM CUSTOMER_BANK_INFOS cb 
     WHERE cb.CUSTOMER_ID = c.ID AND cb.IS_DELETED = 0) AS 銀行帳戶數量
FROM CUSTOMERS c
WHERE c.IS_DELETED = 0;
/

-- Create Unique Index: Email per customer (non-deleted only)
-- Oracle: Use function-based index
BEGIN
   EXECUTE IMMEDIATE 'DROP INDEX UX_CONTACT_EMAIL';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/

CREATE UNIQUE INDEX UX_CONTACT_EMAIL
ON CUSTOMER_CONTACTS (
    CUSTOMER_ID,
    CASE WHEN EMAIL IS NOT NULL AND IS_DELETED = 0 THEN EMAIL ELSE NULL END
);
/

-- Insert Mock Data into CUSTOMERS
INSERT INTO CUSTOMERS (NAME, TAX_ID, PHONE, FAX, ADDRESS, EMAIL, CATEGORY, IS_DELETED) VALUES
    ('司馬伷', '12345678', '0211-123456', '87654321', '台北市中正區1號', 'test1@example.com', 'VIP', 0),
    ('公孫淵', '87654321', '0211-987654', NULL, '台中市西區2號', NULL, '一般', 0),
    ('曹髦', '11223344', '0211-111654', NULL, NULL, 'test3@example.com', '黑名單', 0),
    ('馮劫', '22334455', '0211-222233', NULL, '新北市板橋區3號', 'feng@example.com', '一般', 0),
    ('劉鏔', '33445566', '0211-333344', '44443333', '台北市大安區4號', 'liubei@example.com', 'VIP', 0),
    ('孫休', '44556677', '0711-555566', NULL, '高雄市鼓山區5號', 'sunquan@example.com', '一般', 0),
    ('司馬懿', '55667788', '0311-222277', NULL, '新北市新店區6號', NULL, '黑名單', 0),
    ('諸葛誕', '66778899', '0411-666677', NULL, '台中市南區7號', 'zhugeliang@example.com', 'VIP', 0),
    ('高演', '77889900', '0511-777788', '88887777', '台南市中西區8號', 'guanyu@example.com', '一般', 0),
    ('史思明', '88990011', '0611-999900', NULL, '新竹市東區9號', 'zhangfei@example.com', 'VIP', 0);
/

-- Insert Mock Data into CUSTOMER_CONTACTS
INSERT INTO CUSTOMER_CONTACTS (CUSTOMER_ID, TITLE, NAME, EMAIL, MOBILE, PHONE, IS_DELETED) VALUES
    (1, '經理', '張三', 'zhang@example.com', '0912-345678', '0211-123456', 0),
    (2, '助理', '李四', 'li@example.com', '0922-345678', NULL, 0),
    (3, '總監', '王五', 'wang@example.com', '0933-111222', '0211-111654', 0),
    (4, '專員', '趙六', 'zhao@example.com', '0933-333444', '0211-222233', 0),
    (5, '顧問', '錢七', 'qian@example.com', '0933-555666', '0211-333344', 0),
    (6, '經理', '周八', 'zhou@example.com', '0933-777888', '0711-555566', 0),
    (7, '助理', '吳九', 'wu@example.com', '0933-999000', NULL, 0),
    (8, '顧問', '鄭十', 'zheng@example.com', '0933-222333', '0411-666677', 0),
    (9, '經理', '陳十一', 'chen@example.com', '0933-444555', '0511-777788', 0),
    (10, '助理', '林十二', 'lin@example.com', '0933-666777', '0611-999900', 0);
/

-- Insert Mock Data into CUSTOMER_BANK_INFOS
INSERT INTO CUSTOMER_BANK_INFOS (CUSTOMER_ID, BANK_NAME, BANK_CODE, BRANCH_CODE, ACCOUNT_NAME, ACCOUNT_NUMBER, IS_DELETED) VALUES
    (1, '台灣銀行', '004', '001', '測試帳戶1', '123456789012', 0),
    (2, '第一銀行', '007', '002', '測試帳戶2', '987654321098', 0),
    (3, '合作金庫', '006', '003', '測試帳戶3', '111122223333', 0),
    (4, '華南銀行', '008', '004', '測試帳戶4', '444455556666', 0),
    (5, '彰化銀行', '009', '005', '測試帳戶5', '777788889999', 0),
    (6, '台新銀行', '812', '006', '測試帳戶6', '000011112222', 0),
    (7, '中國信託', '822', '007', '測試帳戶7', '333344445555', 0),
    (8, '玉山銀行', '808', '008', '測試帳戶8', '666677778888', 0),
    (9, '國泰世華', '013', '009', '測試帳戶9', '999900001111', 0),
    (10, '兆豐銀行', '017', '010', '測試帳戶10', '222233334444', 0);
/

COMMIT;