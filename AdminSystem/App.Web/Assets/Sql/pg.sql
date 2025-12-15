-- Use the database (equivalent to USE in SQL Server)
SET search_path TO public;

-- Add Soft Delete Columns
/*
ALTER TABLE public.客戶資料
ADD COLUMN 是否已刪除 boolean NOT NULL DEFAULT false;

ALTER TABLE public.客戶聯絡人
ADD COLUMN 是否已刪除 boolean NOT NULL DEFAULT false;

ALTER TABLE public.客戶銀行資訊
ADD COLUMN 是否已刪除 boolean NOT NULL DEFAULT false;
*/

-- Add Customer Classification Column and Constraint
/*
ALTER TABLE public.客戶資料
ADD COLUMN 客戶分類 varchar(50) NULL;
*/

-- Add CHECK constraint for 客戶分類
ALTER TABLE public.客戶資料
ADD CONSTRAINT ck_客戶分類
CHECK (客戶分類 IN ('一般', 'VIP', '黑名單', '合作夥伴'));

-- Create View
CREATE VIEW public."vw_CustomerSummary" AS
SELECT 
    c."Id",
    c.客戶名稱,
    (SELECT COUNT(*) 
     FROM public.客戶聯絡人 
     WHERE "客戶Id" = c."Id" AND 是否已刪除 = false) AS 聯絡人數量,
    (SELECT COUNT(*) 
     FROM public.客戶銀行資訊 
     WHERE "客戶Id" = c."Id" AND 是否已刪除 = false) AS 銀行帳戶數量
FROM public.客戶資料 c
WHERE c.是否已刪除 = false;

-- Create Unique Index
/*
CREATE UNIQUE INDEX ux_聯絡人_客戶_email
ON public.客戶聯絡人 (客戶Id, email)
WHERE "Email" IS NOT NULL AND 是否已刪除 = false;
*/

-- Clean Existing Data
DELETE FROM public.客戶銀行資訊;
DELETE FROM public.客戶聯絡人;
DELETE FROM public.客戶資料;

-- Reset Identity Seeds (assuming id columns use sequences)
ALTER SEQUENCE public."客戶資料_Id_seq" RESTART WITH 1;
ALTER SEQUENCE public."客戶聯絡人_Id_seq" RESTART WITH 1;
ALTER SEQUENCE public."客戶銀行資訊_Id_seq" RESTART WITH 1;

-- Insert Mock Data into 客戶資料
INSERT INTO public.客戶資料 (客戶名稱, 統一編號, 電話, 傳真, 地址, "Email", 客戶分類, 是否已刪除)
VALUES 
    ('司馬伷', '12345678', '0211-123456', '87654321', '台北市中正區1號', 'test1@example.com', 'VIP', FALSE),
    ('公孫淵', '87654321', '0211-987654', NULL, '台中市西區2號', NULL, '一般', FALSE),
    ('曹髦', '11223344', '0211-111654', NULL, NULL, 'test3@example.com', '黑名單', FALSE),
    ('馮劫', '22334455', '0211-222233', NULL, '新北市板橋區3號', 'feng@example.com', '一般', FALSE),
    ('劉鏔', '33445566', '0211-333344', '44443333', '台北市大安區4號', 'liubei@example.com', 'VIP', FALSE),
    ('孫休', '44556677', '0711-555566', NULL, '高雄市鼓山區5號', 'sunquan@example.com', '一般', FALSE),
    ('司馬懿', '55667788', '0311-222277', NULL, '新北市新店區6號', NULL, '黑名單', FALSE),
    ('諸葛誕', '66778899', '0411-666677', NULL, '台中市南區7號', 'zhugeliang@example.com', 'VIP', FALSE),
    ('高演', '77889900', '0511-777788', '88887777', '台南市中西區8號', 'guanyu@example.com', '一般', FALSE),
    ('史思明', '88990011', '0611-999900', NULL, '新竹市東區9號', 'zhangfei@example.com', 'VIP', FALSE);

-- Insert Mock Data into 客戶聯絡人
INSERT INTO public.客戶聯絡人 ("客戶Id", 職稱, 姓名, "Email", 手機, 電話, 是否已刪除)
VALUES 
    (1, '經理', '張三', 'zhang@example.com', '0912-345678', '0211-123456', FALSE),
    (2, '助理', '李四', 'li@example.com', '0922-345678', NULL, FALSE),
    (3, '總監', '王五', 'wang@example.com', '0933-111222', '0211-111654', FALSE),
    (4, '專員', '趙六', 'zhao@example.com', '0933-333444', '0211-222233', FALSE),
    (5, '顧問', '錢七', 'qian@example.com', '0933-555666', '0211-333344', FALSE),
    (6, '經理', '周八', 'zhou@example.com', '0933-777888', '0711-555566', FALSE),
    (7, '助理', '吳九', 'wu@example.com', '0933-999000', NULL, FALSE),
    (8, '顧問', '鄭十', 'zheng@example.com', '0933-222333', '0411-666677', FALSE),
    (9, '經理', '陳十一', 'chen@example.com', '0933-444555', '0511-777788', FALSE),
    (10, '助理', '林十二', 'lin@example.com', '0933-666777', '0611-999900', FALSE);

-- Insert Mock Data into 客戶銀行資訊
INSERT INTO public.客戶銀行資訊 ("客戶Id", 銀行名稱, 銀行代碼, 分行代碼, 帳戶名稱, 帳戶號碼, 是否已刪除)
VALUES 
    (1, '台灣銀行', '004', '001', '測試帳戶1', '123456789012', FALSE),
    (2, '第一銀行', '007', '002', '測試帳戶2', '987654321098', FALSE),
    (3, '合作金庫', '006', '003', '測試帳戶3', '111122223333', FALSE),
    (4, '華南銀行', '008', '004', '測試帳戶4', '444455556666', FALSE),
    (5, '彰化銀行', '009', '005', '測試帳戶5', '777788889999', FALSE),
    (6, '台新銀行', '812', '006', '測試帳戶6', '000011112222', FALSE),
    (7, '中國信託', '822', '007', '測試帳戶7', '333344445555', FALSE),
    (8, '玉山銀行', '808', '008', '測試帳戶8', '666677778888', FALSE),
    (9, '國泰世華', '013', '009', '測試帳戶9', '999900001111', FALSE),
    (10, '兆豐銀行', '017', '010', '測試帳戶10', '222233334444', FALSE);