import sqlite3
import os

db_path = r"D:\New\POS\local.db"

conn = sqlite3.connect(db_path)
cursor = conn.cursor()

# Count rows in key tables
tables = ['organization', 'branch', 'product', 'user', 'customer']
for table in tables:
    cursor.execute(f"SELECT COUNT(*) FROM {table}")
    count = cursor.fetchone()[0]
    print(f"{table}: {count}")

# Show first few rows from each table
for table in tables:
    cursor.execute(f"SELECT * FROM {table} LIMIT 5")
    rows = cursor.fetchall()
    print(f"\nFirst rows in {table}:")
    for row in rows:
        print(row)

conn.close()