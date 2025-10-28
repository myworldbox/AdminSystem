USE 客戶資料;
GO

-- Add Soft Delete Columns
ALTER TABLE dbo.客戶資料
ADD 是否已刪除 bit NOT NULL DEFAULT 0;

ALTER TABLE dbo.客戶聯絡人
ADD 是否已刪除 bit NOT NULL DEFAULT 0;

ALTER TABLE dbo.客戶銀行資訊
ADD 是否已刪除 bit NOT NULL DEFAULT 0;
GO

-- Add Customer Classification Column and Constraint
ALTER TABLE dbo.客戶資料
ADD 客戶分類 nvarchar(50) NULL;

ALTER TABLE dbo.客戶資料
ADD CONSTRAINT CK_客戶分類 CHECK (客戶分類 IN (N'一般', N'VIP', N'黑名單', N'合作夥伴'));
GO

-- Create View
CREATE VIEW dbo.vw_CustomerSummary
AS
SELECT 
    c.Id,
    c.客戶名稱,
    (SELECT COUNT(*) 
     FROM dbo.客戶聯絡人 
     WHERE 客戶Id = c.Id AND 是否已刪除 = 0) AS 聯絡人數量,
    (SELECT COUNT(*) 
     FROM dbo.客戶銀行資訊 
     WHERE 客戶Id = c.Id AND 是否已刪除 = 0) AS 銀行帳戶數量
FROM dbo.客戶資料 c
WHERE c.是否已刪除 = 0;
GO

-- Create Unique Index
CREATE UNIQUE NONCLUSTERED INDEX UX_聯絡人_客戶_Email
ON dbo.客戶聯絡人(客戶Id, Email)
WHERE Email IS NOT NULL AND 是否已刪除 = 0;
GO

-- Clean Existing Data
DELETE FROM dbo.客戶銀行資訊;
DELETE FROM dbo.客戶聯絡人;
DELETE FROM dbo.客戶資料;
GO

-- Reset Identity Seeds
DBCC CHECKIDENT (N'dbo.客戶資料', RESEED, 0);
DBCC CHECKIDENT (N'dbo.客戶聯絡人', RESEED, 0);
DBCC CHECKIDENT (N'dbo.客戶銀行資訊', RESEED, 0);
GO

-- Insert Mock Data into 客戶資料
INSERT INTO dbo.客戶資料 (客戶名稱, 統一編號, 電話, 傳真, 地址, Email, 客戶分類, 是否已刪除)
VALUES 
    (N'司馬伷', '12345678', '0211-123456', '87654321', N'台北市中正區1號', 'test1@example.com', N'VIP', 0),
    (N'公孫淵', '87654321', '0211-987654', NULL, N'台中市西區2號', NULL, N'一般', 0),
    (N'曹髦', '11223344', '0211-111654', NULL, NULL, 'test3@example.com', N'黑名單', 0),
    (N'馮劫', '22334455', '0211-222233', NULL, N'新北市板橋區3號', 'feng@example.com', N'一般', 0),
    (N'劉鏔', '33445566', '0211-333344', '44443333', N'台北市大安區4號', 'liubei@example.com', N'VIP', 0),
    (N'孫休', '44556677', '0711-555566', NULL, N'高雄市鼓山區5號', 'sunquan@example.com', N'一般', 0),
    (N'司馬懿', '55667788', '0311-222277', NULL, N'新北市新店區6號', NULL, N'黑名單', 0),
    (N'諸葛誕', '66778899', '0411-666677', NULL, N'台中市南區7號', 'zhugeliang@example.com', N'VIP', 0),
    (N'高演', '77889900', '0511-777788', '88887777', N'台南市中西區8號', 'guanyu@example.com', N'一般', 0),
    (N'史思明', '88990011', '0611-999900', NULL, N'新竹市東區9號', 'zhangfei@example.com', N'VIP', 0);
GO

-- Insert Mock Data into 客戶聯絡人
INSERT INTO dbo.客戶聯絡人 (客戶Id, 職稱, 姓名, Email, 手機, 電話, 是否已刪除)
VALUES 
    (1, N'經理', N'張三', 'zhang@example.com', '0912-345678', '0211-123456', 0),
    (2, N'助理', N'李四', 'li@example.com', '0922-345678', NULL, 0),
    (3, N'總監', N'王五', 'wang@example.com', '0933-111222', '0211-111654', 0),
    (4, N'專員', N'趙六', 'zhao@example.com', '0933-333444', '0211-222233', 0),
    (5, N'顧問', N'錢七', 'qian@example.com', '0933-555666', '0211-333344', 0),
    (6, N'經理', N'周八', 'zhou@example.com', '0933-777888', '0711-555566', 0),
    (7, N'助理', N'吳九', 'wu@example.com', '0933-999000', NULL, 0),
    (8, N'顧問', N'鄭十', 'zheng@example.com', '0933-222333', '0411-666677', 0),
    (9, N'經理', N'陳十一', 'chen@example.com', '0933-444555', '0511-777788', 0),
    (10, N'助理', N'林十二', 'lin@example.com', '0933-666777', '0611-999900', 0);
GO

-- Insert Mock Data into 客戶銀行資訊
INSERT INTO dbo.客戶銀行資訊 (客戶Id, 銀行名稱, 銀行代碼, 分行代碼, 帳戶名稱, 帳戶號碼, 是否已刪除)
VALUES 
    (1, N'台灣銀行', '004', '001', N'測試帳戶1', '123456789012', 0),
    (2, N'第一銀行', '007', '002', N'測試帳戶2', '987654321098', 0),
    (3, N'合作金庫', '006', '003', N'測試帳戶3', '111122223333', 0),
    (4, N'華南銀行', '008', '004', N'測試帳戶4', '444455556666', 0),
    (5, N'彰化銀行', '009', '005', N'測試帳戶5', '777788889999', 0),
    (6, N'台新銀行', '812', '006', N'測試帳戶6', '000011112222', 0),
    (7, N'中國信託', '822', '007', N'測試帳戶7', '333344445555', 0),
    (8, N'玉山銀行', '808', '008', N'測試帳戶8', '666677778888', 0),
    (9, N'國泰世華', '013', '009', N'測試帳戶9', '999900001111', 0),
    (10, N'兆豐銀行', '017', '010', N'測試帳戶10', '222233334444', 0);
GO