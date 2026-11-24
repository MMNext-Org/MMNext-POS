#!/usr/bin/env python3
import sqlite3
import json
import os

# Connect to the SQLite database
db_path = os.getenv('DB_PATH', r"D:\New\POS\local.db")
conn = sqlite3.connect(db_path)
cursor = conn.cursor()

# Fetch the schema of the orders table to verify constraints
cursor.execute("""
SELECT sql FROM sqlite_master 
WHERE type='table' AND name='orders';
""")
row = cursor.fetchone()
if row:
    print("Orders table schema:")
    print(row[0])
else:
    print("Orders table not found!")
conn.close()