-- Migration 003: Admin Revamp
-- Date: 2026-03-15
-- Adds: kod_goruntulendi, goruntuleme_tarihi to uye_kodlar
--       test_kaydi flag to uye_kodlar
--       sorgu_log table

-- 1. uye_kodlar yeni kolonlar
ALTER TABLE uye_kodlar
    ADD COLUMN IF NOT EXISTS kod_goruntulendi   BOOLEAN   DEFAULT FALSE,
    ADD COLUMN IF NOT EXISTS goruntuleme_tarihi TIMESTAMP NULL,
    ADD COLUMN IF NOT EXISTS test_kaydi         BOOLEAN   DEFAULT FALSE;

-- 2. Sorgu log tablosu
CREATE TABLE IF NOT EXISTS sorgu_log (
    id            SERIAL PRIMARY KEY,
    tckn          VARCHAR(11)  NOT NULL,
    sorgu_tarihi  TIMESTAMP    DEFAULT NOW(),
    sonuc         VARCHAR(20)  NOT NULL CHECK (sonuc IN ('basarili', 'borclu', 'bulunamadi')),
    ip_adresi     VARCHAR(45)  NULL
);

CREATE INDEX IF NOT EXISTS idx_sorgu_log_tckn         ON sorgu_log (tckn);
CREATE INDEX IF NOT EXISTS idx_sorgu_log_sorgu_tarihi ON sorgu_log (sorgu_tarihi DESC);
