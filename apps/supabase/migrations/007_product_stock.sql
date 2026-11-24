-- Migration to add product_stock table for multi-branch stock tracking
-- This table will track stock levels for each product at each branch/location

CREATE TABLE product_stock (
    organization_id TEXT NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    branch_id TEXT NOT NULL REFERENCES branches(id) ON DELETE CASCADE,
    product_variant_id TEXT NOT NULL REFERENCES product_variants(id) ON DELETE CASCADE,
    quantity REAL NOT NULL CHECK (quantity >= 0),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT (NOW()),
    PRIMARY KEY (organization_id, branch_id, product_variant_id)
);

-- Index for better query performance
CREATE INDEX idx_product_stock_org_branch ON product_stock (organization_id, branch_id);
CREATE INDEX idx_product_stock_branch_product ON product_stock (branch_id, product_variant_id);
CREATE INDEX idx_product_stock_product ON product_stock (product_variant_id);