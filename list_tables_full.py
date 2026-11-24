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

# Check each table's schema
tables_dict = {}
for table in tables:
    tables_dict[table[0]] = None

for table in tables:
    quoted_name = '"%s"' % table
    cursor.execute("PRAGMA table_info(%s);" % quoted_name)
    cols = cursor.fetchall()
    tables_dict[table[0]] = [col[1] for col in cols]  # column names

for name, cols in tables_dict.items():
    if cols:
        print(f"\n{name} columns: {cols}")
    else:
        print(f"{name}: (no schema info)")

conn.close()