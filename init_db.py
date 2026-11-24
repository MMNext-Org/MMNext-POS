import sqlite3
import os

# Path to the database file
db_path = r"D:\New\POS\local.db"

# Ensure the directory exists
os.makedirs(os.path.dirname(db_path), exist_ok=True)

# Connect to the SQLite database (this will create the file if it doesn't exist)
conn = sqlite3.connect(db_path)
cursor = conn.cursor()

# Read migration scripts
migration_files = [
    r"D:\New\POS\migrations\001_initial_schema.sql",
    r"D:\New\POS\migrations\002_add_orders_and_items.sql",
    r"D:\New\POS\migrations\004_set_app_context.sql"
]

# Execute each migration file separately to avoid encoding issues
for mig_file in migration_files:
    with open(mig_file, 'r', encoding='utf-8') as f:
        sql_script = f.read()
        try:
            cursor.executescript(sql_script)
        except sqlite3.OperationalError as e:
            print(f"SQLite error executing {mig_file}: {e}")

# Commit the changes and close the connection
conn.commit()
conn.close()

print("Database schema created successfully.")

# Commit the changes and close the connection
conn.commit()
conn.close()

print("Database schema created successfully.")