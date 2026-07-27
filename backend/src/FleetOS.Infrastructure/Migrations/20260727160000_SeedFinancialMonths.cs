using System;
using FleetOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FleetOS.Infrastructure.Migrations
{
    [DbContext(typeof(FleetOsDbContext))]
    [Migration("20260727160000_SeedFinancialMonths")]
    public partial class SeedFinancialMonths : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS "FinancialMonths" (
                    "id" uuid NOT NULL,
                    "year" integer NOT NULL,
                    "month_number" integer NOT NULL,
                    "owner_salary" numeric(18,2) NOT NULL,
                    "status" integer NOT NULL,
                    "opened_at" timestamp with time zone NOT NULL,
                    "closed_at" timestamp with time zone NULL,
                    "tenant_id" uuid NOT NULL,
                    "organization_id" uuid NOT NULL,
                    "business_unit_id" uuid NOT NULL,
                    "created_at" timestamp with time zone NOT NULL,
                    "created_by" uuid NULL,
                    "updated_at" timestamp with time zone NULL,
                    "updated_by" uuid NULL,
                    "deleted_at" timestamp with time zone NULL,
                    "deleted_by" uuid NULL,
                    "row_version" bigint NOT NULL,
                    CONSTRAINT "pk_financial_months" PRIMARY KEY ("id")
                );
            """);

            migrationBuilder.Sql("""
                INSERT INTO "FinancialMonths" ("id", "year", "month_number", "owner_salary", "status", "opened_at", "tenant_id", "organization_id", "business_unit_id", "created_at", "row_version")
                SELECT
                    gen_random_uuid(),
                    EXTRACT(YEAR FROM NOW())::int,
                    EXTRACT(MONTH FROM NOW())::int,
                    COALESCE(t."owner_salary", 0),
                    1,  -- MonthStatus.Open = 1
                    NOW(),
                    t."id",
                    COALESCE(o."id", '00000000-0000-0000-0000-000000000000'),
                    COALESCE(bu."id", '00000000-0000-0000-0000-000000000000'),
                    NOW(),
                    0
                FROM "tenants" t
                LEFT JOIN "organizations" o ON o."tenant_id" = t."id" AND o."deleted_at" IS NULL
                LEFT JOIN "business_units" bu ON bu."tenant_id" = t."id" AND bu."deleted_at" IS NULL
                WHERE NOT EXISTS (
                    SELECT 1 FROM "FinancialMonths" fm WHERE fm."tenant_id" = t."id"
                )
            """);

            // Add financial_month_id column if it doesn't exist (idempotent check)
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'FinancialTransactions'
                        AND column_name = 'financial_month_id'
                    ) THEN
                        ALTER TABLE "FinancialTransactions" ADD COLUMN "financial_month_id" uuid NULL;
                    END IF;
                END $$;
            """);

            // Update existing transactions to link to their tenant's current month
            migrationBuilder.Sql("""
                UPDATE "FinancialTransactions" ft
                SET "financial_month_id" = (
                    SELECT fm."id" FROM "FinancialMonths" fm
                    WHERE fm."tenant_id" = ft."tenant_id"
                    ORDER BY fm."year" DESC, fm."month_number" DESC
                    LIMIT 1
                )
                WHERE ft."financial_month_id" IS NULL
            """);

            // Make the column NOT NULL only if it's still nullable (avoids ownership check)
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'FinancialTransactions'
                        AND column_name = 'financial_month_id'
                        AND is_nullable = 'YES'
                    ) THEN
                        ALTER TABLE "FinancialTransactions" ALTER COLUMN "financial_month_id" SET NOT NULL;
                    END IF;
                END $$;
            """);

            // Create index only if it doesn't exist (avoids ownership check)
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_indexes
                        WHERE tablename = 'FinancialTransactions'
                        AND indexname = 'ix_financial_transactions_financial_month_id'
                    ) THEN
                        CREATE INDEX "ix_financial_transactions_financial_month_id"
                        ON "FinancialTransactions" ("financial_month_id");
                    END IF;
                END $$;
            """);

            // Add FK only if it doesn't exist (avoids ownership check)
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_constraint
                        WHERE conname = 'fk_financial_transactions_financial_months_financial_month_id'
                    ) THEN
                        ALTER TABLE "FinancialTransactions"
                        ADD CONSTRAINT "fk_financial_transactions_financial_months_financial_month_id"
                        FOREIGN KEY ("financial_month_id")
                        REFERENCES "FinancialMonths" ("id")
                        ON DELETE RESTRICT;
                    END IF;
                END $$;
            """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "FinancialTransactions" DROP CONSTRAINT IF EXISTS "fk_financial_transactions_financial_months_financial_month_id"
            """);

            migrationBuilder.Sql("""
                DROP INDEX IF EXISTS "ix_financial_transactions_financial_month_id"
            """);

            migrationBuilder.Sql("""
                ALTER TABLE "FinancialTransactions" DROP COLUMN IF EXISTS "financial_month_id"
            """);

            migrationBuilder.Sql("""
                DROP TABLE IF EXISTS "FinancialMonths"
            """);
        }
    }
}
