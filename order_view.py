#!/usr/bin/env python3
import sqlite3
import os

db_path = os.getenv('DB_PATH', r"D:\New\POS\local.db")
conn = sqlite3.connect(db_path)
cursor = conn.cursor()

# Quote the table name to avoid reserved word issues
cursor.execute("""
SELECT sql FROM sqlite_master 
WHERE type='table' AND name='order';
""")
row = cursor.fetchone()
if row:
    print("CREATE TABLE statement for 'order':")
    print(row[0])
else:
    print("Table 'order' schema not found.")
conn.close()