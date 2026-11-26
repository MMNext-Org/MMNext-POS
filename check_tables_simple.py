import sqlite3

conn = sqlite3.connect('local.db')
cursor = conn.cursor()
cursor.execute("SELECT name FROM sqlite_master WHERE type='table'")
tables = cursor.fetchall()
print("Tables:", [row[0] for row in tables])
conn.close()