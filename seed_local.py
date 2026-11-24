import sqlite3
import os

db_path = r"D:\New\POS\local.db"

# Connect to the SQLite database
conn = sqlite3.connect(db_path)
cursor = conn.cursor()

# Insert sample organizations
organizations = [
    (1, "Central Café", "Main location", "2023-01-01"),
    (2, "BrewFlow", "Secondary location", "2023-02-01"),
]

# Insert sample branches (branch_id, organization_id, name, address, created_at)
branches = [
    (1, 1, "Main Street", "123 Main St", "2023-01-01"),
    (2, 1, "Park Avenue", "456 Park Ave", "2023-01-01"),
    (3, 2, "Downtown", "789 Downtown", "2023-02-01"),
]

# Insert sample products (product_id, organization_id, branch_id, name, description, price, stock, created_at)
products = [
    (1, 1, 1, "Espresso", "Shot of espresso", 350, 100, "2023-01-01"),
    (2, 1, 1, "Latte", "Milk coffee", 400, 80, "2023-01-01"),
    (3, 1, 1, "Croissant", "Butter croissant", 250, 50, "2023-01-01"),
    (4, 1, 2, "Americano", "Hot water coffee", 300, 70, "2023-01-01"),
]

# Insert sample users (admin accounts)
users = [
    (1, "admin1@example.com", "Admin One", "hashed_password", 1, 1, "2023-01-01"),
    (2, "admin2@example.com", "Admin Two", "hashed_password", 2, 1, "2023-01-01"),
]

# Insert sample customers (optional)
customers = [
    (1, 1, "555-1234", "123 Main St", 0, "2023-01-01", "2023-01-01"),
]

# Insert sample branches referencing organization_id
# (We already inserted branches above; ensure foreign keys are correct)

# Insert organizations first (skip if exists)
for org_id, name, desc, created_at in organizations:
    cursor.execute("""
        INSERT OR IGNORE INTO organization (id, name, description, created_at, updated_at)
        VALUES (?, ?, ?, ?, ?)
    """, (org_id, name, desc, created_at, created_at))

# Insert branches
for branch_id, org_id, name, address, created_at in branches:
    cursor.execute("""
        INSERT OR IGNORE INTO branch (id, organization_id, name, address, created_at, updated_at)
        VALUES (?, ?, ?, ?, ?, ?)
    """, (branch_id, org_id, name, address, created_at, created_at))

# Insert products
for prod_id, org_id, branch_id, name, desc, price, stock, created_at in products:
    cursor.execute("""
        INSERT OR IGNORE INTO product (id, organization_id, branch_id, name, description, price, stock, created_at, updated_at)
        VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)
    """, (prod_id, org_id, branch_id, name, desc, price, stock, created_at, created_at))

# Insert users
for user_id, email, name, pwd_hash, org_id, is_active, created_at in users:
    cursor.execute("""
        INSERT OR IGNORE INTO "user" (id, email, name, password_hash, organization_id, is_active, created_at, updated_at)
        VALUES (?, ?, ?, ?, ?, ?, ?, ?)
    """, (user_id, email, name, pwd_hash, org_id, is_active, created_at, created_at))

# Insert customers
for cust_id, user_id, phone, address, loyalty_points, created_at, updated_at in customers:
    cursor.execute("""
        INSERT OR IGNORE INTO customer (id, user_id, phone, address, loyalty_points, created_at, updated_at)
        VALUES (?, ?, ?, ?, ?, ?, ?)
    """, (cust_id, user_id, phone, address, loyalty_points, created_at, updated_at))

# Commit the transaction
conn.commit()

# Close connection
conn.close()

print("Local SQLite database seeded successfully.")