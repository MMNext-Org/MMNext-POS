-- Add orders and order_items tables (if not exists)
CREATE TABLE IF NOT EXISTS orders (  -- Table name corrected to plural 'orders' to match backend expectations
    id TEXT PRIMARY KEY,
    organization_id TEXT NOT NULL,
    branch_id TEXT NOT NULL,
    status TEXT NOT NULL DEFAULT 'draft',
    total REAL NOT NULL,
    items TEXT NOT NULL,
    created_at TEXT NOT NULL DEFAULT (datetime('now')),
    updated_at TEXT NOT NULL DEFAULT (datetime('now')),
    FOREIGN KEY (organization_id) REFERENCES organizations(id) ON DELETE CASCADE,
    FOREIGN KEY (branch_id) REFERENCES branches(id) ON DELETE CASCADE,
    UNIQUE (organization_id, branch_id, status, total, items)
);

CREATE TABLE IF NOT EXISTS order_items (
    id TEXT PRIMARY KEY,
    order_id TEXT NOT NULL,
    product_id TEXT NOT NULL,
    quantity INTEGER NOT NULL DEFAULT 0,
    unit_price REAL NOT NULL,
    created_at TEXT NOT NULL DEFAULT (datetime('now')),
    updated_at TEXT NOT NULL DEFAULT (datetime('now')),
    FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE,  -- Changed 'orders' to 'order'
    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE
);
