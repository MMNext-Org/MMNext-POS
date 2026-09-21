"""Systematic schema-vs-model drift audit for MMNext POS.

Compares:
  1. Tables/columns created by migration SQL files (src/MMNextPOS.Infrastructure/Migrations/*.sql)
  2. Domain model properties (src/MMNextPOS.Domain/Models/*.cs), excluding
     [NotMapped] / computed / navigation properties, mapped to table names via
     the repositories' `base(unitOfWork, "TableName")` declarations.

Reports every missing column per table.
"""
import re
import glob
from pathlib import Path

ROOT = Path(r"J:\Project 1\MMNext POS")
MIGRATIONS = ROOT / "src/MMNextPOS.Infrastructure/Migrations"
MODELS = ROOT / "src/MMNextPOS.Domain/Models"
REPOS = ROOT / "src/MMNextPOS.Infrastructure/Repositories"

# ── 1. Parse migration SQL → table → columns ──────────────────────────────
table_cols: dict[str, set[str]] = {}
col_type: dict[tuple[str, str], str] = {}

create_re = re.compile(r"CREATE TABLE IF NOT EXISTS\s+`?(\w+)`?\s*\(", re.IGNORECASE)
stop_re = re.compile(r"^\s*(\)\s*ENGINE|\)\s*;|\)\s*$|CONSTRAINT|PRIMARY KEY|UNIQUE KEY|UNIQUE\s|FOREIGN KEY|KEY\s)", re.IGNORECASE)
col_re = re.compile(r"^\s*`?(\w+)`?\s+([A-Za-z]+)", re.IGNORECASE)

for sql_file in glob.glob(str(MIGRATIONS / "*.sql")):
    text = Path(sql_file).read_text(encoding="utf-8", errors="ignore")
    current = None
    for line in text.splitlines():
        m = create_re.search(line)
        if m:
            current = m.group(1)
            table_cols.setdefault(current, set())
            continue
        if current is None:
            continue
        if stop_re.match(line):
            # close on a line that ends the column list
            if re.match(r"^\s*\)", line):
                current = None
            continue
        cm = col_re.match(line)
        if cm:
            name, typ = cm.group(1), cm.group(2).upper()
            if name.upper() in ("CONSTRAINT", "PRIMARY", "UNIQUE", "FOREIGN", "KEY"):
                continue
            table_cols[current].add(name)
            col_type[(current, name)] = typ

# Also parse ALTER TABLE ADD COLUMN statements — both plain SQL and the
# guarded "PREPARE FROM @sql" pattern where the ALTER lives inside a string literal.
alter_re = re.compile(r"ALTER TABLE\s+`?(\w+)`?\s+ADD (?:COLUMN\s+)?`?(\w+)`?\s+([A-Za-z]+)", re.IGNORECASE)
for sql_file in glob.glob(str(MIGRATIONS / "*.sql")):
    text = Path(sql_file).read_text(encoding="utf-8", errors="ignore")
    for line in text.splitlines():
        am = alter_re.search(line)
        if am:
            t, n, typ = am.group(1), am.group(2), am.group(3).upper()
            table_cols.setdefault(t, set()).add(n)
            col_type[(t, n)] = typ

# ── 2. Parse repos → entity class → table name ────────────────────────────
entity_table: dict[str, str] = {}
repo_re = re.compile(r"class\s+(\w+)\s*:\s*GenericRepository<(\w+)>", re.IGNORECASE)
base_re = re.compile(r"base\(unitOfWork,\s*\"(\w+)\"\)", re.IGNORECASE)

for repo_file in glob.glob(str(REPOS / "*.cs")):
    text = Path(repo_file).read_text(encoding="utf-8", errors="ignore")
    rm = repo_re.search(text)
    if not rm:
        continue
    entity = rm.group(2)
    bm = base_re.search(text)
    if bm:
        entity_table[entity] = bm.group(1)

# ── 3. Parse models → properties ─────────────────────────────────────────
skip_types = re.compile(r"^(ICollection|IReadOnlyList|IList|IEnumerable|List)<", re.IGNORECASE)
nav_model_types = {"StockMovement", "Product", "Customer", "Supplier", "Sale", "Purchase", "SaleReceipt", "PurchaseReceipt", "StockTransfer", "Assembly"}

def model_props(path: Path) -> tuple[str, set[str], dict[str, str]]:
    text = path.read_text(encoding="utf-8", errors="ignore")
    cm = re.search(r"class\s+(\w+)\s*:\s*EntityBase", text)
    if not cm:
        return "", set(), {}
    cls = cm.group(1)
    props: set[str] = set()
    ptypes: dict[str, str] = {}
    lines = text.splitlines()
    i = 0
    notmapped = False
    while i < len(lines):
        line = lines[i]
        if "NotMapped" in line:
            notmapped = True
            i += 1
            continue
        # auto-property possibly with initializer, single line
        am = re.match(r"\s*public\s+([\w<>?,\[\]\. ]+?)\s+(\w+)\s*\{\s*get;\s*set;.*\}", line)
        # expression-bodied / multi-line property start
        em = re.match(r"\s*public\s+([\w<>?,\[\]\. ]+?)\s+(\w+)\s*$", line)
        next_line = lines[i + 1] if i + 1 < len(lines) else ""
        if am:
            name, typ = am.group(2), am.group(1).strip()
            if not skip_types.match(typ) and typ not in nav_model_types:
                props.add(name)
                ptypes[name] = typ
            notmapped = False
        elif em and re.match(r"\s*\{\s*get(\s*;| =>)", next_line):
            name, typ = em.group(2), em.group(1).strip()
            # computed or navigational property → only add if it has plain get;set on following lines
            block = " ".join(lines[i:i + 6])
            if notmapped or ("=>" in block and "get;" not in block.replace("get =>", "")):
                pass  # skip computed
            elif re.search(r"\{\s*get;\s*set;", block):
                if not skip_types.match(typ):
                    props.add(name)
                    ptypes[name] = typ
            notmapped = False
        else:
            if not ("NotMapped" in line):
                notmapped = False
        i += 1

    # EntityBase inherited columns
    props.update(["Id", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "IsDeleted"])
    return cls, props, ptypes

# ── 4. Compare ────────────────────────────────────────────────────────────
print("=== SCHEMA DRIFT REPORT: model properties missing from DB table ===\n")
issues = 0
for model_file in sorted(glob.glob(str(MODELS / "*.cs"))):
    cls, props, ptypes = model_props(Path(model_file))
    if not cls:
        continue
    table = entity_table.get(cls)
    if not table:
        continue
    cols = table_cols.get(table)
    if cols is None:
        print(f"[{cls}] -> table `{table}`: TABLE NOT CREATED BY ANY MIGRATION")
        issues += 1
        continue
    missing = props - cols
    if missing:
        print(f"[{cls}] -> table `{table}` MISSING: {sorted(missing)}")
        issues += 1

print(f"\nTotal tables with drift: {issues}")

