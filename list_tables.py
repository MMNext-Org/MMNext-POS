#!/usr/bin/env python3
import sqlite3
import os

db_path = os.getenv('DB_PATH', r"D:\New\POS\local.db")
conn = sqlite3.connect(db_path)
cursor = conn.cursor()

cursor.execute("SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;")
tables = cursor.fetchall()
print("Tables:")
for table in tables:
    print(table[0])

conn.close()