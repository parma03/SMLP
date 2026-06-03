# MIGRATION GUIDE — SMLP

Panduan migrasi database untuk proyek SMLP.

---

## Prasyarat

```bash
# Install / update dotnet-ef tool
dotnet tool update --global dotnet-ef

# Set environment (pilih salah satu sesuai shell)
# PowerShell:
$env:ASPNETCORE_ENVIRONMENT='Local'
# CMD:
set ASPNETCORE_ENVIRONMENT=Local
# Bash/Linux:
export ASPNETCORE_ENVIRONMENT=Local
```

---

## ✅ Migration yang Sudah Dijalankan

| # | Nama Migration | Keterangan |
|---|---|---|
| - | (Cek folder `05.Infrastructure/Persistence/SqlServer/Migrations`) | |

---

## 🔜 Migration yang Perlu Dijalankan

### 1. AddTransactionPayment

Menambahkan tabel `TransactionPayments` untuk mendukung split payment.
`TransactionPayment` sudah ada di domain dan DbContext, namun belum ada migration-nya.

**Langkah:**

```bash
# 1. Pastikan connection string sudah benar di 06.WebApi/appsettings.Local.json

# 2. Jalankan dari root project (direktori yang mengandung folder 05.Infrastructure dan 06.WebApi)
dotnet ef migrations add AddTransactionPayment \
    --context SqlServerSmlpDbContext \
    --project 05.Infrastructure \
    --startup-project 06.WebApi \
    --output-dir Persistence/SqlServer/Migrations

# 3. Verifikasi file migration yang dihasilkan (cek folder 05.Infrastructure/Persistence/SqlServer/Migrations)

# 4. Apply migration ke database
dotnet ef database update \
    --context SqlServerSmlpDbContext \
    --project 05.Infrastructure \
    --startup-project 06.WebApi
```

**Verifikasi setelah migration:**
- Tabel `TransactionPayments` muncul di database
- Kolom: `Id`, `TransactionId` (FK), `PaymentMethod`, `Amount`, `Reference`, `CreatedAt`, `UpdatedAt`
- Test trigger manual via Scheduler UI → Jobs → ExpiredPointsJob

---

### 2. (Migration berikutnya jika ada)

Tambahkan di sini jika ada schema baru.

---

## Rollback Migration

```bash
# Kembali ke migration tertentu (ganti NamaMigrationTarget)
dotnet ef database update NamaMigrationTarget \
    --context SqlServerSmlpDbContext \
    --project 05.Infrastructure \
    --startup-project 06.WebApi

# Hapus migration terakhir (hanya jika belum di-apply ke DB)
dotnet ef migrations remove \
    --context SqlServerSmlpDbContext \
    --project 05.Infrastructure \
    --startup-project 06.WebApi
```

---

## Catatan

- Selalu jalankan migration di environment `Local` atau `Development` terlebih dahulu sebelum `Production`.
- Backup database production sebelum menjalankan `database update` di production.
- File migration di-commit ke Git; jangan hapus file migration yang sudah di-apply.
