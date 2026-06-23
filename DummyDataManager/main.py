import csv
import sqlite3

conn = sqlite3.connect("dev_design_inventory.db")
cursor = conn.cursor()

try:
    cursor.execute("BEGIN TRANSACTION")

    with open("Products.csv", "r", encoding="utf-8") as file:
        reader = csv.DictReader(file)

        products = [
            (
                row["Name"],
                row["Qty"],
                row["RetailPrice"],
                row["ListPrice"],
                row["CategoryId"],
                0,
                0.10
            )
            for row in reader
        ]
    
    categories = [(idx, i, 1) if i != "Cosmetics" else (idx, i, 0) for idx, i in enumerate(["Generic", "Branded", "Cosmetics"], start=1)] #; print(categories)
    cursor.executemany("""
    INSERT OR IGNORE INTO Categories (
        Id,
        Name,
        IsPharma
    )
    VALUES (?, ?, ?)
    """, categories)

    cursor.executemany("""
    INSERT OR IGNORE INTO Products (
        Name,
        Qty,
        RetailPrice,
        CostPrice,
        CategoryId,
        IsDeleted,
        MarkupRate
    )
    VALUES (?, ?, ?, ?, ?, ?, ?)
    """, products)
    
    cursor.execute("""
    UPDATE Products
    SET Barcode =
    (
        WITH base AS (
            SELECT printf('480%09d', Id) AS code12
        ),
        digits AS (
            SELECT
                CAST(substr(code12, 1, 1) AS INTEGER) AS d1,
                CAST(substr(code12, 2, 1) AS INTEGER) AS d2,
                CAST(substr(code12, 3, 1) AS INTEGER) AS d3,
                CAST(substr(code12, 4, 1) AS INTEGER) AS d4,
                CAST(substr(code12, 5, 1) AS INTEGER) AS d5,
                CAST(substr(code12, 6, 1) AS INTEGER) AS d6,
                CAST(substr(code12, 7, 1) AS INTEGER) AS d7,
                CAST(substr(code12, 8, 1) AS INTEGER) AS d8,
                CAST(substr(code12, 9, 1) AS INTEGER) AS d9,
                CAST(substr(code12, 10, 1) AS INTEGER) AS d10,
                CAST(substr(code12, 11, 1) AS INTEGER) AS d11,
                CAST(substr(code12, 12, 1) AS INTEGER) AS d12,
                code12
            FROM base
        )
        SELECT code12 ||
            ((10 - (
                    (
                        d1 + d3 + d5 + d7 + d9 + d11 +
                        3 * (d2 + d4 + d6 + d8 + d10 + d12)
                    ) % 10
            )) % 10)
        FROM digits
    )
    WHERE Barcode IS NULL OR Barcode = '';
    """)

    conn.commit()
    print("Products imported successfully.")

except Exception as e:
    conn.rollback()
    print("Import failed:", e)