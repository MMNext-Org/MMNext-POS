import sqlite3

db_path = r"D:\New\POS\local.db"
conn = sqlite3.connect(db_path)
cursor = conn.cursor()
cursor.execute("PRAGMA table_info(order)")
rows = cursor.fetchall()
print("Columns in order table:")
for row in rows:
    print(row)
conn.close()