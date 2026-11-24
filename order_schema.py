#!/usr/bin/env python3
import sqlite3
import os

db_path = os.getenv('DB_PATH', r"D:\New\POS\local.db")
conn = sqlite3.connect(db_path)
cursor = conn.cursor()

# List all tables and their schemas
cursor.execute("SELECT name FROM sqlite_master WHERE type='table'")
tables = cursor.fetchall()
for table in tables:
    table_name = table[0]
    print(f"Table: {table_name}")
    # Get column info - need to quote table name if reserved
    cursor.execute(f"PRAGMA table_info(\"{table_name}\");")
    cols = cursor.fetchall()
    print("  Columns:")
    for col in cols:
        print(f"    {col[1]} ({col[2]})")
    # Get CREATE statement - also quote the name
    cursor.execute(f"SELECT sql FROM sqlite_master WHERE type='table' AND name=\"{table_name}\";")
    create_sql = cursor.fetchone()
    if create_sql:
        print(f"  CREATE: {create_sql[0]}")
    print()
conn.close()