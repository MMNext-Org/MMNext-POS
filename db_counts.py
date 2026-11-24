import sqlite3

db = sqlite3.connect("D:\\\\New\\\\POS\\\\local.db")
cursor = db.cursor()
tables = ["organization", "branch", "product", "user", "customer", "order", "order_item"]
for t in tables:
    # Quote table name to avoid SQL syntax issues
    cursor.execute(f"SELECT COUNT(*) FROM \"{t}\"")
    print(f"{t}: {cursor.fetchone()[0]}")
db.close()