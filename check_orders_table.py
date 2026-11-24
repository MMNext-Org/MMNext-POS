#!/usr/bin/env python3
import sqlite3
import os

db_path = os.getenv('DB_PATH', r"D:\New\POS\local.db")
conn = sqlite3.connect(db_path)
cursor = conn.cursor()

# Check if 'orders' table exists
cursor.execute("SELECT name FROM sqlite_master WHERE type='table' AND name='orders';")
row = cursor.fetchone()
if row:
    print("Table 'orders' exists.")
else:
    print("Table 'orders' does not exist.")

# Check if 'order' table exists (singular)
cursor.execute("SELECT name FROM sqlite_master WHERE type='table' AND name='order';")
row = cursor.fetchone()
if row:
    print("Table 'order' exists.")
else:
    print("Table 'order' does not exist.")

conn.close()