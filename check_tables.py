import sqlite3
import os

db_path = r"D:\New\POS\local.db"

conn = sqlite3.connect(db_path)
cursor = conn.cursor()
cursor.execute("SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;")
tables = cursor.fetchall()
print("Tables:", [row[0] for row in tables])
conn.close()