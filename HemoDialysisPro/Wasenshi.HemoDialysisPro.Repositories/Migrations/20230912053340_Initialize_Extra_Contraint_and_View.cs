using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Utils;

#nullable disable

namespace Wasenshi.HemoDialysisPro.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class Initialize_Extra_Contraint_and_View : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ====================== Refresh Token ========================
            // ========================= Postgres =====================
            // Put real table behind
            migrationBuilder.CreateTable(
                name: "RefreshToken_Data",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Token = table.Column<string>(nullable: true),
                    Expires = table.Column<DateTime>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    CreatedByIp = table.Column<string>(nullable: true),
                    Revoked = table.Column<DateTime>(nullable: true),
                    RevokedByIp = table.Column<string>(nullable: true),
                    ReplacedByToken = table.Column<string>(nullable: true),
                    UserId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshToken", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshToken_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_UserId",
                table: "RefreshToken_Data",
                column: "UserId");

            // Create view representing the real table instead
            migrationBuilder.Sql($@"CREATE VIEW ""{nameof(RefreshToken)}"" AS SELECT * FROM ""RefreshToken_Data"";");

            // Create trigger for inserting in view
            migrationBuilder.Sql(
@$"
CREATE OR REPLACE FUNCTION create_new_partition_and_insert() RETURNS trigger AS
    $BODY$
    DECLARE
        partition_date TEXT;
        partition TEXT;
        startdate date;
        enddate date;
    BEGIN
        partition_date := to_char(NEW.""{nameof(RefreshToken.Created)}"",'YYYY_MM_DD');
        partition := TG_RELNAME || '_' || partition_date;
        IF NOT EXISTS(SELECT relname FROM pg_class WHERE relname=partition) THEN
        RAISE NOTICE 'A partition has been created %',partition;
        startdate := to_char(NEW.""{nameof(RefreshToken.Created)}"",'YYYY-MM-DD');
        enddate := NEW.""{nameof(RefreshToken.Created)}"" + INTERVAL '1 day';
        EXECUTE 'CREATE TABLE ""' || partition || '"" ( CHECK ( ""{nameof(RefreshToken.Created)}"" >= DATE ''' || startdate || '''  AND ""{nameof(RefreshToken.Created)}"" <  DATE ''' ||  enddate || ''' )) INHERITS (""RefreshToken_Data"");'; 
        EXECUTE 'CREATE INDEX ON ""' || partition || '"" (""{nameof(RefreshToken.Created)}"");';
        END IF;
        IF NEW.""Id"" is null THEN
            NEW.""Id"" := NEXTVAL('""RefreshToken_Data_Id_seq""');
        END IF;
        EXECUTE 'INSERT INTO ""' || partition || '"" SELECT(""RefreshToken_Data"" ' || quote_literal(NEW) || ').* RETURNING ""{nameof(RefreshToken.Id)}"";';
        RETURN NEW;
    END;
    $BODY$
LANGUAGE plpgsql VOLATILE
COST 100;
");

            migrationBuilder.Sql($@"
CREATE TRIGGER {nameof(RefreshToken).ToSnakeCase()}_insert
instead of INSERT ON ""{nameof(RefreshToken)}""
FOR EACH ROW EXECUTE PROCEDURE create_new_partition_and_insert();
");

            //Prevent insertion to the real Table
            migrationBuilder.Sql("CREATE FUNCTION prevent_insert() RETURNS trigger LANGUAGE plpgsql AS $BODY$ BEGIN raise exception 'insert on wrong table'; return NULL; END; $BODY$");
            migrationBuilder.Sql(@"CREATE TRIGGER prevent_insert before insert on ""RefreshToken_Data"" execute procedure prevent_insert();");

            migrationBuilder.Sql($@"
CREATE VIEW refresh_token_partitions AS
SELECT nmsp_parent.nspname AS parent_schema,
       parent.relname AS parent,
       nmsp_child.nspname AS child_schema,
       child.relname AS partition
FROM pg_inherits
JOIN pg_class parent ON pg_inherits.inhparent = parent.oid
JOIN pg_class child ON pg_inherits.inhrelid = child.oid
JOIN pg_namespace nmsp_parent ON nmsp_parent.oid = parent.relnamespace
JOIN pg_namespace nmsp_child ON nmsp_child.oid = child.relnamespace
WHERE parent.relname='RefreshToken_Data'
ORDER BY partition DESC;
            ");

            //Job for scheduling and clear data
            migrationBuilder.Sql($@"
CREATE OR REPLACE FUNCTION prune_refresh_token(max_days INTEGER = 365) RETURNS INTEGER LANGUAGE plpgsql
AS $BODY$
	DECLARE
		discard_tokens TEXT[];
    BEGIN
        IF (SELECT count(1) FROM refresh_token_partitions) > max_days THEN
			discard_tokens := ARRAY(select partition from refresh_token_partitions offset max_days);
			for var in array_lower(discard_tokens, 1)..array_upper(discard_tokens, 1) loop
				EXECUTE 'drop table' || quote_ident(discard_tokens[var]);
			end loop;
			RETURN array_length(discard_tokens, 1);
		END IF;
        RETURN 0;
    END;
$BODY$
            ");

            // ============== Lab Overview ==================
            // Create view manually, for Lab Overview (use for query only)
            migrationBuilder.Sql($@"CREATE VIEW ""LabOverviews"" AS SELECT l.""PatientId"", MAX(l.""EntryTime"") AS ""LastRecord"", count(*) as ""Total"" FROM ""LabExams"" AS l GROUP BY l.""PatientId"";");

            // =============== Assessment Constraint ===================
            // Create trigger for checking assessment type before insert/update dialysis record assessment items
            migrationBuilder.Sql(
@$"
CREATE OR REPLACE FUNCTION check_dialysis_assessment_item() RETURNS trigger AS
    $BODY$
    DECLARE
        assessment_id bigint;
        assessment_type integer;
        
    BEGIN
        assessment_id := NEW.""{nameof(DialysisRecordAssessmentItem.AssessmentId)}"";
        IF NOT EXISTS(SELECT ""{nameof(Assessment.Id)}"" FROM ""{nameof(ApplicationDbContext.Assessments)}"" WHERE ""{nameof(Assessment.Id)}""=assessment_id) THEN
        RAISE NOTICE 'non-existed assessment';
        raise exception 'wrong assessment id';
        RETURN NULL;
        END IF;
        assessment_type := (SELECT ""{nameof(Assessment.Type)}"" FROM ""{nameof(ApplicationDbContext.Assessments)}"" WHERE ""{nameof(Assessment.Id)}""=assessment_id);
        IF assessment_type != 3 THEN
        raise exception 'wrong assessment type';
        RETURN NULL;
        END IF;

        RETURN NEW;
    END;
    $BODY$
LANGUAGE plpgsql VOLATILE
COST 100;
");
            migrationBuilder.Sql(@$"CREATE TRIGGER check_dialysis_assessment_item before INSERT OR UPDATE OF ""{nameof(DialysisRecordAssessmentItem.AssessmentId)}"" on ""{nameof(ApplicationDbContext.DialysisRecordAssessmentItems)}"" for each row execute procedure check_dialysis_assessment_item();");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"DROP TRIGGER check_dialysis_assessment_item ON ""{nameof(ApplicationDbContext.DialysisRecordAssessmentItems)}"";");

            migrationBuilder.Sql($@"DROP FUNCTION check_dialysis_assessment_item;");

            migrationBuilder.Sql($@"DROP VIEW ""LabOverviews"";");

            migrationBuilder.Sql($@"DROP FUNCTION prune_refresh_token;");

            migrationBuilder.Sql($@"DROP VIEW refresh_token_partitions;");

            migrationBuilder.Sql($"DROP FUNCTION prevent_insert;");

            migrationBuilder.Sql($"DROP FUNCTION create_new_partition_and_insert;");

            migrationBuilder.Sql($@"DROP VIEW ""RefreshToken"";");

            migrationBuilder.DropTable(
                name: @"""RefreshToken_Data""");
        }
    }
}
