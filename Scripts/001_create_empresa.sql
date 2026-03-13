-- =============================================================
-- GestaoRH — Script inicial do banco de dados
-- Banco: GestaoRHDB  |  PostgreSQL
-- =============================================================

-- Tabela: empresa
-- Armazena o perfil da empresa que usa o sistema.
CREATE TABLE IF NOT EXISTS empresa (
    id                    SERIAL PRIMARY KEY,
    cnpj                  VARCHAR(18)  NOT NULL UNIQUE,
    razao_social          VARCHAR(200) NOT NULL,
    endereco              VARCHAR(300) NOT NULL DEFAULT '',
    telefone              VARCHAR(20)  NOT NULL DEFAULT '',
    logo_base64           TEXT,                            -- imagem em base64
    responsavel_nome      VARCHAR(100) NOT NULL,
    responsavel_sobrenome VARCHAR(100) NOT NULL,
    senha                 VARCHAR(255) NOT NULL,           -- hash BCrypt
    ativo                 BOOLEAN      NOT NULL DEFAULT TRUE,
    criado_em             TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

-- Índice para login rápido por CNPJ
CREATE INDEX IF NOT EXISTS idx_empresa_cnpj ON empresa (cnpj);
