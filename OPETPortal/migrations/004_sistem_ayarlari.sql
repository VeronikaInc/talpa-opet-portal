-- Migration 004: Sistem Ayarları
-- Date: 2026-03-15

CREATE TABLE IF NOT EXISTS sistem_ayarlari (
    anahtar    VARCHAR(50) PRIMARY KEY,
    deger      TEXT        NOT NULL,
    guncelleme TIMESTAMP   DEFAULT NOW()
);

INSERT INTO sistem_ayarlari (anahtar, deger) VALUES
    ('basvuru_aktif',  'true'),
    ('max_kod_limiti', '0')
ON CONFLICT (anahtar) DO NOTHING;
