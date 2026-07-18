using CodeAppsDataMigration.Data;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml;
using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic;
using Npgsql;
using System;
using System.Data;
using System.IO.Packaging;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;

namespace CodeAppsDataMigration.Migration
{
    public class MigrationRunner
    {
        private readonly string _sql;
        private readonly string _pg;
        private Action<string, int>? _onProgress;

        // Shared PostgreSQL connection + transaction for one branch's migration.
        // When active, every Postgres write goes through these so a failure anywhere
        // rolls the whole branch back. See BeginBranchTransaction/Commit/Rollback.
        private NpgsqlConnection? _pgConn;
        private NpgsqlTransaction? _pgTxn;

        public MigrationRunner(string sql, string pg)
        {
            _sql = sql;
            _pg = pg;
        }

        public void SetProgressCallback(Action<string, int> onProgress)
        {
            _onProgress = onProgress;
        }

        // ==================================================================
        // TRANSACTION SCOPE
        // ------------------------------------------------------------------
        // Wrap a branch's full pipeline:
        //     runner.BeginBranchTransaction();
        //     try   { ...all migration steps...; runner.CommitBranchTransaction(); }
        //     catch { runner.RollbackBranchTransaction(); throw; }
        // While a transaction is open, all Postgres work uses the shared
        // connection so it commits or reverts as a single unit.
        // ==================================================================

        /// <summary>Opens the shared PostgreSQL connection and starts a transaction.</summary>
        public void BeginBranchTransaction()
        {
            _pgConn = PostgresConnection.Create();
            _pgConn.Open();
            _pgTxn = _pgConn.BeginTransaction();
        }

        /// <summary>Commits the shared transaction and releases the connection.</summary>
        public void CommitBranchTransaction()
        {
            try
            {
                _pgTxn?.Commit();
            }
            finally
            {
                _pgTxn?.Dispose();
                _pgConn?.Dispose();
                _pgTxn = null;
                _pgConn = null;
            }
        }

        /// <summary>Rolls back the shared transaction and releases the connection.</summary>
        public void RollbackBranchTransaction()
        {
            try
            {
                _pgTxn?.Rollback();
            }
            catch
            {
                // Ignore rollback errors (e.g. connection already broken).
            }
            finally
            {
                _pgTxn?.Dispose();
                _pgConn?.Dispose();
                _pgTxn = null;
                _pgConn = null;
            }
        }

        /// <summary>
        /// Builds an NpgsqlCommand bound to the active shared connection and transaction.
        /// Requires BeginBranchTransaction() to have been called first.
        /// </summary>
        private NpgsqlCommand PgCmd(string sql)
        {
            if (_pgConn == null || _pgTxn == null)
                throw new InvalidOperationException(
                    "No active PostgreSQL transaction. Call BeginBranchTransaction() before running migration steps.");

            return new NpgsqlCommand(sql, _pgConn, _pgTxn);
        }

        /// <summary>
        /// Executes a PostgreSQL non-query on the shared transaction. On failure it throws
        /// a MigrationException carrying the failing SQL script, the target table, and the
        /// calling function name (captured automatically) so the UI can show all three.
        /// </summary>
        private int ExecPgNonQuery(string sql, [System.Runtime.CompilerServices.CallerMemberName] string caller = "")
        {
            // Nothing to run: callers build their SQL by looping over source rows, so an
            // empty script just means the branch had no rows for that table. Executing it
            // would make Npgsql throw "CommandText property has not been initialized" and
            // needlessly abort the branch transaction, so treat it as a no-op.
            if (string.IsNullOrWhiteSpace(sql))
                return 0;

            try
            {
                using var cmd = PgCmd(sql);
                return cmd.ExecuteNonQuery();
            }
            catch (Exception ex) when (!(ex is MigrationException))
            {
                Console.WriteLine($"ERROR in {caller}: {ExceptionFormatter.Describe(ex)}");
                throw new MigrationException(
                    $"'{caller}' failed on table '{ExceptionFormatter.ExtractTableName(sql)}'." + Environment.NewLine +
                    "Failing query: " + sql + Environment.NewLine + ex.Message,
                    tableName: ExceptionFormatter.ExtractTableName(sql),
                    inner: ex,
                    failingQuery: sql,
                    functionName: "MigrationRunner." + caller);
            }
        }

        private void ReportProgress(string message, int percent)
        {
            _onProgress?.Invoke(message, percent);
        }

        public void RunAll(Int64 nMainBranchId, Int64 nBranchId, int nFromBranchId)
        {
            Console.WriteLine("=======================================");
            Console.WriteLine("   SQL Server → PostgreSQL Migration");
            Console.WriteLine("=======================================\n");

            var migrator = new DynamicMigrator(_sql, _pg);

            MigrationConfig.nMainBranchId = nMainBranchId;
            MigrationConfig.nBranchId = nBranchId;
            MigrationConfig.nFromBranchId = nFromBranchId;

            PreMigrationCleanup(nMainBranchId, nBranchId);

            int totalTables = MigrationConfig.Tables.Count;
            int tableIndex = 1;

            // --------------------------------------------------
            // 1️⃣ MIGRATE ALL TABLES
            // --------------------------------------------------
            foreach (var table in MigrationConfig.Tables)
            {
                int pct = (int)((double)tableIndex / totalTables * 80); // 0-80% for tables
                ReportProgress($"[{tableIndex}/{totalTables}] Migrating: {table.SqlTable}", pct);

                Console.WriteLine(
                    $"[{tableIndex}/{totalTables}] " +
                    $"Migrating table: {table.SqlTable}");

                try
                {
                    var start = DateTime.Now;

                    int rows = migrator.Run(table, _pgConn, _pgTxn);

                    var end = DateTime.Now;

                    ReportProgress($"Done: {table.SqlTable} - {rows:N0} rows ({(end - start).TotalSeconds:N1}s)", pct);

                    Console.WriteLine(
                        $" {table.SqlTable} → {rows:N0} rows " +
                        $"({(end - start).TotalSeconds:N1} sec)\n");
                }
                catch (Exception ex)
                {
                    ReportProgress($"FAILED: {table.SqlTable} - {ex.Message}", pct);

                    Console.ForegroundColor = ConsoleColor.Red;

                    Console.WriteLine(
                        $" FAILED: {table.SqlTable}\n" +
                        $"Reason: {ex.Message}\n");

                    Console.ResetColor();

                    // stop migration if any table fails
                    throw;
                }

                tableIndex++;
            }

            // --------------------------------------------------
            // 2️⃣ UPDATE FOREIGN KEYS
            // --------------------------------------------------
            Console.WriteLine("Updating foreign keys...\n");

            //try
            //{
            //    FkUpdater.UpdateAll(_pg);
            //    Console.WriteLine(" Foreign keys updated successfully\n");
            //}
            //catch (Exception ex)
            //{
            //    Console.ForegroundColor = ConsoleColor.Yellow;

            //    Console.WriteLine(
            //        " Foreign key update failed\n" +
            //        ex.Message);

            //    Console.ResetColor();
            //}

            // --------------------------------------------------
            // DONE
            // --------------------------------------------------
            Console.WriteLine("=======================================");
            Console.WriteLine("   MIGRATION COMPLETED SUCCESSFULLY");
            Console.WriteLine("=======================================");
        }




        private void PreMigrationCleanup(Int64 nMainBranchId, Int64 nBranchId)
        {
            var cleanups = new (string Table, string ExtraWhere)[]
            {
                ("area",       ""),
                ("billseries", " AND billsersource IN ('SALES','PURCHASE','SERVICE BILL')"),
                ("category",   ""),
                ("notes",      ""),
                ("printdisplaysettings",      "")
            };

            ReportProgress("Pre-migration cleanup...", 0);
            Console.WriteLine("Pre-migration cleanup (area, billseries [SALES/PURCHASE/SERVICE BILL only], category, notes)...");

            string currentSql = "";
            string currentTable = "";
            try
            {
                foreach (var (table, extra) in cleanups)
                {
                    currentTable = table;
                    currentSql = $"DELETE FROM {table} WHERE branchid = {nBranchId} AND mainbranchid = {nMainBranchId}{extra}";
                    using var cmd = PgCmd(currentSql);
                    cmd.CommandTimeout = 120;
                    int affected = cmd.ExecuteNonQuery();
                    ReportProgress($"Cleaned {table}: {affected} row(s) removed", 0);
                    Console.WriteLine($" {table} → {affected} row(s) deleted");
                }
            }
            catch (Exception ex) when (!(ex is MigrationException))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(" Pre-migration cleanup failed: " + ex.Message);
                Console.ResetColor();
                throw new MigrationException(
                    $"Pre-migration cleanup failed on table '{currentTable}'." + Environment.NewLine +
                    "Failing query: " + currentSql + Environment.NewLine + ex.Message,
                    tableName: currentTable,
                    inner: ex,
                    failingQuery: currentSql,
                    functionName: "MigrationRunner." + nameof(PreMigrationCleanup));
            }
        }

        private void ExecuteBulkUpdates(Int64 nMainBranchId, Int64 nBranchId, Int64 nFromBranchId)
        {
            string testquerytemplate = "";
            try
            {

                List<string> stringBuilder = new List<string>();
                stringBuilder.Add($"UPDATE accounthead{nMainBranchId} ah SET areaid = a.area_id FROM area a WHERE a.tempid = ah.areaid AND ah.branchid = {nBranchId} and ah.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"UPDATE accounthead{nMainBranchId} ah SET bankflag = true  WHERE ah.supplytype = 'Yes' AND ah.branchid = {nBranchId} and ah.mainbranchid = {nMainBranchId}");

                stringBuilder.Add($"UPDATE branch im SET acid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = im.acid AND im.branchid = {nBranchId} and im.mainbranchid = {nMainBranchId}");

                stringBuilder.Add($"UPDATE productmain{nMainBranchId} pm SET categoryid = ca.categoryid FROM category ca WHERE ca.branchid={nBranchId} and  ca.tempid = pm.categoryid AND pm.branchid = {nBranchId} and pm.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"UPDATE productmain{nMainBranchId} pm SET taxid = tx.taxid FROM tax tx WHERE tx.taxpercent = pm.prodlinkeshopid AND pm.branchid = {nBranchId} and pm.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"UPDATE productmain{nMainBranchId} pm SET manufacture_id = mf.manufacture_id FROM manufacture{nMainBranchId} mf WHERE mf.tempid = pm.manufacture_id AND pm.branchid = {nBranchId} and pm.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"UPDATE productmain{nMainBranchId} pm SET hsnid = hs.hsn_id FROM hsn{nMainBranchId} hs WHERE hs.tempid = pm.hsnid AND pm.branchid = {nBranchId} and pm.mainbranchid = {nMainBranchId}");

                stringBuilder.Add($"UPDATE productmain{nMainBranchId} pm SET productsearch = itemdesc  WHERE pm.producttype = 'serviceitem' AND pm.branchid = {nBranchId} and pm.mainbranchid = {nMainBranchId}");

                stringBuilder.Add($"delete from ProductSub{nMainBranchId} ps where ps.productid not in (select tempid from productmain{nMainBranchId} ) and ps.branchid = {nBranchId}");

                stringBuilder.Add($"UPDATE productsub{nMainBranchId} isub SET productid = pm.productid FROM productmain{nMainBranchId} pm WHERE pm.tempid = isub.productid AND isub.branchid = {nBranchId} and  isub.mainbranchid = {nMainBranchId} and pm.producttype='product'");

                stringBuilder.Add($"UPDATE hsn{nMainBranchId} hs SET taxid = pm.taxid FROM productmain{nMainBranchId} pm WHERE hs.hsn_id = pm.hsnid AND pm.branchid = {nBranchId} and pm.mainbranchid = {nMainBranchId}");

                // Sales
                stringBuilder.Add($"UPDATE issuesubdetails{nMainBranchId} isub SET productid = pm.productid FROM productmain{nMainBranchId} pm WHERE pm.tempid = isub.productid AND  isub.branchid = {nBranchId} AND  isub.mainbranchid = {nMainBranchId} and pm.producttype='product'");
                stringBuilder.Add($"UPDATE issuemain{nMainBranchId} im SET acid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = im.acid AND im.branchid = {nBranchId} and im.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"UPDATE issuemain{nMainBranchId} im SET salesexeid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = im.salesexeid AND im.branchid = {nBranchId} and im.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"UPDATE issuemain{nMainBranchId} im SET staffid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = im.staffid AND im.branchid = {nBranchId} and im.mainbranchid = {nMainBranchId}");
                var strQuery = $"update issuemain{nMainBranchId} im set billserid =  bs.billserid from billseries bs where bs.tempid = im.billserid";
                strQuery += $"\n and bs.branchid = im.branchid and bs.mainbranchid = im.mainbranchid";
                strQuery += $"\n and im.branchid ={nBranchId}    and im.mainbranchid ={nMainBranchId} and bs.billsersource='SALES' and  bs.branchid = {nBranchId} and bs.mainbranchid = {nMainBranchId}";
                stringBuilder.Add(strQuery);

                stringBuilder.Add($"UPDATE issuesubdetails{nMainBranchId} pm SET taxid = tx.taxid FROM tax tx WHERE tx.taxpercent = pm.taxpers AND pm.branchid = {nBranchId} and pm.mainbranchid = {nMainBranchId}");

                strQuery = $"update issuesubdetails{nMainBranchId} im set billserid =  bs.billserid from billseries bs where bs.tempid = im.billserid";
                strQuery += $"\n and bs.branchid = im.branchid and bs.mainbranchid = im.mainbranchid";
                strQuery += $"\n and im.branchid ={nBranchId}    and im.mainbranchid ={nMainBranchId} and bs.billsersource='SALES' and  bs.branchid = {nBranchId} and bs.mainbranchid = {nMainBranchId}";
                stringBuilder.Add(strQuery);


                //

                // SalesOrder Details
                stringBuilder.Add($"UPDATE salesorderdetails{nMainBranchId} isub SET productid = pm.productid FROM productmain{nMainBranchId} pm WHERE pm.tempid = isub.productid AND  isub.branchid = {nBranchId} AND  isub.mainbranchid = {nMainBranchId} and pm.producttype='product'");
                stringBuilder.Add($"UPDATE salesordermain{nMainBranchId} im SET acid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = im.acid AND im.branchid = {nBranchId} and im.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"UPDATE salesordermain{nMainBranchId} im SET salesexeid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = im.salesexeid AND im.branchid = {nBranchId} and im.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"UPDATE salesordermain{nMainBranchId} im SET staffid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = im.staffid AND im.branchid = {nBranchId} and im.mainbranchid = {nMainBranchId}");
                strQuery = $"update salesordermain{nMainBranchId} im set billserid =  bs.billserid from billseries bs where bs.tempid = im.billserid";
                strQuery += $"\n and bs.branchid = im.branchid and bs.mainbranchid = im.mainbranchid";
                strQuery += $"\n and im.branchid ={nBranchId}    and im.mainbranchid ={nMainBranchId} and bs.billsersource='SALES ORDER' and  bs.branchid = {nBranchId} and bs.mainbranchid = {nMainBranchId}";
                stringBuilder.Add(strQuery);

                stringBuilder.Add($"UPDATE salesorderdetails{nMainBranchId} pm SET taxid = tx.taxid FROM tax tx WHERE tx.taxpercent = pm.taxpers AND pm.branchid = {nBranchId} and pm.mainbranchid = {nMainBranchId}");

                strQuery = $"update salesorderdetails{nMainBranchId} im set billserid =  bs.billserid from billseries bs where bs.tempid = im.billserid";
                strQuery += $"\n and bs.branchid = im.branchid and bs.mainbranchid = im.mainbranchid";
                strQuery += $"\n and im.branchid ={nBranchId}    and im.mainbranchid ={nMainBranchId} and bs.billsersource='SALES ORDER' and  bs.branchid = {nBranchId} and bs.mainbranchid = {nMainBranchId}";
                stringBuilder.Add(strQuery);

                // servicemain

                stringBuilder.Add($"UPDATE servicemain{nMainBranchId} im SET acid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = im.acid AND im.branchid = {nBranchId} and im.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"UPDATE servicemain{nMainBranchId} im SET salesexeid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = im.salesexeid AND im.branchid = {nBranchId} and im.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"UPDATE servicemain{nMainBranchId} im SET staffid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = im.staffid AND im.branchid = {nBranchId} and im.mainbranchid = {nMainBranchId}");
                strQuery = $"update servicemain{nMainBranchId} im set billserid =  bs.billserid from billseries bs where bs.tempid = im.billserid";
                strQuery += $"\n and bs.branchid = im.branchid and bs.mainbranchid = im.mainbranchid";
                strQuery += $"\n and im.branchid ={nBranchId}    and im.mainbranchid ={nMainBranchId} and bs.billsersource='SERVICE BILL' and  bs.branchid = {nBranchId} and bs.mainbranchid = {nMainBranchId}";
                stringBuilder.Add(strQuery);

                // servicesubdetails
                stringBuilder.Add($"UPDATE servicesubdetails{nMainBranchId} isub SET productid = pm.productid FROM productmain{nMainBranchId} pm WHERE pm.tempid = isub.productid AND  isub.branchid = {nBranchId} AND  isub.mainbranchid = {nMainBranchId} and pm.producttype='product' and isub.prodfrom='Product'");
                stringBuilder.Add($"UPDATE servicesubdetails{nMainBranchId} isub SET productid = pm.productid FROM productmain{nMainBranchId} pm WHERE pm.tempid = isub.productid AND  isub.branchid = {nBranchId} AND  isub.mainbranchid = {nMainBranchId} and pm.producttype='serviceitem' and isub.prodfrom != 'Product'");
                stringBuilder.Add($"UPDATE servicesubdetails{nMainBranchId} pm SET taxid = tx.taxid FROM tax tx WHERE tx.taxpercent = pm.taxpers AND pm.branchid = {nBranchId} and pm.mainbranchid = {nMainBranchId}");

                strQuery = $"update servicesubdetails{nMainBranchId} isub set billserid =  sm.billserid from servicemain{nMainBranchId} sm where isub.servicemainno = sm.servicemainno";
                strQuery += $"\n and isub.servicemainno = sm.servicemainno and isub.branchid = sm.branchid and isub.mainbranchid = sm.mainbranchid";
                strQuery += $"\n and sm.branchid ={nBranchId} and sm.mainbranchid ={nMainBranchId}";
                stringBuilder.Add(strQuery);


                // Purchase
                stringBuilder.Add($"UPDATE receiptdetails{nMainBranchId} isub SET productid = pm.productid FROM productmain{nMainBranchId} pm WHERE pm.tempid = isub.productid AND  isub.branchid = {nBranchId} AND isub.mainbranchid = {nMainBranchId} and pm.producttype='product'");
                stringBuilder.Add($"UPDATE receiptmain{nMainBranchId} im SET acid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = im.acid AND im.branchid = {nBranchId} and im.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"UPDATE receiptmain{nMainBranchId} im SET staffid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = im.staffid AND im.branchid = {nBranchId} AND im.mainbranchid = {nMainBranchId}");
                strQuery = $"update receiptmain{nMainBranchId} im set billserid =  bs.billserid from billseries bs where bs.tempid = im.billserid";
                strQuery += $"\n and bs.branchid = im.branchid and bs.mainbranchid = im.mainbranchid";
                strQuery += $"\n and im.branchid ={nBranchId}    and im.mainbranchid ={nMainBranchId} and bs.billsersource='PURCHASE' and  bs.branchid = {nBranchId} and bs.mainbranchid = {nMainBranchId}";
                stringBuilder.Add(strQuery);

                strQuery = $"update receiptdetails{nMainBranchId} im set billserid =  bs.billserid from billseries bs where bs.tempid = im.billserid";
                strQuery += $"\n and bs.branchid = im.branchid and bs.mainbranchid = im.mainbranchid";
                strQuery += $"\n and im.branchid ={nBranchId}    and im.mainbranchid ={nMainBranchId} and bs.billsersource='PURCHASE' and  bs.branchid = {nBranchId} and bs.mainbranchid = {nMainBranchId}";
                stringBuilder.Add(strQuery);

                stringBuilder.Add($"UPDATE receiptdetails{nMainBranchId} pm SET taxid = tx.taxid FROM tax tx WHERE tx.taxpercent = pm.taxpers AND pm.branchid = {nBranchId} and pm.mainbranchid = {nMainBranchId}");

                strQuery = $"update receiptdetails{nMainBranchId} rsub set receiptid =  rm.receiptid from receiptmain{nMainBranchId} rm where rm.paytermsid = rsub.priceid";
                strQuery += $"\n and rsub.branchid = rm.branchid and rsub.mainbranchid = rm.mainbranchid and rsub.receiptno=rm.receiptno";
                strQuery += $"\n and rm.branchid ={nBranchId}    and rm.mainbranchid ={nMainBranchId}";
                stringBuilder.Add(strQuery);

                //store
                stringBuilder.Add($"UPDATE store{nMainBranchId} isub SET productid = pm.productid FROM productmain{nMainBranchId} pm WHERE pm.tempid = isub.productid  AND  isub.branchid = {nBranchId} and isub.mainbranchid = {nMainBranchId} and pm.producttype='product'");
                stringBuilder.Add($"UPDATE store{nMainBranchId} im SET acid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = im.acid AND im.branchid = {nBranchId} and im.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"UPDATE store{nMainBranchId} im SET godownid = g.godownid FROM godown g WHERE g.tempid = im.godownid and im.godownid > 0 AND im.branchid = {nBranchId} and im.mainbranchid = {nMainBranchId}");
                strQuery = $"update store{nMainBranchId} st set receiptid =  rsub.receiptid from receiptdetails{nMainBranchId} rsub where rsub.priceid = st.receiptid";
                strQuery += $"\n and st.branchid = rsub.branchid and st.mainbranchid = rsub.mainbranchid and st.receiptno=rsub.receiptno and st.batchslno=rsub.batchslno";
                strQuery += $"\n and st.productid = rsub.productid and rsub.branchid ={nBranchId}    and rsub.mainbranchid ={nMainBranchId}";
                stringBuilder.Add(strQuery);

                stringBuilder.Add($"UPDATE store{nMainBranchId} pm SET taxid = tx.taxid FROM tax tx WHERE tx.taxpercent = pm.taxpers AND pm.branchid = {nBranchId} and pm.mainbranchid = {nMainBranchId}");


                stringBuilder.Add($"update receiptmain{nMainBranchId}    set paytermsid = 0  where branchid = {nBranchId} and mainbranchid = {nMainBranchId};");
                stringBuilder.Add($"update receiptdetails{nMainBranchId} set priceid    = 0  where branchid = {nBranchId} and mainbranchid = {nMainBranchId}");



                ///opening stock main
                stringBuilder.Add($"UPDATE openingstockmain{nMainBranchId} os SET acid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = os.acid AND os.branchid = {nBranchId} and os.mainbranchid = {nMainBranchId}");


                ///opening stock details
                stringBuilder.Add($"UPDATE openingstockdetails{nMainBranchId} os SET productid = pm.productid FROM productmain{nMainBranchId} pm WHERE pm.tempid = os.productid AND  os.branchid = {nBranchId} AND  os.mainbranchid = {nMainBranchId} and pm.producttype='product'");
                stringBuilder.Add($"UPDATE openingstockdetails{nMainBranchId} pm SET taxid = tx.taxid FROM tax tx WHERE tx.taxpercent = pm.taxpers AND pm.branchid = {nBranchId} and pm.mainbranchid = {nMainBranchId}");
                strQuery = $"update openingstockdetails{nMainBranchId} rsub set openingstockid =  rm.openingstockid from openingstockmain{nMainBranchId} rm where rm.paytermsid = rsub.dcinno";
                strQuery += $"\n and rsub.branchid = rm.branchid and rsub.mainbranchid = rm.mainbranchid and rsub.openingstockno=rm.openingstockno";
                strQuery += $"\n and rm.branchid ={nBranchId}    and rm.mainbranchid ={nMainBranchId}";
                stringBuilder.Add(strQuery);

                stringBuilder.Add($"update openingstockmain{nMainBranchId}    set paytermsid = 0  where branchid = {nBranchId} and mainbranchid = {nMainBranchId};");
                stringBuilder.Add($"update openingstockdetails{nMainBranchId}    set dcinno = 0  where branchid = {nBranchId} and mainbranchid = {nMainBranchId};");


                strQuery = $"update openingstockmain{nMainBranchId} erm set billserid = bs.billserid from billseries bs";
                strQuery += $"\n  where bs.branchid = {nBranchId} and bs.mainbranchid = {nMainBranchId} and billsersource  = 'OPENING STOCK ENTRY'";
                strQuery += $"\n  and erm.branchid = {nBranchId} and erm.mainbranchid = {nMainBranchId} and  bs.branchid = {nBranchId} and bs.mainbranchid = {nMainBranchId}";
                stringBuilder.Add(strQuery);

                strQuery = $"update openingstockdetails{nMainBranchId} erm set billserid = bs.billserid from billseries bs";
                strQuery += $"\n  where bs.branchid = {nBranchId} and bs.mainbranchid = {nMainBranchId} and billsersource  = 'OPENING STOCK ENTRY'";
                strQuery += $"\n  and erm.branchid = {nBranchId} and erm.mainbranchid = {nMainBranchId} and  bs.branchid = {nBranchId} and bs.mainbranchid = {nMainBranchId}";
                stringBuilder.Add(strQuery);

                /// receipt return main
                stringBuilder.Add($"UPDATE receiptreturnmain{nMainBranchId} rm SET staffid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.staffid AND rm.branchid = {nBranchId} and rm.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"UPDATE receiptreturnmain{nMainBranchId} rm SET reasonid = ah.categoryid FROM category ah WHERE ah.tempid = rm.reasonid AND rm.branchid = {nBranchId} and rm.mainbranchid = {nMainBranchId}");

                stringBuilder.Add($"UPDATE receiptreturndetails{nMainBranchId} rm SET staffid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.staffid AND rm.branchid = {nBranchId} and rm.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"UPDATE receiptreturndetails{nMainBranchId} os SET productid = pm.productid FROM productmain{nMainBranchId} pm WHERE pm.tempid = os.productid  AND  os.branchid = {nBranchId} and os.mainbranchid = {nMainBranchId}  and pm.producttype='product'");
                stringBuilder.Add($"UPDATE receiptreturndetails{nMainBranchId} pm SET taxid = tx.taxid FROM tax tx WHERE tx.taxpercent = pm.receiptsubtaxpers AND pm.branchid = {nBranchId} and pm.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"UPDATE receiptreturndetails{nMainBranchId} rm SET reasonid = ah.categoryid FROM category ah WHERE ah.tempid = rm.reasonid AND rm.branchid = {nBranchId} and rm.mainbranchid = {nMainBranchId}");

                strQuery = $"update receiptreturndetails{nMainBranchId} rsub set receiptreturnmainid =  rm.receiptreturnmainid from receiptreturnmain{nMainBranchId} rm where rm.billserid = rsub.receiptreturnmainid";
                strQuery += $"\n and rsub.branchid = rm.branchid and rsub.mainbranchid = rm.mainbranchid";
                strQuery += $"\n and rm.branchid ={nBranchId}    and rm.mainbranchid ={nMainBranchId}";
                stringBuilder.Add(strQuery);

                strQuery = $"update receiptreturndetails{nMainBranchId} rsub set receiptreturnno =  rm.receiptreturnno from receiptreturnmain{nMainBranchId} rm where rm.receiptreturnmainid = rsub.receiptreturnmainid";
                strQuery += $"\n and rsub.branchid = rm.branchid and rsub.mainbranchid = rm.mainbranchid";
                strQuery += $"\n and rm.branchid ={nBranchId}    and rm.mainbranchid ={nMainBranchId}";
                stringBuilder.Add(strQuery);


                stringBuilder.Add($"update receiptreturnmain{nMainBranchId}    set billserid = 0  where branchid = {nBranchId} and mainbranchid = {nMainBranchId};");

                //debitnotemain
                stringBuilder.Add($"UPDATE debitnotemain{nMainBranchId} rm SET staffid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.staffid AND rm.branchid = {nBranchId} and rm.mainbranchid ={nMainBranchId}");
                stringBuilder.Add($"UPDATE debitnotemain{nMainBranchId} rm SET acid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.acid AND rm.branchid = {nBranchId} and rm.mainbranchid = {nMainBranchId}");
                // stringBuilder.Add($"UPDATE debitnotemain{nMainBranchId} rm SET entrytype='product' WHERE rm.branchid = {nBranchId} and rm.mainbranchid = {nMainBranchId} ");

                //debitnotedetails
                stringBuilder.Add($"UPDATE debitnotedetails{nMainBranchId} os SET productid = pm.productid FROM productmain{nMainBranchId} pm WHERE pm.tempid = os.productid AND  os.branchid = {nBranchId}  And os.mainbranchid = {nMainBranchId}  and pm.producttype='product'");
                strQuery = $"update debitnotedetails{nMainBranchId} rsub set debitnoteid =  rm.debitnoteid from debitnotemain{nMainBranchId} rm where rm.billserid = rsub.debitnoteid";
                strQuery += $"\n and rsub.branchid = rm.branchid and rsub.mainbranchid = rm.mainbranchid and rsub.debitnoteno=rm.debitnoteno";
                strQuery += $"\n and rm.branchid ={nBranchId}    and rm.mainbranchid ={nMainBranchId}";
                stringBuilder.Add(strQuery);

                stringBuilder.Add($"UPDATE debitnotedetails{nMainBranchId} pm SET taxid = tx.taxid FROM tax tx WHERE tx.taxpercent = pm.taxper AND pm.branchid = {nBranchId} and pm.mainbranchid = {nMainBranchId}");



                //expirydebitnotemain

                stringBuilder.Add($"UPDATE expirydebitnotemain{nMainBranchId} rm SET staffid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.staffid AND rm.branchid = {nBranchId} and rm.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"UPDATE expirydebitnotemain{nMainBranchId} rm SET acid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.acid AND rm.branchid = {nBranchId} and rm.mainbranchid = {nMainBranchId}");

                //expirydebitnotedetails
                stringBuilder.Add($"UPDATE expirydebitnotedetails{nMainBranchId} os SET productid = pm.productid FROM productmain{nMainBranchId} pm WHERE pm.tempid = os.productid AND  os.branchid = {nBranchId} AND os.mainbranchid = {nMainBranchId}  and pm.producttype='product'");
                strQuery = $"update expirydebitnotedetails{nMainBranchId} rsub set expirydebitnoteid =  rm.expirydebitnoteid from expirydebitnotemain{nMainBranchId} rm where rm.billserid = rsub.expirydebitnoteid";
                strQuery += $"\n and rsub.branchid = rm.branchid and rsub.mainbranchid = rm.mainbranchid and rsub.expirydebitnotemainno=rm.expirydebitnoteno";
                strQuery += $"\n and rm.branchid ={nBranchId}    and rm.mainbranchid ={nMainBranchId}";
                stringBuilder.Add(strQuery);

                stringBuilder.Add($"UPDATE expirydebitnotedetails{nMainBranchId} pm SET taxid = tx.taxid FROM tax tx WHERE tx.taxpercent = pm.taxper AND pm.branchid = {nBranchId}");

                strQuery = $"update expirydebitnotemain{nMainBranchId} erm set billserid = bs.billserid from billseries bs";
                strQuery += $"\n  where bs.branchid = {nBranchId} and bs.mainbranchid = {nMainBranchId} and billtype = 'EXPIRY/DAMAGE DEBITNOTE'";
                strQuery += $"\n  and erm.branchid = {nBranchId} and erm.mainbranchid = {nMainBranchId} and  bs.branchid = {nBranchId} and bs.mainbranchid = {nMainBranchId};";

                List<AccountIdMigration> accountIdMigrations = GetAccountIdMigrations();
                foreach (AccountIdMigration acidmig in accountIdMigrations)
                {
                    stringBuilder.Add($"UPDATE voucherdetails{nMainBranchId} vd SET acid     = {acidmig.PosgresAcId}  WHERE vd.acid    = {acidmig.AcId} and vd.acid    < 55 AND vd.branchid = {nBranchId} and vd.mainbranchid = {nMainBranchId}");
                    stringBuilder.Add($"UPDATE voucherdetails{nMainBranchId} vd SET revacid  = {acidmig.PosgresAcId}  WHERE vd.revacid = {acidmig.AcId} and vd.revacid < 55 AND vd.branchid = {nBranchId} and vd.mainbranchid = {nMainBranchId}");
                }

                /// voucherdetails
                stringBuilder.Add($"UPDATE voucherdetails{nMainBranchId} rm SET staffid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.staffid AND rm.branchid = {nBranchId} and rm.mainbranchid ={nMainBranchId}");
                stringBuilder.Add($"UPDATE voucherdetails{nMainBranchId} rm SET repid   = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.repid AND rm.branchid = {nBranchId} and rm.mainbranchid = {nMainBranchId}");

                stringBuilder.Add($"UPDATE voucherdetails{nMainBranchId} rm SET acid    = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.acid and rm.acid>55 AND rm.branchid = {nBranchId} and rm.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"UPDATE voucherdetails{nMainBranchId} rm SET revacid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.revacid and rm.revacid>55 AND rm.branchid = {nBranchId} and rm.mainbranchid = {nMainBranchId}");

                //  stringBuilder.Add($"UPDATE voucherdetails{nMainBranchId} rm SET acid    = -46 FROM accounthead{nMainBranchId} ah WHERE rm.acid=26 AND rm.branchid = {nBranchId} And rm.mainbranchid = {nMainBranchId}");
                //  stringBuilder.Add($"UPDATE voucherdetails{nMainBranchId} rm SET revacid = -46 FROM accounthead{nMainBranchId} ah WHERE rm.revacid =26  AND rm.branchid = {nBranchId} And rm.mainbranchid = {nMainBranchId}");


                strQuery = $"INSERT INTO vouchermain{nMainBranchId}(";
                strQuery += $"\n vouchermaindate, vprefixid, voucherno, uniquevoucherid, vouchergroupid, voucherprefix,";
                strQuery += $"\n chequeno, chequedate, voucheramt, bankname, acid, repid, staffid, vouchertime, remarks, enterdate, tdspers,";
                strQuery += $"\n tdsamt, transtype, bvouchercancel, branchid, mainbranchid, revacid, refno, headtype, balanceamt";
                strQuery += $"\n )";
                strQuery += $"\n select distinct     vd.voucherdate, vd.vprefixid, vd.voucherno, vd.uniquevoucherid,0 vouchergroupid, vd.voucherprefix,";
                strQuery += $"\n vd.chequeno, vd.chequedate, 0 voucheramt, vd.bankname, 0 acid, vd.repid, vd.staffid, vd.vouchertime, vd.remarks,";
                strQuery += $"\n vd.enterdate, vd.tdspers, ";
                strQuery += $"\n vd.tdsamt,'' transtype,False bvouchercancel, vd.branchid, vd.mainbranchid, 0 revacid, vd.refno,'' headtype, vd.balanceamt";
                strQuery += $"\n from voucherdetails{nMainBranchId} vd where vd.vprefixid in (1,2,3,4,7,8,10) and vd.branchid={nBranchId} and vd.mainbranchid={nMainBranchId};";
                stringBuilder.Add(strQuery);

                strQuery = $" update vouchermain{nMainBranchId} vm set voucheramt = vd.voucheramt,acid = vd.acid,revacid = vd.revacid from voucherdetails{nMainBranchId} vd WHERE";
                strQuery += $"\n vm.vprefixid = vd.vprefixid and vm.voucherno = vd.voucherno";
                strQuery += $"\n and vm.uniquevoucherid = vd.uniquevoucherid and vm.branchid=vd.branchid and vm.mainbranchid=vd.mainbranchid";
                strQuery += $"\n and vd.voucheramt > 0 and vd.vprefixid in (1, 3) and vd.branchid={nBranchId} and vd.mainbranchid={nMainBranchId};";
                stringBuilder.Add(strQuery);


                strQuery = $" update vouchermain{nMainBranchId} vm set voucheramt = vd.voucheramt,acid = vd.acid,revacid = vd.revacid from voucherdetails{nMainBranchId} vd WHERE";
                strQuery += $"\n vm.vprefixid = vd.vprefixid and vm.voucherno = vd.voucherno";
                strQuery += $"\n and vm.uniquevoucherid = vd.uniquevoucherid and vm.branchid=vd.branchid and vm.mainbranchid=vd.mainbranchid";
                strQuery += $"\n and vd.voucheramt > 0 and vd.vprefixid in (1, 3) and vd.branchid={nBranchId} and vd.mainbranchid={nMainBranchId};";
                stringBuilder.Add(strQuery);


                strQuery = $" update vouchermain{nMainBranchId} vm set voucheramt = vd.voucheramt,acid = vd.acid,revacid = vd.revacid from voucherdetails{nMainBranchId} vd WHERE";
                strQuery += $"\n vm.vprefixid = vd.vprefixid and vm.voucherno = vd.voucherno";
                strQuery += $"\n and vm.uniquevoucherid = vd.uniquevoucherid and vm.branchid=vd.branchid and vm.mainbranchid=vd.mainbranchid";
                strQuery += $"\n and vd.voucheramt < 0 and vd.vprefixid in (2, 4) and vd.branchid={nBranchId} and vd.mainbranchid={nMainBranchId};";
                stringBuilder.Add(strQuery);


                strQuery = $" update vouchermain{nMainBranchId} vm set voucheramt = vd.voucheramt,acid = vd.acid,revacid = vd.acid from voucherdetails{nMainBranchId} vd WHERE";
                strQuery += $"\n vm.vprefixid = vd.vprefixid and vm.voucherno = vd.voucherno";
                strQuery += $"\n and vm.uniquevoucherid = vd.uniquevoucherid and vm.branchid=vd.branchid and vm.mainbranchid=vd.mainbranchid";
                strQuery += $"\n and vd.vprefixid in (10) and vd.branchid={nBranchId} and vd.mainbranchid={nMainBranchId};";
                stringBuilder.Add(strQuery);


                strQuery = $" update vouchermain{nMainBranchId} vm set revacid = acid  WHERE  vm.vprefixid in (10) and vm.branchid={nBranchId} and vm.mainbranchid={nMainBranchId};";
                stringBuilder.Add(strQuery);

                strQuery = $"update vouchermain{nMainBranchId} os    set headtype = 'Customer'  where vprefixid=10 and branchid = {nBranchId} and mainbranchid = {nMainBranchId}";
                strQuery += $" and os.acid in (select acid from accounthead{nMainBranchId} where customerflag=true and branchid={nBranchId});";
                stringBuilder.Add(strQuery);

                strQuery = $"update vouchermain{nMainBranchId} os    set headtype = 'Supplier'  where vprefixid=10 and branchid = {nBranchId} and mainbranchid = {nMainBranchId}";
                strQuery += $" and os.acid in (select acid from accounthead{nMainBranchId} where supplierflag=true and branchid={nBranchId});";
                stringBuilder.Add(strQuery);

                //voucheraccoutnheadupdate
                stringBuilder.Add($"UPDATE vouchermain{nMainBranchId} rm SET acid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.acid and rm.acid>55 AND rm.branchid = {nBranchId} and rm.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"UPDATE vouchermain{nMainBranchId} rm SET revacid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.revacid and rm.revacid>55 AND rm.branchid = {nBranchId} and rm.mainbranchid = {nMainBranchId}");


                //returnadjustmentlog
                stringBuilder.Add($" UPDATE returnadjustmentlog{nMainBranchId} rm SET acid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.acid and rm.acid>55 AND rm.branchid = {nBranchId} and rm.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($" update returnadjustmentlog{nMainBranchId} rm set postflag = 'SalesReturn'  where rm.postflag = 'Return' AND rm.branchid = {nBranchId} and rm.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($" update returnadjustmentlog{nMainBranchId} rm set postflag = 'ExpiryReturn' where rm.postflag = 'Expiry' AND rm.branchid = {nBranchId} and rm.mainbranchid = {nMainBranchId}");

                stringBuilder.Add($" update returnadjustmentlog{nMainBranchId} rm set fromsource = 'Sales' where (rm.postflag = 'SalesReturn' or rm.postflag = 'ExpiryReturn' ) AND rm.branchid = {nBranchId} and rm.mainbranchid = {nMainBranchId}");

                //accountlogfile
                stringBuilder.Add($"UPDATE accountlogfile{nMainBranchId} rm SET acid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.acid and rm.acid>55 AND rm.branchid = {nBranchId} and rm.mainbranchid = {nMainBranchId}");

                //chequeentry
                stringBuilder.Add($"UPDATE chequeentry{nMainBranchId} rm SET staffid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.staffid AND rm.branchid = {nBranchId} and rm.mainbranchid= {nMainBranchId}");

                stringBuilder.Add($"UPDATE chequeentry{nMainBranchId} rm SET acid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.acid and rm.acid>55 AND rm.branchid = {nBranchId} And rm.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"UPDATE chequeentry{nMainBranchId} rm SET recid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.recid and rm.recid>55 AND rm.branchid = {nBranchId} And rm.mainbranchid = {nMainBranchId}");

                //outstanding
                stringBuilder.Add($"UPDATE outstanding{nMainBranchId} rm SET billserid = bs.billserid FROM billseries bs WHERE bs.tempid = rm.billserid AND rm.branchid = {nBranchId} and rm.mainbranchid = {nMainBranchId} and rm.billserid<>0 and  bs.branchid = {nBranchId} and bs.mainbranchid = {nMainBranchId} and rm.vprefixid=5 and bs.billsersource='SALES' ");
                stringBuilder.Add($"UPDATE outstanding{nMainBranchId} rm SET billserid = bs.billserid FROM billseries bs WHERE bs.tempid = rm.billserid AND rm.branchid = {nBranchId} and rm.mainbranchid = {nMainBranchId} and rm.billserid<>0 and  bs.branchid = {nBranchId} and bs.mainbranchid = {nMainBranchId} and rm.vprefixid=6 and bs.billsersource='PURCHASE' ");
                stringBuilder.Add($"UPDATE outstanding{nMainBranchId} rm SET acid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.acid and rm.acid>55 AND rm.branchid = {nBranchId} and rm.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"UPDATE outstanding{nMainBranchId} rm SET salesmanid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.salesmanid AND rm.branchid = {nBranchId} and rm.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"update outstanding{nMainBranchId}    set sourcetype = 'Sales'  where vprefixid=5 and branchid = {nBranchId} and mainbranchid = {nMainBranchId};");
                stringBuilder.Add($"update outstanding{nMainBranchId}    set sourcetype = 'Purchase'  where vprefixid=6 and branchid = {nBranchId} and mainbranchid = {nMainBranchId};");

                strQuery = $"update outstanding{nMainBranchId} os    set sourcetype = 'Sales'  where vprefixid=10 and branchid = {nBranchId} and mainbranchid = {nMainBranchId}";
                strQuery += $" and os.acid in (select acid from accounthead{nMainBranchId} where customerflag=true and branchid={nBranchId});";
                stringBuilder.Add(strQuery);
                strQuery = $"update outstanding{nMainBranchId} os    set sourcetype = 'Purchase'  where vprefixid=10 and branchid = {nBranchId} and mainbranchid = {nMainBranchId}";
                strQuery += $" and os.acid in (select acid from accounthead{nMainBranchId} where supplierflag=true  and branchid={nBranchId});";
                stringBuilder.Add(strQuery);
                //issuereturnmain

                stringBuilder.Add($"UPDATE issuereturnmain{nMainBranchId} im SET acid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = im.acid AND im.branchid = {nBranchId} AND im.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"UPDATE issuereturnmain{nMainBranchId} im SET salesexeid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = im.salesexeid AND im.branchid = {nBranchId} AND im.mainbranchid = {nMainBranchId} ");
                stringBuilder.Add($"UPDATE issuereturnmain{nMainBranchId} im SET staffid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = im.staffid AND im.branchid = {nBranchId} AND im.mainbranchid = {nMainBranchId}");

                strQuery = $"update issuereturnmain{nMainBranchId} im set salesbillserid =  bs.billserid from billseries bs where bs.tempid = im.salesbillserid";
                strQuery += $"\n and bs.branchid = im.branchid and bs.mainbranchid = im.mainbranchid";
                strQuery += $"\n and im.branchid ={nBranchId}    and im.mainbranchid ={nMainBranchId} and bs.billsersource='CREDIT NOTE'  AND bs.branchid = {nBranchId} and bs.mainbranchid = {nMainBranchId}";
                stringBuilder.Add(strQuery);

                strQuery = $"update issuereturnmain{nMainBranchId} erm set billserid = bs.billserid from billseries bs";
                strQuery += $"\n  where bs.branchid = {nBranchId} and bs.mainbranchid = {nMainBranchId} and billtype = 'CREDIT NOTE'";
                strQuery += $"\n  and erm.branchid = {nBranchId} and erm.mainbranchid = {nMainBranchId}  AND bs.branchid = {nBranchId} and bs.mainbranchid = {nMainBranchId};";
                stringBuilder.Add(strQuery);

                //issuereturndetails
                stringBuilder.Add($"UPDATE issuereturndetails{nMainBranchId} pm SET taxid = tx.taxid FROM tax tx WHERE tx.taxpercent = pm.taxpers AND pm.branchid = {nBranchId} AND pm.mainbranchid = {nMainBranchId}");

                stringBuilder.Add($"update issuereturndetails{nMainBranchId} set totqty = qty + freqty + advfre where branchid = {nBranchId} AND mainbranchid ={nMainBranchId}");
                stringBuilder.Add($"UPDATE issuereturndetails{nMainBranchId} isub SET productid = pm.productid FROM productmain{nMainBranchId} pm WHERE pm.tempid = isub.productid AND  isub.branchid = {nBranchId} AND isub.mainbranchid = {nMainBranchId}  and pm.producttype='product'");

                strQuery = $"update issuereturndetails{nMainBranchId} im set salesbillserid =  bs.billserid from billseries bs where bs.tempid = im.salesbillserid";
                strQuery += $"\n and bs.branchid = im.branchid and bs.mainbranchid = im.mainbranchid";
                strQuery += $"\n and im.branchid ={nBranchId}    and im.mainbranchid ={nMainBranchId} and bs.billsersource='SALES'  and  bs.branchid = {nBranchId} and bs.mainbranchid = {nMainBranchId}";
                stringBuilder.Add(strQuery);
                strQuery = $"update issuereturndetails{nMainBranchId} ird set issuereturnid =  irm.issuereturnid from issuereturnmain{nMainBranchId} irm where ird.uniquereturnno = irm.uniquereturnno";
                strQuery += $"\n and ird.branchid = irm.branchid and ird.mainbranchid = irm.mainbranchid";
                strQuery += $"\n and ird.branchid ={nBranchId}    and ird.mainbranchid ={nMainBranchId} ";

                stringBuilder.Add(strQuery);

                //expiryreturnmain

                stringBuilder.Add($"UPDATE expiryreturnmain{nMainBranchId} im SET acid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = im.acid AND im.branchid = {nBranchId} AND im.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"UPDATE expiryreturnmain{nMainBranchId} im SET salesexeid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = im.salesexeid AND im.branchid = {nBranchId} AND im.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"UPDATE expiryreturnmain{nMainBranchId} im SET staffid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = im.staffid AND im.branchid = {nBranchId} AND im.mainbranchid ={nMainBranchId}");
                //strQuery = $"update issuereturnmain{nMainBranchId} im set billserid =  bs.billserid from billseries bs where bs.tempid = im.billserid";
                //strQuery += $"\n and bs.branchid = im.branchid and bs.mainbranchid = im.mainbranchid";
                //strQuery += $"\n and im.branchid ={nBranchId}    and im.mainbranchid ={nMainBranchId} and bs.billsersource='SALES'";
                //stringBuilder.Add(strQuery);


                //expiryreturndetails
                stringBuilder.Add($"UPDATE expiryreturndetails{nMainBranchId} pm SET taxid = tx.taxid FROM tax tx WHERE tx.taxpercent = pm.taxpers AND pm.branchid = {nBranchId} AND pm.mainbranchid ={nMainBranchId}");
                //strQuery = $"update issuesubdetails{nMainBranchId} im set billserid =  bs.billserid from billseries bs where bs.tempid = im.billserid";
                //strQuery += $"\n and bs.branchid = im.branchid and bs.mainbranchid = im.mainbranchid";
                //strQuery += $"\n and im.branchid ={nBranchId}    and im.mainbranchid ={nMainBranchId} and bs.billsersource='SALES'";
                //stringBuilder.Add(strQuery);
                stringBuilder.Add($"UPDATE expiryreturndetails{nMainBranchId} isub SET productid = pm.productid FROM productmain{nMainBranchId} pm WHERE pm.tempid = isub.productid AND  isub.branchid = {nBranchId}  AND isub.mainbranchid = {nMainBranchId}  and pm.producttype='product'");
                stringBuilder.Add($"update expiryreturndetails{nMainBranchId} set totqty = qty + freqty + advfre where branchid = {nBranchId} and mainbranchid = {nMainBranchId}");

                strQuery = $"update expiryreturndetails{nMainBranchId} rsub set expiryreturnid =  rm.expiryreturnid from expiryreturnmain{nMainBranchId} rm where rm.billserid = rsub.billserid";
                strQuery += $"\n and rsub.branchid = rm.branchid and rsub.mainbranchid = rm.mainbranchid and rsub.expiryreturnno=rm.expiryreturnno";
                strQuery += $"\n and rm.branchid ={nBranchId}    and rm.mainbranchid ={nMainBranchId}";
                stringBuilder.Add(strQuery);

                strQuery = $"update expiryreturnmain{nMainBranchId} erm set billserid = bs.billserid from billseries bs";
                strQuery += $"\n where bs.branchid = {nBranchId} and bs.mainbranchid = {nMainBranchId} and billtype = 'EXPIRY RETURN' and erm.branchid = {nBranchId} and erm.mainbranchid = {nMainBranchId}";
                strQuery += $"\n  and bs.branchid = {nBranchId} and bs.mainbranchid = {nMainBranchId};";
                stringBuilder.Add(strQuery);

                strQuery = $"update expiryreturndetails{nMainBranchId} erm set billserid = bs.billserid from billseries bs";
                strQuery += $"\n where bs.branchid = {nBranchId} and bs.mainbranchid = {nMainBranchId} and billtype = 'EXPIRY RETURN' and erm.branchid = {nBranchId} and erm.mainbranchid = {nMainBranchId}";
                strQuery += $"\n  and bs.branchid = {nBranchId} and bs.mainbranchid = {nMainBranchId};";
                stringBuilder.Add(strQuery);

                //EInvoice
                strQuery = $"update einvoicedetails{nMainBranchId} im set billserid =  bs.billserid from billseries bs where bs.tempid = im.billserid";
                strQuery += $"\n and bs.branchid = im.branchid and bs.mainbranchid = im.mainbranchid";
                strQuery += $"\n and im.fromtype= 'SALES'  and im.branchid ={nBranchId}    and im.mainbranchid ={nMainBranchId} and bs.billsersource='SALES'";
                strQuery += $"\n  and bs.branchid = {nBranchId} and bs.mainbranchid = {nMainBranchId};";
                stringBuilder.Add(strQuery);

                //DeliveryOutMain
                strQuery = $"update deliveryoutmain{nMainBranchId} dom set billserid =  bs.billserid from billseries bs where bs.tempid = dom.billserid";
                strQuery += $"\n and bs.branchid = dom.branchid and bs.mainbranchid = dom.mainbranchid";
                strQuery += $"\n and dom.branchid ={nBranchId}    and dom.mainbranchid ={nMainBranchId} and bs.billsersource='DELIVERYOUT'";
                strQuery += $"\n  and bs.branchid = {nBranchId} and bs.mainbranchid = {nMainBranchId};";
                stringBuilder.Add(strQuery);


                //DeliveryOutDetails
                strQuery = $"update deliveryoutdetails{nMainBranchId} dos set billserid =  bs.billserid from billseries bs where bs.tempid = dos.billserid";
                strQuery += $"\n and bs.branchid = dos.branchid and bs.mainbranchid = dos.mainbranchid";
                strQuery += $"\n and dos.branchid ={nBranchId} and dos.mainbranchid ={nMainBranchId} and bs.billsersource='DELIVERYOUT'";
                strQuery += $"\n  and bs.branchid = {nBranchId} and bs.mainbranchid = {nMainBranchId};";
                stringBuilder.Add(strQuery);
                stringBuilder.Add($"UPDATE deliveryoutdetails{nMainBranchId} isub SET productid = pm.productid FROM productmain{nMainBranchId} pm WHERE pm.tempid = isub.productid AND  isub.branchid = {nBranchId}  AND isub.mainbranchid = {nMainBranchId}  and pm.producttype='product'");
                stringBuilder.Add($"update deliveryoutdetails{nMainBranchId} set totqty = qty + freqty + advfre where branchid = {nBranchId} and mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"UPDATE deliveryoutdetails{nMainBranchId} pm SET taxid = tx.taxid FROM tax tx WHERE tx.taxpercent = pm.taxpers AND pm.branchid = {nBranchId} AND pm.mainbranchid ={nMainBranchId}");

                //ExpenseEntryDetails
                strQuery = $"update expenseentrydetails{nMainBranchId} eed set expensemainid =  eem.entrymainid from expenseentrymain eem where eem.tempid = eed.expensemainid";
                strQuery += $"\n and eed.branchid = eem.branchid and eed.mainbranchid = eem.mainbranchid";

                strQuery = $" UPDATE category";
                strQuery += $"\n SET categorytypeid = CASE categorytypeid";
                strQuery += $"\n WHEN 13 THEN 2";
                strQuery += $"\n WHEN 11  THEN 4";//unit
                //strQuery += $"\n WHEN 9  THEN 7";
                //strQuery += $"\n WHEN 14 THEN 5";
                //strQuery += $"\n WHEN 4  THEN 11";
                //strQuery += $"\n WHEN 5  THEN 9";
                //strQuery += $"\n WHEN 6  THEN 10";
                //strQuery += $"\n WHEN 7  THEN 12";
                //strQuery += $"\n WHEN 15 THEN 12";
                strQuery += $"\n ELSE categorytypeid";
                strQuery += $"\n END";
                // strQuery += $"\n WHERE categorytypeid >0";
                strQuery += $"\n where tempid<> 0";
                strQuery += $"\n AND branchid = {nBranchId}";
                strQuery += $"\n AND mainbranchid = {nMainBranchId};";
                stringBuilder.Add(strQuery);


                strQuery = $" UPDATE categoryhead";
                strQuery += $"\n SET headtypeid = CASE headtypeid";
                strQuery += $"\n WHEN 15 THEN 6";
                // strQuery += $"\n WHEN 11  THEN 4";//unit
                //strQuery += $"\n WHEN 9  THEN 7";
                //strQuery += $"\n WHEN 14 THEN 5";
                //strQuery += $"\n WHEN 4  THEN 11";
                //strQuery += $"\n WHEN 5  THEN 9";
                //strQuery += $"\n WHEN 6  THEN 10";
                //strQuery += $"\n WHEN 7  THEN 12";
                //strQuery += $"\n WHEN 15 THEN 12";
                strQuery += $"\n ELSE headtypeid";
                strQuery += $"\n END";
                // strQuery += $"\n WHERE categorytypeid >0";
                strQuery += $"\n where tempid<> 0";
                strQuery += $"\n AND branchid = {nBranchId}";
                strQuery += $"\n AND mainbranchid = {nMainBranchId};";
                stringBuilder.Add(strQuery);

                stringBuilder.Add($"UPDATE category ct SET categoryhead_id = ch.categoryheadid FROM categoryhead ch WHERE ct.categoryhead_id = ch.tempid and ch.branchid={nBranchId} AND ct.branchid = {nBranchId} and ct.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"UPDATE restotabledetails isub SET tableid = pm.tableid FROM restotable pm WHERE pm.tempid = isub.tableid AND isub.branchid = {nBranchId} and isub.mainbranchid = {nMainBranchId}");

                //              { id: 1, categorytype: 'Product' },
                //{ id: 2, categorytype: 'Reason' },
                //{ id: 3, categorytype: 'Customer' },
                //{ id: 4, categorytype: 'Unit' },
                //{ id: 5, categorytype: 'AreaGroup' },
                //{ id: 6, categorytype: 'CompGroup' },
                //{ id: 7, categorytype: 'AccountHead' },
                //{ id: 8, categorytype: 'Schedule' },
                //{ id: 9, categorytype: 'Media' },
                //{ id: 10, categorytype: 'Bank' },
                //{ id: 11, categorytype: 'Agent' },
                //{ id: 12, categorytype: 'Notes' }


                strQuery = $"\n update store{nMainBranchId} st";
                strQuery += $"\n set stkbilledqty = st.stkbilledqty + agg.totqty";
                strQuery += $"\n from(";
                strQuery += $"\n select isub.productid, isub.branchid, isub.mainbranchid,";
                strQuery += $"\n sum(isub.qty + isub.freqty + isub.advfre) as totqty,isub.batchslno";
                strQuery += $"\n from issuemain{nMainBranchId} im    inner    join issuesubdetails{nMainBranchId} isub  on im.billserid = isub.billserid";
                strQuery += $"\n and im.issueno = isub.issueno       and im.uniquebillno = isub.uniquebillno       and im.branchid = isub.branchid";
                strQuery += $"\n and im.mainbranchid = isub.mainbranchid    where im.branchid = {nBranchId} and im.mainbranchid = {nMainBranchId} and COALESCE(im.issuecancel,'No') <> 'Yes'";
                strQuery += $"\n group by isub.productid, isub.branchid, isub.mainbranchid,isub.batchslno";
                strQuery += $"\n ) agg";
                strQuery += $"\n where st.productid = agg.productid  and st.branchid = agg.branchid  and st.mainbranchid = agg.mainbranchid and st.batchslno = agg.batchslno;";
                stringBuilder.Add(strQuery);

                strQuery = $"\n update store{nMainBranchId} st";
                strQuery = $"\n set stkbilledqty = st.stkbilledqty + agg.totqty";
                strQuery = $"\n from(";
                strQuery = $"\n select isub.productid, isub.branchid, isub.mainbranchid,";
                strQuery = $"\n sum(stockreturn) as totqty, isub.batchslno";
                strQuery = $"\n from receiptreturnmain{nMainBranchId} im    inner";
                strQuery = $"\n join receiptreturndetails{nMainBranchId} isub  on im.receiptreturnmainid = isub.receiptreturnmainid";
                strQuery = $"\n and im.receiptreturnno = isub.receiptreturnno   and im.branchid = isub.branchid";
                strQuery = $"\n and im.mainbranchid = isub.mainbranchid    where im.branchid = {nBranchId} and im.mainbranchid = {nMainBranchId} and im.returncancel<> true";
                strQuery = $"\n group by isub.productid, isub.branchid, isub.mainbranchid, isub.batchslno";
                strQuery = $"\n ) agg";
                strQuery = $"\n where st.productid = agg.productid  and st.branchid = agg.branchid  and st.mainbranchid = agg.mainbranchid and st.batchslno = agg.batchslno;";

                stringBuilder.Add($"UPDATE issuemain{nMainBranchId} im SET doctid = dc.doctorid FROM doctor dc WHERE im.doctid = dc.tempid AND dc.branchid = {nBranchId} and dc.mainbranchid = {nMainBranchId}");

                stringBuilder.Add($"update stocktransfermain{nMainBranchId} st set tobranch = br.branchid from branch br where st.billserid = br.tempid and frombranch={nBranchId};");

                stringBuilder.Add($"update accountheadsub ahs set acid = ah.acid from accounthead{nMainBranchId} ah  where ahs.acid = ah.tempid and ahs.branchid={nBranchId} and ah.branchid={nBranchId};");
                stringBuilder.Add($"update accountheadsub ahs set doctorid = d.doctorid from doctor d  where ahs.doctorid = d.tempid and ahs.branchid={nBranchId} and d.branchid={nBranchId};");
                stringBuilder.Add($"update accountheadsub ahs set bloodgroupid = b.bloodgroupid from bloodgroup b  where ahs.bloodgroupid = b.tempid and ahs.branchid={nBranchId} and b.branchid={nBranchId};");
                stringBuilder.Add($"update accountheadsub ahs set paytermsid = ah.acid from accounthead{nMainBranchId} ah  where ahs.paytermsid = ah.tempid and ah.bankflag = true and ah.upiflag =true and ahs.branchid={nBranchId} and ah.branchid={nBranchId};");
                stringBuilder.Add($"update accountheadsub ahs set paytermsid = -1 where ahs.paytermsid = 1 and ahs.branchid={nBranchId} ;");
                stringBuilder.Add($"update accountheadsub ahs set paytermsid = -2 where ahs.paytermsid = 2 and ahs.branchid={nBranchId} ;");
                stringBuilder.Add($"update accountheadsub ahs set paytermsid = -3 where ahs.paytermsid = 3 and ahs.branchid={nBranchId} ;");
                stringBuilder.Add($"update accountheadsub ahs set paytermsid = -4 where ahs.paytermsid = 4 and ahs.branchid={nBranchId} ;");

                stringBuilder.Add($"update doctor d set department_id = dt.dptid from department dt  where d.department_id = dt.tempid and dt.branchid={nBranchId} and d.branchid={nBranchId};");
                stringBuilder.Add($"update doctor d set specialistid = s.splid from specialist s  where d.specialistid = s.tempid and s.branchid={nBranchId} and d.branchid={nBranchId};");

                stringBuilder.Add($"update diseasesub d set diagnosisid = dg.diagnosisid from diagnosis dg  where d.diagnosisid = dg.tempid ;");

                stringBuilder.Add($"update revisiting r set visitdoctorid = d.doctorid from doctor d  where r.visitdoctorid = d.tempid and r.branchid={nBranchId} and d.branchid={nBranchId} ;");
                stringBuilder.Add($"update revisiting r set specialistid = s.splid from specialist s  where r.specialistid = s.tempid and r.branchid={nBranchId} and s.branchid={nBranchId} ;");
                stringBuilder.Add($"update revisiting r set acid = ah.acid from accounthead{nMainBranchId} ah  where r.acid = ah.tempid and r.branchid={nBranchId} and ah.branchid={nBranchId} ;");
                stringBuilder.Add($"UPDATE revisiting r SET visitstaffid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = r.visitstaffid AND r.branchid = {nBranchId} and r.mainbranchid = {nMainBranchId}");

                stringBuilder.Add($"update labbill l set revisitid = r.visitid from revisiting r  where l.revisitid = r.tempid and l.branchid={nBranchId} and r.branchid={nBranchId} ;");
                stringBuilder.Add($"update labbill l set hospitalid = h.hosid::text from hospital h  where l.hospitalid = h.tempid::text and l.branchid={nBranchId} and h.branchid={nBranchId} ;");
                stringBuilder.Add($"update labbill l set doctorid = d.doctorid from doctor d  where l.doctorid = d.tempid and l.branchid={nBranchId} and d.branchid={nBranchId} ;");
                stringBuilder.Add($"UPDATE labbill l SET staffid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = l.staffid AND l.branchid = {nBranchId} and l.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"UPDATE labbill l SET acid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = l.acid AND l.branchid = {nBranchId} and l.mainbranchid = {nMainBranchId}");

                stringBuilder.Add($"update labbillsub  lbs set labbillid = l.labbillid from labbill l  where lbs.labbillid = l.tempid and lbs.branchid={nBranchId} and l.branchid={nBranchId} ;");
                stringBuilder.Add($"update labbillsub lbs set departmentid = d.dptid::text from department d  where lbs.departmentid = d.tempid::text and lbs.branchid={nBranchId} and d.branchid={nBranchId} ;");
                stringBuilder.Add($"update labbillsub lbs set testid = t.testid from test t  where lbs.testid = t.tempid and lbs.branchid={nBranchId} and t.branchid={nBranchId} ;");

                stringBuilder.Add($"update test t set departmentid = d.dptid from department d  where t.departmentid = d.tempid and t.branchid={nBranchId} and d.branchid={nBranchId} ;");
                stringBuilder.Add($"update testsub ts set testid = t.testid from test t  where ts.testid = t.tempid and ts.branchid={nBranchId} and t.branchid={nBranchId} ;");

                stringBuilder.Add($"UPDATE testresult tr SET staffid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = tr.staffid AND tr.branchid = {nBranchId} and tr.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"update testresult  tr set labbillid = l.labbillid from labbill l  where tr.labbillid = l.tempid and tr.branchid={nBranchId} and l.branchid={nBranchId} ;");
                stringBuilder.Add($"UPDATE testresult tr SET acid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = tr.acid AND tr.branchid = {nBranchId} and tr.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"update testresult tr set doctorid = d.doctorid from doctor d  where tr.doctorid = d.tempid and tr.branchid={nBranchId} and d.branchid={nBranchId} ;");

                stringBuilder.Add($"update testresultsub trs set testresultid = tr.testresultid from testresult tr  where trs.testresultid = tr.tempid and trs.branchid={nBranchId} and tr.branchid={nBranchId} ;");
                stringBuilder.Add($"update testresultsub trs set testid = t.testid from test t  where trs.testid = t.tempid and trs.branchid={nBranchId} and t.branchid={nBranchId} ;");
                stringBuilder.Add($"update testresultsub trs set testsubid = ts.testsubid from testsub ts  where trs.testid = ts.tempid and trs.branchid={nBranchId} and ts.branchid={nBranchId} ;");

                stringBuilder.Add($"UPDATE appointmentdetails ad SET acid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = ad.acid AND ad.branchid = {nBranchId} and ad.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"update appointmentdetails ad set doctorid = d.doctorid from doctor d  where ad.doctorid = d.tempid and ad.branchid={nBranchId} and d.branchid={nBranchId} ;");

                stringBuilder.Add($"UPDATE pmrappointment pa SET acid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = pa.acid AND pa.branchid = {nBranchId} and pa.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"update pmrappointment pa set doctorid = d.doctorid from doctor d  where pa.doctorid = d.tempid and pa.branchid={nBranchId} and d.branchid={nBranchId} ;");

                stringBuilder.Add($"update pmrsheet ps set revisitid = r.visitid from revisiting r  where ps.revisitid = r.tempid and ps.branchid={nBranchId} and r.branchid={nBranchId} ;");
                stringBuilder.Add($"update pmrsheet ps set doctorid = d.doctorid from doctor d  where ps.doctorid = d.tempid and ps.branchid={nBranchId} and d.branchid={nBranchId} ;");
                stringBuilder.Add($"update pmrsheet ps set doctorid = d.doctorid from doctor d  where ps.doctorid = d.tempid and ps.branchid={nBranchId} and d.branchid={nBranchId} ;");
                stringBuilder.Add($"UPDATE pmrsheet ps SET staffid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = ps.staffid AND ps.branchid = {nBranchId} and ps.mainbranchid = {nMainBranchId}");

                stringBuilder.Add($"update pmrdiagnosis pd set pmruniquekey = ps.pmruniquekey from pmrsheet ps  where pd.pmruniquekey = ps.tempid and pd.branchid={nBranchId} and ps.branchid={nBranchId} ;");
                stringBuilder.Add($"update pmrdiagnosis pd set diagnosisid = d.diagnosisid from diagnosis d  where pd.diagnosisid = d.tempid and pd.branchid={nBranchId} and d.branchid={nBranchId} ;");

                stringBuilder.Add($"update pmrdiseases pd set pmruniquekey = ps.pmruniquekey from pmrsheet ps  where pd.pmruniquekey = ps.tempid and pd.branchid={nBranchId} and ps.branchid={nBranchId} ;");
                stringBuilder.Add($"update pmrdiseases pd set diseasesid = s.symptomsid from symptoms s  where pd.diseasesid = s.tempid and pd.branchid={nBranchId} and s.branchid={nBranchId} ;");

                stringBuilder.Add($"update pmrmedicine pm set pmruniquekey = ps.pmruniquekey from pmrsheet ps  where pm.pmruniquekey = ps.tempid and pm.branchid={nBranchId} and ps.branchid={nBranchId} ;");
                stringBuilder.Add($"update pmrmedicine pm set productid = ps.productid from productmain{nMainBranchId} ps  where pm.productid = ps.tempid and pm.branchid={nBranchId} and ps.branchid={nBranchId} ;");

                stringBuilder.Add($"update godownreturnmain{nMainBranchId} grm set godownid = g.godownid from godown g  where grm.godownid = g.tempid and grm.branchid={nBranchId} and g.branchid={nBranchId} ;");
                stringBuilder.Add($"update godownreturnmain{nMainBranchId} grm set staffid = ah.acid from accounthead{nMainBranchId} ah  where grm.staffid = ah.tempid and grm.branchid={nBranchId} and ah.branchid={nBranchId} ;");
                stringBuilder.Add($"update godownreturnmain{nMainBranchId} grm set acid = ah.acid from accounthead{nMainBranchId} ah  where grm.acid = ah.tempid and grm.branchid={nBranchId} and ah.branchid={nBranchId} ;");


                stringBuilder.Add($"update godownreturndetails{nMainBranchId} grd set godownreturnid = grm.godownreturnid from godownreturnmain{nMainBranchId} grm  where grd.godownreturnid = grm.tempid and grd.branchid={nBranchId} and grm.branchid={nBranchId} ;");
                stringBuilder.Add($"update godownreturndetails{nMainBranchId} grd set productid = pm.productid from productmain{nMainBranchId} pm  where grd.productid = pm.tempid and grd.branchid={nBranchId} and pm.branchid={nBranchId} ;");

                stringBuilder.Add($"update godowntransfermain{nMainBranchId} gtm set godownid = g.godownid from godown g  where gtm.godownid = g.tempid and gtm.branchid={nBranchId} and g.branchid={nBranchId} ;");
                stringBuilder.Add($"update godowntransfermain{nMainBranchId} gtm set staffid = ah.acid from accounthead{nMainBranchId} ah  where gtm.staffid = ah.tempid and gtm.branchid={nBranchId} and ah.branchid={nBranchId} ;");

                stringBuilder.Add($"update godowntransferdetails{nMainBranchId} gtd set godowntransferid = gtm.godowntransferid from godowntransfermain{nMainBranchId} gtm  where gtd.godowntransferid = gtm.tempid and gtd.branchid={nBranchId} and gtm.branchid={nBranchId} ;");
                stringBuilder.Add($"update godowntransferdetails{nMainBranchId} gtd set productid = pm.productid from productmain{nMainBranchId} pm  where gtd.productid = pm.tempid and gtd.branchid={nBranchId} and pm.branchid={nBranchId} ;");
               // stringBuilder.Add($"UPDATE godowntransferdetails{nMainBranchId} pm SET taxid = tx.taxid FROM tax tx WHERE tx.taxpercent = pm.taxpers AND pm.branchid = {nBranchId} and pm.mainbranchid = {nMainBranchId}");

                stringBuilder.Add($"update creditdebitnotedismain grm set acid = ah.acid from accounthead{nMainBranchId} ah  where grm.acid = ah.tempid and grm.branchid={nBranchId} and ah.branchid={nBranchId} ;");
                stringBuilder.Add($"update creditdebitnotedismain grm set salesmanid = ah.acid from accounthead{nMainBranchId} ah  where grm.acid = ah.tempid and salesmanflag = true and grm.branchid={nBranchId} and ah.branchid={nBranchId} ;");

                stringBuilder.Add($"update creditdebitnotedissub gtd set creditdebitnotemainid = gtm.creditdebitnotemainid from creditdebitnotedismain gtm  where gtd.creditdebitnotemainid = gtm.tempid and gtd.branchid={nBranchId} and gtm.branchid={nBranchId} ;");
                stringBuilder.Add($"update creditdebitnotedissub grm set headid = ah.acid from accounthead{nMainBranchId} ah  where grm.headid = ah.tempid and grm.branchid={nBranchId} and ah.branchid={nBranchId} ;");
                stringBuilder.Add($"update creditdebitnotedissub grm set hsnid = h.hsn_id from hsn{nMainBranchId} h  where grm.hsnid = h.tempid and grm.branchid={nBranchId} and h.branchid={nBranchId} ;");



                strQuery = $"update creditdebitnotedismain ird set uniquebillno =  irm.issuereturnid from issuereturnmain{nMainBranchId} irm where ird.uniquebillno = irm.uniquereturnno";
                strQuery += $"\n and ird.billno = irm.uniquereturnno and ird.branchid = irm.branchid and ird.mainbranchid = irm.mainbranchid";
                strQuery += $"\n and ird.branchid ={nBranchId}    and ird.mainbranchid ={nMainBranchId} and irm.branchid={nBranchId} and ird.vouchertype = 'CREDITNOTE'; ";
                stringBuilder.Add(strQuery);


                strQuery = $"update creditdebitnotedismain rsub set uniquebillno =  rm.debitnoteid from debitnotemain{nMainBranchId} rm where rm.billserid = rsub.uniquebillno";
                strQuery += $"\n and rsub.billno = rm.debitnoteno and rsub.branchid = rm.branchid and rsub.mainbranchid = rm.mainbranchid ";
                strQuery += $"\n and rm.branchid ={nBranchId}    and rm.mainbranchid ={nMainBranchId} and rsub.branchid={nBranchId} and rsub.vouchertype = 'DEBITNOTE'; ";
                stringBuilder.Add(strQuery);


                strQuery = $"update debitnotemain{nMainBranchId} erm set billserid = bs.billserid from billseries bs";
                strQuery += $"\n  where bs.branchid = {nBranchId} and bs.mainbranchid = {nMainBranchId} and billsersource  = 'DEBIT NOTE'";
                strQuery += $"\n  and erm.branchid = {nBranchId} and erm.mainbranchid = {nMainBranchId} and  bs.branchid = {nBranchId} and bs.mainbranchid = {nMainBranchId}";
                stringBuilder.Add(strQuery);

                strQuery = $"update issuereturnmain{nMainBranchId} set sourcefrom = 'Accountentry' where COALESCE(sourcefrom,'') = 'AccountsEntry';";
                stringBuilder.Add(strQuery);
                strQuery = $"update issuereturnmain{nMainBranchId} set sourcefrom = 'Productwise'  where COALESCE(sourcefrom,'') <> 'AccountsEntry' and issueno = 0;";
                stringBuilder.Add(strQuery);
                strQuery = $"update issuereturnmain{nMainBranchId} set sourcefrom = 'Billwise'     where COALESCE(sourcefrom,'') <> 'AccountsEntry' and issueno <> 0;";
                stringBuilder.Add(strQuery);


                strQuery = $"update issuereturnmain{nMainBranchId} irm set sourcefrom = 'Accountentry' where COALESCE(sourcefrom,'') = 'AccountsEntry' and irm.branchid= {nBranchId};";
                stringBuilder.Add(strQuery);

                strQuery = $"update debitnotemain{nMainBranchId} irm set entrytype = 'Productwise'  where COALESCE(entrytype,'') = '' and irm.branchid= {nBranchId};";
                stringBuilder.Add(strQuery);

                stringBuilder.Add($"UPDATE issuemain{nMainBranchId} im SET godownid = g .godownid FROM godown g WHERE im.godownid = g.tempid AND g.branchid = {nBranchId} and g.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"UPDATE issuesubdetails{nMainBranchId} isd SET godownid = g.godownid FROM godown g WHERE isd.godownid = g.tempid AND g.branchid = {nBranchId} and g.mainbranchid = {nMainBranchId}");


                strQuery = $"\n INSERT INTO public.issuereturndetails{nMainBranchId}(";
                strQuery += $"\n issuereturnid, issuereturnno, uniquereturnno, issuereturndate, salesbillserid, issueno, salesuniquebillno, issuedate,";
                strQuery += $"\n batch, expdate, originalrate, selrate, whrate, mrp, qtytype, qty, freqty, advfre, rqty, rfreqty, lqty, loosefree, totqty,";
                strQuery += $"\n taxpers, taxamt, itemdispers, amount, oldamount, productid, batchslno, batchslno1, amoutbefortax, flgspecialrate, actualrate,";
                strQuery += $"\n color, unit, taxid, itemdisamt, schmpers, schmamt, prodpack, pack, perrate, prodtype, amountbeforedis, adddispers, pricemenuid,";
                strQuery += $"\n inclusivesales, salesmanid, agentprice, purrate, orgpurrate, salesmanprice, rmrp, sprate1, sprate2, sprate3, sprate4, sprate5,";
                strQuery += $"\n pcsselrate, pcsmrp, pcswhrate, pcssprate1, pcssprate2, pcssprate3, pcssprate4, pcssprate5, sgsttaxpers, sgsttaxamount, sgstamount,";
                strQuery += $"\n cgsttaxpers, cgsttaxamount, cgstamount, igsttaxpers, igsttaxamount, igstamount, godownid, cesspers, cessamt, neethidispers, packageid,";
                strQuery += $"\n packageuniqueno, extracesspers, extracessamt, specialorgrate, extraschemeamt, addrateperunit, addrateunitamt, prodfrom, hsnid,";
                strQuery += $"\n hsncode, branchid, mainbranchid, acid, priceid";
                strQuery += $"\n )";
                strQuery += $"\n  select";
                strQuery += $"\n  uniquebillno issuereturnid,billno issuereturnno, billno uniquereturnno,billdate issuereturndate,0 salesbillserid,0 issueno,0 salesuniquebillno,billdate issuedate,";
                strQuery += $"\n   '' batch,billdate expdate, amount originalrate,amount selrate,0 whrate,0 mrp,'NOS' qtytype,1 qty,0 freqty,0 advfre,0 rqty,0 rfreqty,0 lqty,0 loosefree,1 totqty,";
                strQuery += $"\n  taxpers, taxamt,0 itemdispers,total amount,0 oldamount,0 productid,0 batchslno,0 batchslno1,0 amoutbefortax,0 flgspecialrate,amount actualrate,";
                strQuery += $"\n   '' color,'' unit,taxid taxid,0 itemdisamt,0 schmpers,0 schmamt,1 prodpack,1 pack,0 perrate,'' prodtype,0 amountbeforedis,0 adddispers,1 pricemenuid,";
                strQuery += $"\n  'No' inclusivesales,0 salesmanid,0 agentprice,0 purrate,0 orgpurrate,0 salesmanprice,0 rmrp,0 sprate1,0 sprate2,0 sprate3,0 sprate4,0 sprate5,";
                strQuery += $"\n   0 pcsselrate,0 pcsmrp,0 pcswhrate,0 pcssprate1,0 pcssprate2,0 pcssprate3,0 pcssprate4,0 pcssprate5, sgsttaxpers, sgsttaxamount,0 sgstamount,";
                strQuery += $"\n  cgsttaxpers, cgsttaxamount,0 cgstamount, igsttaxpers, igsttaxamount,0 igstamount,0 godownid,0 cesspers,0 cessamt,0 neethidispers,0 packageid,";
                strQuery += $"\n  0 packageuniqueno,0 extracesspers,0 extracessamt,0 specialorgrate,0 extraschemeamt,0 addrateperunit,0 addrateunitamt,'' prodfrom, hsnid,";
                strQuery += $"\n  hsncode, cm.branchid, cm.mainbranchid, acid,0 priceid";
                strQuery += $"\n  from creditdebitnotedissub cs inner join creditdebitnotedismain cm on cs.creditdebitnotemainid=cm.creditdebitnotemainid";
                strQuery += $"\n  and cm.branchid= {nBranchId} and cm.vouchertype = 'CREDITNOTE';";
                stringBuilder.Add(strQuery);


                strQuery = $"\n INSERT INTO public.debitnotedetails{nMainBranchId}(";
                strQuery += $"\n debitnoteid, debitnoteno, subdate, batchslno, batch, pack, qtytype, expdate, purrate, selrate, mrp, qty, freqty, totqty, amount,";
                strQuery += $"\n taxper, taxamt, itemdisc, itemdisamt, productid, taxid, landcost, receiptretid, receiptrettype, amountbeforetax, invono, invodate, sgsttaxpers,";
                strQuery += $"\n sgsttaxamount, sgstamount, cgsttaxpers, cgsttaxamount, cgstamount, igsttaxpers, igsttaxamount, igstamount, receiptid, receiptno, oldtaxamount,";
                strQuery += $"\n oldamount, purqty, cesspers, cessamt, totdis, totdisamt, debitfreqty, extracesspers, extracessamt, branchid, mainbranchid, accid, hsncode, hsnid";
                strQuery += $"\n )";
                strQuery += $"\n select uniquebillno debitnoteid,billno debitnoteno, billdate subdate,0 batchslno,'' batch,1 pack,'NOS' qtytype,billdate expdate,";
                strQuery += $"\n  0 purrate,amount selrate,0 mrp,1 qty,0 freqty,1 totqty,total amount,";
                strQuery += $"\n  cs.taxpers taxper, taxamt,0 itemdisc,0 itemdisamt, 0 productid, taxid,0 landcost,0 receiptretid,'' receiptrettype,0 amountbeforetax,'' invono,billdate invodate, sgsttaxpers,";
                strQuery += $"\n  sgsttaxamount,0 sgstamount, cgsttaxpers, cgsttaxamount,0 cgstamount, igsttaxpers, igsttaxamount,0 igstamount,0 receiptid,0 receiptno,0 oldtaxamount, ";
                strQuery += $"\n  0 oldamount,0 purqty,0 cesspers,0 cessamt,0 totdis,0 totdisamt,0 debitfreqty,0 extracesspers,0 extracessamt,cm.branchid,cm.mainbranchid,acid accid, hsncode, hsnid";
                strQuery += $"\n from creditdebitnotedissub cs inner join creditdebitnotedismain cm on cs.creditdebitnotemainid=cm.creditdebitnotemainid";
                strQuery += $"\n and cm.branchid={nBranchId} and cm.vouchertype = 'DEBITNOTE';";
                stringBuilder.Add(strQuery);

                stringBuilder.Add($"UPDATE debitnotedetails{nMainBranchId} pm SET taxid = tx.taxid FROM tax tx WHERE tx.taxpercent = pm.taxper AND pm.branchid = {nBranchId} and pm.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"UPDATE issuereturndetails{nMainBranchId} pm SET taxid = tx.taxid FROM tax tx WHERE tx.taxpercent = pm.taxpers AND pm.branchid = {nBranchId} AND pm.mainbranchid = {nMainBranchId}");

                int totalQueries = stringBuilder.Count;
                int queryIndex = 1;
                foreach (string queryTemplate in stringBuilder)
                {
                    int pct = 85 + (int)((double)queryIndex / totalQueries * 15); // 85-100%
                    ReportProgress($"FK Update [{queryIndex}/{totalQueries}]", pct);
                    testquerytemplate = queryTemplate;
                    var query = string.Format(queryTemplate, nMainBranchId, nBranchId);
                    using var command = PgCmd(query);
                    command.ExecuteNonQuery();
                    queryIndex++;
                }

            }
            catch (Exception ex)
            {
                string failedTable = ExceptionFormatter.ExtractTableName(testquerytemplate);
                Console.WriteLine(testquerytemplate);
                Console.WriteLine(ExceptionFormatter.Describe(ex));
                // Attach the target table + the failing query as structured fields and
                // re-throw so the branch transaction is rolled back and the UI shows it.
                throw new MigrationException(
                    $"Foreign-key / bulk update failed on table '{failedTable}'." + Environment.NewLine +
                    "Failing query: " + testquerytemplate + Environment.NewLine +
                    ex.Message,
                    tableName: failedTable,
                    inner: ex,
                    failingQuery: testquerytemplate,
                    functionName: "MigrationRunner." + nameof(ExecuteBulkUpdates));
            }
        }

        public void UpdatePrimaryKeyColumns(Int64 nMainBranchId, Int64 nBranchId, Int64 nFromBranchId)
        {
            // Create the indexes the FK-update phase relies on BEFORE running it. Without
            // these, every UPDATE ... FROM ... JOIN below sequentially scans the
            // (multi-branch) target tables, which is the main reason the phase is slow.
            EnsureMigrationIndexes(nMainBranchId);

            ReportProgress("Updating primary keys & foreign keys...", 85);
            ExecuteBulkUpdates(nMainBranchId, nBranchId, nFromBranchId);


            ReportProgress("Primary key updates completed", 100);
        }

        /// <summary>
        /// Creates (IF NOT EXISTS) the indexes the bulk FK-update phase joins/filters on,
        /// then refreshes planner statistics with ANALYZE so the new indexes are actually
        /// used. These run on the shared branch transaction/connection because the tables
        /// were just COPY-loaded in the same uncommitted transaction — creating the indexes
        /// from any other connection would block on those row locks.
        ///
        /// Each statement is wrapped in a SAVEPOINT: a single failure (e.g. a table that
        /// does not exist for this branch) is rolled back to the savepoint and skipped,
        /// instead of aborting the entire migration transaction.
        /// </summary>
        private void EnsureMigrationIndexes(Int64 nMainBranchId)
        {
            ReportProgress("Creating indexes for foreign-key update phase...", 83);
            Console.WriteLine("Creating indexes for foreign-key update phase...");

            // Per-main-branch lookup tables joined by tempid on the FROM side of the updates.
            var tempidTables = new List<string>
            {
                $"accounthead{nMainBranchId}",
                $"productmain{nMainBranchId}",
                $"manufacture{nMainBranchId}",
                $"hsn{nMainBranchId}",
            };

            // Shared lookup tables joined by tempid.
            var sharedTempidTables = new[]
            {
                "billseries", "category", "categoryhead", "doctor", "branch", "restotable", "area"
            };

            // Large per-main-branch target tables scanned by (branchid, mainbranchid)
            // repeatedly throughout the update phase.
            var branchFilteredTables = new List<string>
            {
                $"voucherdetails{nMainBranchId}", $"vouchermain{nMainBranchId}",
                $"store{nMainBranchId}",
                $"issuemain{nMainBranchId}", $"issuesubdetails{nMainBranchId}",
                $"receiptmain{nMainBranchId}", $"receiptdetails{nMainBranchId}",
                $"servicemain{nMainBranchId}", $"servicesubdetails{nMainBranchId}",
                $"productsub{nMainBranchId}",
                $"outstanding{nMainBranchId}",
                $"openingstockmain{nMainBranchId}", $"openingstockdetails{nMainBranchId}",
            };

            var statements = new List<string>();

            foreach (var t in tempidTables.Concat(sharedTempidTables))
                statements.Add($"CREATE INDEX IF NOT EXISTS ix_{t}_tempid ON {t} (tempid)");

            // tax is joined on taxpercent, not tempid.
            statements.Add("CREATE INDEX IF NOT EXISTS ix_tax_taxpercent ON tax (taxpercent)");

            foreach (var t in branchFilteredTables)
                statements.Add($"CREATE INDEX IF NOT EXISTS ix_{t}_branch ON {t} (branchid, mainbranchid)");

            foreach (var stmt in statements)
                TryExecInSavepoint(stmt);

            // Refresh statistics on the freshly-loaded tables so the planner uses the indexes.
            foreach (var t in tempidTables.Concat(branchFilteredTables))
                TryExecInSavepoint($"ANALYZE {t}");
        }

        /// <summary>
        /// Runs a single statement on the shared branch transaction inside a SAVEPOINT.
        /// On success the savepoint is released; on failure it is rolled back and the error
        /// is logged and swallowed, leaving the surrounding transaction usable.
        /// </summary>
        private void TryExecInSavepoint(string sql)
        {
            const string sp = "mig_idx_sp";
            using (var begin = PgCmd($"SAVEPOINT {sp}"))
                begin.ExecuteNonQuery();

            try
            {
                using var cmd = PgCmd(sql);
                cmd.ExecuteNonQuery();
                using var release = PgCmd($"RELEASE SAVEPOINT {sp}");
                release.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                using (var rollback = PgCmd($"ROLLBACK TO SAVEPOINT {sp}"))
                    rollback.ExecuteNonQuery();
                using (var release = PgCmd($"RELEASE SAVEPOINT {sp}"))
                    release.ExecuteNonQuery();
                Console.WriteLine($"  (skipped) {sql} -> {ex.Message}");
            }
        }


        public void FromDbTaxUpdate()
        {
            ReportProgress("Updating tax data in SQL Server...", 0);

            string strQuery = @"update Product set ProdLinkEShopId = Tax.TaxPercent  from Product inner join TaxGroup on Product.TaxGroupId = TaxGroup.TaxGroupId
               inner join TaxDetails on TaxGroup.TaxGroupId = TaxDetails.TaxGroupId
               inner join Tax on Tax.taxid = TaxDetails.TaxId
               where TransType = 'PURCHASE'";

            try
            {

                using var connection = SqlServerConnection.Create();
                connection.Open();
                var query = string.Format(strQuery);
                using var command = new SqlCommand(query, connection);
                command.ExecuteNonQuery();
                connection.Close();

                ReportProgress("Tax data updated successfully", 2);
            }
            catch (Exception ex)
            {
                ReportProgress($"Tax update failed: {ex.Message}", 2);
            }
        }


        public void fnMainSettingUpdate(long nMainBranchId)
        {

            ReportProgress("Updating mainsetting in SQL Server...", 0);

            string strQuery = @"select * from Settings";

            try
            {
                System.Data.DataTable dtsql = new System.Data.DataTable();
                using var connection = SqlServerConnection.Create();
                connection.Open();
                var query = string.Format(strQuery);
                using var command = new SqlCommand(query, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                adapter.Fill(dtsql);
                connection.Close();

                //strQuery = @"select * from mainsetting";
                //DataTable  dtposgres = new DataTable();
                //using var posconnection = PostgresConnection.Create();
                //posconnection.Open();
                //var posquery = string.Format(strQuery);
                //using var poscommand = new SqlCommand(query, connection);
                //adapter = new SqlDataAdapter(command);
                //adapter.Fill(dtposgres);
                //posconnection.Close();
                string strUpdateQuery = "";
                foreach (DataRow row in dtsql.Rows)
                {
                    string KeyValue = row["KeyValue"].ToString();
                    string Value = row["Value"].ToString();
                    switch (KeyValue)
                    {
                        case "AccMonth":
                            strUpdateQuery += "\n Update mainsetting set settingvalue = '" + Value + "' where mainbranchid = " + nMainBranchId + " and settingname='AccMonth';";
                            break;
                        case "AdditionalCessInclusiveInSales":
                            strUpdateQuery += "\n Update mainsetting set settingvalue = '" + Value + "' where mainbranchid = " + nMainBranchId + " and settingname='AdditionalCessInclusiveInSales';";
                            break;
                        case "Neethi":
                            strUpdateQuery += "\n Update mainsetting set settingvalue = '" + Value + "' where mainbranchid = " + nMainBranchId + " and settingname='AddOrMinusNeethiDis';";
                            break;
                        case "AutoAdjustReceiptPending":
                            strUpdateQuery += "\n Update mainsetting set settingvalue = '" + Value + "' where mainbranchid = " + nMainBranchId + " and settingname='AutoAdjustReceiptPending';";
                            break;
                        case "BatchDisplayName":
                            strUpdateQuery += "\n Update mainsetting set settingvalue = '" + Value + "' where mainbranchid = " + nMainBranchId + " and settingname='BatchDisplayName';";
                            break;
                        case "CompanyBranchLink":
                            strUpdateQuery += "\n Update mainsetting set settingvalue = '" + Value + "' where mainbranchid = " + nMainBranchId + " and settingname='CompanyBranchLink';";
                            break;
                        case "SaleExpiry":
                            strUpdateQuery += "\n Update mainsetting set settingvalue = '" + Value + "' where mainbranchid = " + nMainBranchId + " and settingname='ExpiryVisible';";
                            break;
                        case "SaleBatch":
                            strUpdateQuery += "\n Update mainsetting set settingvalue = '" + Value + "' where mainbranchid = " + nMainBranchId + " and settingname='BatchVisible';";
                            break;
                        case "ExpMonthYearFormat":
                            strUpdateQuery += "\n Update mainsetting set settingvalue = '" + Value + "' where mainbranchid = " + nMainBranchId + " and settingname='ExpMonthYearFormat';";
                            break;
                        case "ImageSave":
                            strUpdateQuery += "\n Update mainsetting set settingvalue = '" + Value + "' where mainbranchid = " + nMainBranchId + " and settingname='ImageSave';";
                            break;
                        case "ImageSavePath":
                            strUpdateQuery += "\n Update mainsetting set settingvalue = '" + Value + "' where mainbranchid = " + nMainBranchId + " and settingname='ImageSavePath';";
                            break;
                        case "ItemSearch":
                            strUpdateQuery += "\n Update mainsetting set settingvalue = 'MultiDeepSearch' where mainbranchid = " + nMainBranchId + " and settingname='ItemSearch';";
                            break;
                        case "NegativeBilling":
                            strUpdateQuery += "\n Update mainsetting set settingvalue = '" + Value + "' where mainbranchid = " + nMainBranchId + " and settingname='NegativeBilling';";
                            break;
                        case "PreviousHistoryPurchase":
                            strUpdateQuery += "\n Update mainsetting set settingvalue = '" + Value + "' where mainbranchid = " + nMainBranchId + " and settingname='PreviousHistoryPurchase';";
                            break;
                        case "PreviousHistory":
                            strUpdateQuery += "\n Update mainsetting set settingvalue = '" + Value + "' where mainbranchid = " + nMainBranchId + " and settingname='PreviousHistorySales';";
                            break;
                        case "ProductSaveOtherBranch":
                            strUpdateQuery += "\n Update mainsetting set settingvalue = '" + Value + "' where mainbranchid = " + nMainBranchId + " and settingname='ProductSaveOtherBranch';";
                            break;
                        case "PurchaseItemCode":
                            strUpdateQuery += "\n Update mainsetting set settingvalue = '" + Value + "' where mainbranchid = " + nMainBranchId + " and settingname='PurchaseItemCodeVisible';";
                            break;
                        case "QtyDecPlace":
                            strUpdateQuery += "\n Update mainsetting set settingvalue = '" + Value + "' where mainbranchid = " + nMainBranchId + " and settingname='QtyDecPlace';";
                            break;
                        case "RateConditionInSales":
                            strUpdateQuery += "\n Update mainsetting set settingvalue = '" + Value + "' where mainbranchid = " + nMainBranchId + " and settingname='RateConditionInSales';";
                            break;
                        case "DecimalPlace":
                            strUpdateQuery += "\n Update mainsetting set settingvalue = '" + Value + "' where mainbranchid = " + nMainBranchId + " and settingname='RateDecimalPlace';";
                            break;
                        case "SRof":
                            strUpdateQuery += "\n Update mainsetting set settingvalue = '" + Value + "' where mainbranchid = " + nMainBranchId + " and settingname='Rof';";
                            break;
                        //case "SalesInclusiveVisibleInSales":
                        //    strUpdateQuery += "\n Update mainsetting set settingvalue = '" + Value + "' where mainbranchid = " + nMainBranchId + " and settingkey='SalesInclusive';";
                        //    break;
                        case "SalesItemCode":
                            strUpdateQuery += "\n Update mainsetting set settingvalue = '" + Value + "' where mainbranchid = " + nMainBranchId + " and settingname='SalesItemCodeVisible';";
                            break;
                        case "ProductName":
                            strUpdateQuery += "\n Update mainsetting set settingvalue = '" + Value + "' where mainbranchid = " + nMainBranchId + " and settingname='SoftwareBusinessType';";
                            break;
                        case "TaxName":
                            strUpdateQuery += "\n Update mainsetting set settingvalue = '" + Value + "' where mainbranchid = " + nMainBranchId + " and settingname='TaxName';";
                            break;
                        case "UniqueBarCode":
                            strUpdateQuery += "\n Update mainsetting set settingvalue = '" + Value + "' where mainbranchid = " + nMainBranchId + " and settingname='UniqueBarCode';";
                            break;
                        case "UniqueNo":
                            strUpdateQuery += "\n Update mainsetting set settingbillno = '" + Value + "' where mainbranchid = " + nMainBranchId + " and settingname='UniqueBatchNo';";
                            break;
                    }

                }



                ExecPgNonQuery(strUpdateQuery);

                ReportProgress("Updating mainsetting successfully", 2);
            }
            catch (Exception ex)
            {
                ReportProgress($"Updating mainsetting failed: {ex.Message}", 2);
                throw; // abort so the branch transaction is rolled back
            }
        }

        public void fnBranchSettingUpdate(long nMainBranchId, long nBranchId, long nFromBranchId)
        {
            ReportProgress("Updating Branchsetting in SQL Server...", 0);

            string strQuery = " select * from branchsetting where branchid=" + nFromBranchId;

            try
            {
                System.Data.DataTable dtsql = new System.Data.DataTable();
                using var connection = SqlServerConnection.Create();
                connection.Open();
                var query = string.Format(strQuery);
                using var command = new SqlCommand(query, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                adapter.Fill(dtsql);
                connection.Close();

                //strQuery = @"select * from branchsetting where branchid="+nBranchId+ " and mainbranchid=";
                //DataTable dtposgres = new DataTable();
                //using var posconnection = PostgresConnection.Create();
                //posconnection.Open();
                //var posquery = string.Format(strQuery);
                //using var poscommand = new SqlCommand(query, connection);
                //adapter = new SqlDataAdapter(command);
                //adapter.Fill(dtposgres);
                //posconnection.Close();
                string strUpdateQuery = "";
                foreach (DataRow row in dtsql.Rows)
                {
                    string KeyValue = row["SettingName"].ToString();
                    string Value = row["Value"].ToString();
                    switch (KeyValue)
                    {
                        case "AccountHeadUpperCase":
                            strUpdateQuery += "\n Update branchsetting set settingvalue = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingname='AccountHeadUpperCase' and branchid='" + nBranchId + "' ;";
                            break;
                        case "AgeRange1":
                            strUpdateQuery += "\n Update branchsetting set settingbillno = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingname='AgeRange1' and branchid='" + nBranchId + "';";
                            break;
                        case "AgeRange2":
                            strUpdateQuery += "\n Update branchsetting set settingbillno = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingname='AgeRange2' and branchid='" + nBranchId + "';";
                            break;
                        case "AgeRange3":
                            strUpdateQuery += "\n Update branchsetting set settingbillno = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingname='AgeRange3' and branchid='" + nBranchId + "';";
                            break;
                        case "AgeRange4":
                            strUpdateQuery += "\n Update branchsetting set settingbillno = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingname='AgeRange4' and branchid='" + nBranchId + "' ;";
                            break;
                        case "AgeRange5":
                            strUpdateQuery += "\n Update branchsetting set settingbillno = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingname='AgeRange5' and branchid='" + nBranchId + "';";
                            break;
                        case "AgeRange6":
                            strUpdateQuery += "\n Update branchsetting set settingbillno = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingname='AgeRange6' and branchid='" + nBranchId + "' ;";
                            break;
                        case "AmtPerPoint":
                            strUpdateQuery += "\n Update branchsetting set settingbillno = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingname='AmtPerPoint' and branchid='" + nBranchId + "' ;";
                            break;
                        case "BarCodeBoxVisibleInSales":
                            strUpdateQuery += "\n Update branchsetting set settingvalue = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingname='BarCodeBoxVisibleInSales' and branchid='" + nBranchId + "';";
                            break;
                        case "BillPrintSaveOrder":
                            strUpdateQuery += "\n Update branchsetting set settingvalue = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingname='BillPrintSaveOrder' and branchid='" + nBranchId + "';";
                            break;
                        case "CancelledBillRemove":
                            strUpdateQuery += "\n Update branchsetting set settingvalue = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingname='CancelledBillRemove' and branchid='" + nBranchId + "';";
                            break;
                        case "BillPost":
                            strUpdateQuery += "\n Update branchsetting set settingvalue = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingname='CashBillPostAccounts' and branchid='" + nBranchId + "';";
                            break;
                        case "DcAccountsPost":
                            strUpdateQuery += "\n Update branchsetting set settingvalue = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingname='DeliveryOutAccountsPost' and branchid='" + nBranchId + "';";
                            break;
                        case "ExpiryDebitNoteItemFromExpiryReceive":
                            strUpdateQuery += "\n Update branchsetting set settingvalue = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingname='ExpiryDebitNoteItemFromExpiryReceive' and branchid='" + nBranchId + "';";
                            break;
                        case "GodownTransferNextNo":
                            strUpdateQuery += "\n Update branchsetting set settingbillno = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingname='GodownTransferBillNo' and branchid='" + nBranchId + "';";
                            break;
                        case "GodownReturnNextNo":
                            strUpdateQuery += "\n Update branchsetting set settingbillno = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingname='GodownTransferReturnNo' and branchid='" + nBranchId + "' ;";
                            break;
                        case "JobCardNo":
                            strUpdateQuery += "\n Update branchsetting set settingbillno = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingname='JobCardNo' and branchid='" + nBranchId + "';";
                            break;
                        case "PointSystem":
                            strUpdateQuery += "\n Update branchsetting set settingvalue= '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingname='PointSystem' and branchid='" + nBranchId + "';";
                            break;
                        case "PointValue":
                            strUpdateQuery += "\n Update branchsetting set settingbillno = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingname='PointValue' and branchid='" + nBranchId + "';";
                            break;
                        case "SalesmanFixedInSales":
                            strUpdateQuery += "\n Update branchsetting set settingvalue = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingname='SalesmanFixedInSales' and branchid='" + nBranchId + "';";
                            break;
                        case "ExpiryDebitNoteSameItemPrintOneLine":
                            strUpdateQuery += "\n Update branchsetting set settingvalue = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingname='ExpiryDebitNoteSameItemPrintOneLine' and branchid='" + nBranchId + "';";
                            break;
                        case "TemporaryPurchaseNo":
                            strUpdateQuery += "\n Update branchsetting set settingbillno = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingname='TemporaryPurchaseNo' and branchid='" + nBranchId + "';";
                            break;
                        case "InclusiveInSales":
                            strUpdateQuery += "\n Update branchsetting set settingvalue = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingname='InclusiveInSales' and branchid='" + nBranchId + "';";
                            strUpdateQuery += $"\n UPDATE billseries SET billserbillinclusive = '{Value}' WHERE   mainbranchid ={nMainBranchId} AND branchid ={nBranchId} AND billsersource = 'SALES';";
                            break;
                        case "GSTRAddDebitNote":
                            strUpdateQuery += "\n Update branchsetting set settingvalue = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingname='GSTRAddDebitNote' and branchid='" + nBranchId + "';";
                            break;
                        case "HsnSummaryReturnAddExpiry":
                            strUpdateQuery += "\n Update branchsetting set settingvalue = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingname='HsnSummaryReturnAddExpiry' and branchid='" + nBranchId + "';";
                            break;
                        case "DebitNotePost":
                            strUpdateQuery += "\n Update branchsetting set settingvalue = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingname='DebitNotePost' and branchid='" + nBranchId + "';";
                            break;
                        case "ExpiryReturn":
                            strUpdateQuery += "\n Update branchsetting set settingvalue = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingname='ExpiryReturnAccountPost' and branchid='" + nBranchId + "';";
                            break;
                        case "IssueReturn":
                            strUpdateQuery += "\n Update branchsetting set settingvalue = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingname='SalesReturnAccountPost' and branchid='" + nBranchId + "';";
                            strUpdateQuery += $"\n Update accounthead{nMainBranchId} set retadjustment = '" + Value + "' where mainbranchid = '" + nMainBranchId + "'  and branchid='" + nBranchId + "';";
                            break;
                        case "SalesItemCode":
                            strUpdateQuery += "\n Update branchsetting set settingvalue = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingname='SalesItemCode' and branchid='" + nBranchId + "';";
                            break;
                        case "PurchaseItemCode":
                            strUpdateQuery += "\n Update branchsetting set settingvalue = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingname='PurchaseItemCode' and branchid='" + nBranchId + "';";
                            break;

                    }

                }

                ExecPgNonQuery(strUpdateQuery);

                ReportProgress("Updating BranchSetting  successfully", 2);
            }
            catch (Exception ex)
            {
                ReportProgress($"Updating BranchSetting  failed: {ex.Message}", 2);
                MessageBox.Show(ex.Message.ToString());
                throw; // abort so the branch transaction is rolled back
            }
        }

        public void fnVouchePrefixUpdate(long nMainBranchId, long nBranchId, long nFromBranchId)
        {
            ReportProgress("Updating voucher prefix in SQL Server...", 0);

            string strQuery = @"select * from VoucherPrefix where branchid=" + nFromBranchId;

            try
            {
                System.Data.DataTable dtsql = new System.Data.DataTable();
                using var connection = SqlServerConnection.Create();
                connection.Open();
                var query = string.Format(strQuery);
                using var command = new SqlCommand(query, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                adapter.Fill(dtsql);
                connection.Close();

                //strQuery = @"select * from branchsetting where branchid="+nBranchId+ " and mainbranchid=";
                //DataTable dtposgres = new DataTable();
                //using var posconnection = PostgresConnection.Create();
                //posconnection.Open();
                //var posquery = string.Format(strQuery);
                //using var poscommand = new SqlCommand(query, connection);
                //adapter = new SqlDataAdapter(command);
                //adapter.Fill(dtposgres);
                //posconnection.Close();
                string strUpdateQuery = "";
                foreach (DataRow row in dtsql.Rows)
                {
                    string Prefix = row["Prefix"].ToString();
                    string vocherNo = row["VoucherNo"].ToString();
                    string Value = row["Value"].ToString();
                    string UniqueVoucherId = row["UniqueVoucherId"].ToString();
                    switch (Value)
                    {
                        case "1":
                            strUpdateQuery += "\n Update VoucherPrefix set voucherNo = '" + vocherNo + "',uniquevoucherid = '" + UniqueVoucherId + "' where mainbranchid = '" + nMainBranchId + "' and vprefixid ='" + Value + "' and branchid='" + nBranchId + "' ;";
                            break;
                        case "2":
                            strUpdateQuery += "\n Update VoucherPrefix set voucherNo = '" + vocherNo + "',uniquevoucherid = '" + UniqueVoucherId + "' where mainbranchid = '" + nMainBranchId + "' and vprefixid ='" + Value + "' and branchid='" + nBranchId + "' ;";
                            break;
                        case "3":
                            strUpdateQuery += "\n Update VoucherPrefix set voucherNo = '" + vocherNo + "',uniquevoucherid ='" + UniqueVoucherId + "' where mainbranchid = '" + nMainBranchId + "' and vprefixid ='" + Value + "' and branchid='" + nBranchId + "' ;";
                            break;
                        case "4":
                            strUpdateQuery += "\n Update VoucherPrefix set voucherNo = '" + vocherNo + "',uniquevoucherid ='" + UniqueVoucherId + "' where mainbranchid = '" + nMainBranchId + "' and vprefixid ='" + Value + "' and branchid='" + nBranchId + "' ;";
                            break;
                        case "5":
                            strUpdateQuery += "\n Update VoucherPrefix set voucherNo = '" + vocherNo + "',uniquevoucherid ='" + UniqueVoucherId + "' where mainbranchid = '" + nMainBranchId + "' and vprefixid ='" + Value + "' and branchid='" + nBranchId + "' ;";
                            break;
                        case "6":
                            strUpdateQuery += "\n Update VoucherPrefix set voucherNo = '" + vocherNo + "',uniquevoucherid ='" + UniqueVoucherId + "' where mainbranchid = '" + nMainBranchId + "' and vprefixid ='" + Value + "' and branchid='" + nBranchId + "' ;";
                            break;
                        case "7":
                            strUpdateQuery += "\n Update VoucherPrefix  set voucherNo = '" + vocherNo + "',uniquevoucherid ='" + UniqueVoucherId + "' where mainbranchid = '" + nMainBranchId + "' and vprefixid ='" + Value + "' and branchid='" + nBranchId + "' ;";
                            break;
                        case "8":
                            strUpdateQuery += "\n Update VoucherPrefix set voucherNo = '" + vocherNo + "',uniquevoucherid ='" + UniqueVoucherId + "' where mainbranchid = '" + nMainBranchId + "' and vprefixid ='" + Value + "' and branchid='" + nBranchId + "' ;";
                            break;
                        case "9":
                            strUpdateQuery += "\n Update VoucherPrefix set voucherNo = '" + vocherNo + "',uniquevoucherid ='" + UniqueVoucherId + "' where mainbranchid = '" + nMainBranchId + "' and vprefixid ='" + Value + "' and branchid='" + nBranchId + "' ;";
                            break;
                        case "10":
                            strUpdateQuery += "\n Update VoucherPrefix set voucherNo = '" + vocherNo + "',uniquevoucherid ='" + UniqueVoucherId + "' where mainbranchid = '" + nMainBranchId + "' and vprefixid ='" + Value + "' and branchid='" + nBranchId + "' ;";
                            break;


                    }

                }

                ExecPgNonQuery(strUpdateQuery);

                ReportProgress("Updating voucher prefix successfully", 2);
            }
            catch (Exception ex)
            {
                ReportProgress($"Updating voucher prefix failed: {ex.Message}", 2);
                throw; // abort so the branch transaction is rolled back
            }
        }

        public void fnBillSeriesInclusiveUpdate(long nFromBranchId, long nMainBranchId, long nToBranchId)
        {
            ReportProgress("Updating BillSeries in SQL Server...", 0);
            string strUpdateQuery = "";
            string strQuery = $"select * from BillSeriesSalesInclusiveSet where billserid in ( select  billserid from BillSeries where  branchid = {nFromBranchId})";

            try
            {
                System.Data.DataTable dtsql = new System.Data.DataTable();
                using var connection = SqlServerConnection.Create();
                connection.Open();

                using var command = new SqlCommand(strQuery, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                adapter.Fill(dtsql);
                connection.Close();



                foreach (DataRow row in dtsql.Rows)
                {
                    string BillSerId = row["BillSerId"].ToString();
                    string InclusiveSales = row["InclusiveSales"].ToString();
                    string TaxCondition = row["TaxCondition"].ToString();


                    strUpdateQuery += "\n UPDATE billseries SET billserbillinclusive = '" + InclusiveSales +
                                      "', billsertaxadd = '" + TaxCondition +
                                      "' WHERE tempid = '" + BillSerId +
                                      "' AND mainbranchid = '" + nMainBranchId +
                                      "' AND branchid = '" + nToBranchId +
                                      "' AND billsersource = 'SALES';";
                }

                ExecPgNonQuery(strUpdateQuery);

                ReportProgress("Updating BillSeries successfully", 2);
            }
            catch (Exception ex)
            {
                ReportProgress($"Updating failed: {ex.Message} Update Query {strUpdateQuery}", 2);
                throw; // abort so the branch transaction is rolled back
            }
        }
        public void fnHospitalSettingUpdate(long nMainBranchId, long nBranchId, long nFromBranchId)
        {
            ReportProgress("Updating Hospitalsetting in SQL Server...", 0);

            string strQuery = " select * from HospitalSetting where branchid=" + nFromBranchId;

            try
            {
                System.Data.DataTable dtsql = new System.Data.DataTable();
                using var connection = SqlServerConnection.Create();
                connection.Open();
                var query = string.Format(strQuery);
                using var command = new SqlCommand(query, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                adapter.Fill(dtsql);
                connection.Close();

                //strQuery = @"select * from branchsetting where branchid="+nBranchId+ " and mainbranchid=";
                //DataTable dtposgres = new DataTable();
                //using var posconnection = PostgresConnection.Create();
                //posconnection.Open();
                //var posquery = string.Format(strQuery);
                //using var poscommand = new SqlCommand(query, connection);
                //adapter = new SqlDataAdapter(command);
                //adapter.Fill(dtposgres);
                //posconnection.Close();
                string strUpdateQuery = "";
                foreach (DataRow row in dtsql.Rows)
                {
                    string KeyValue = row["HosSetting_Name"].ToString();
                    string Value = row["HosSettings_Value"].ToString();
                    switch (KeyValue)
                    {
                        case "RegNo":
                            strUpdateQuery += "\n Update hospitalsetting set hossettingsvalue = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingname='RegNo' and branchid='" + nBranchId + "' ;";
                            break;
                        case "OpNo":
                            strUpdateQuery += "\n Update hospitalsetting set hossettingsvalue = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingname='OpNo' and branchid='" + nBranchId + "';";
                            break;
                        //case "Shift":
                        //    strUpdateQuery += "\n Update hospitalsetting set hossettingsvalue = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingname='LabBillNo' and branchid='" + nBranchId + "';";
                        //    break;
                        //case "IPNO":
                        //    strUpdateQuery += "\n Update hospitalsetting set hossettingsvalue = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingname='AgeRange3' and branchid='" + nBranchId + "';";
                        //    break;
                        case "RevisitNo":
                            strUpdateQuery += "\n Update hospitalsetting set hossettingsvalue = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingname='RevisitNo' and branchid='" + nBranchId + "' ;";
                            break;
                        case "LabBillNo":
                            strUpdateQuery += "\n Update hospitalsetting set hossettingsvalue = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingname='LabBillNo' and branchid='" + nBranchId + "';";
                            break;
                        case "TestResultBillNo":
                            strUpdateQuery += "\n Update hospitalsetting set hossettingsvalue = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingname='TestResultBillNo' and branchid='" + nBranchId + "' ;";
                            break;
                        case "PMRBillNo":
                            strUpdateQuery += "\n Update hospitalsetting set hossettingsvalue = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingname='PMRBillNo' and branchid='" + nBranchId + "' ;";
                            break;
                        case "AppointmentBillNo":
                            strUpdateQuery += "\n Update hospitalsetting set hossettingsvalue = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingname='AppointmentBillNo' and branchid='" + nBranchId + "';";
                            break;
                            //case "DischargeId":
                            //    strUpdateQuery += "\n Update hospitalsetting set hossettingsvalue = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingname='BillPrintSaveOrder' and branchid='" + nBranchId + "';";
                            //    break;


                    }

                }

                ExecPgNonQuery(strUpdateQuery);

                ReportProgress("Updating HospitalSetting  successfully", 2);
            }
            catch (Exception ex)
            {
                ReportProgress($"Updating HospitalSetting  failed: {ex.Message}", 2);
                MessageBox.Show(ex.Message.ToString());
                throw; // abort so the branch transaction is rolled back
            }
        }
        

        public void fnBranchUpdate(long nMainBranchId, long nBranchId, long nFromBranchId)
        {
            long nBillNo = 0;
            ReportProgress("Updating Branch in SQL Server...", 0);
            string strQuery = @"select * from branch where branchid=" + nFromBranchId;
            try
            {
                System.Data.DataTable dtsql = new System.Data.DataTable();
                using var connection = SqlServerConnection.Create();
                connection.Open();
                using var command = new SqlCommand(strQuery, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                adapter.Fill(dtsql);
                connection.Close();

                string strUpdateQuery = "";
                foreach (DataRow row in dtsql.Rows)
                {
                    string BranchCode = row["BranchCode"].ToString().Replace("'", "''");
                    string BranchName = row["BranchName"].ToString().Replace("'", "''");
                    string BranchAdr1 = row["BranchAdr1"].ToString().Replace("'", "''");
                    string BranchAdr2 = row["BranchAdr2"].ToString().Replace("'", "''");
                    string BranchAdr3 = row["BranchAdr3"].ToString().Replace("'", "''");
                    string BranchFtr1 = row["BranchFtr1"].ToString().Replace("'", "''");
                    string BranchFtr2 = row["BranchFtr2"].ToString().Replace("'", "''");
                    string BranchFtr3 = row["BranchFtr3"].ToString().Replace("'", "''");
                    string Phone = row["Phone"].ToString().Replace("'", "''");
                    string Mail = row["Mail"].ToString().Replace("'", "''");
                    string Active = Convert.ToBoolean(row["Active"]) ? "true" : "false";
                    string TinNo1 = row["TinNo1"].ToString().Replace("'", "''");
                    string TinNo2 = row["TinNo2"].ToString().Replace("'", "''");
                    string DLNo1 = row["DLNo1"].ToString().Replace("'", "''");
                    string DLNo2 = row["DLNo2"].ToString().Replace("'", "''");
                    string MobileNo = row["MobileNo"].ToString().Replace("'", "''");
                    string MailId = row["MailId"].ToString().Replace("'", "''");
                    string MailPwd = row["MailPwd"].ToString().Replace("'", "''");
                    string BarCodeName = row["BarCodeName"].ToString().Replace("'", "''");
                    string BarCodeHeaderName = row["BarCodeHeaderName"].ToString().Replace("'", "''");
                    string ComImage = row["ComImage"].ToString().Replace("'", "''");
                    string Branch_StateCode = row["Branch_StateCode"].ToString().Replace("'", "''");
                    string Branch_StateName = row["Branch_StateName"].ToString().Replace("'", "''");
                    string Branch_BankName = row["Branch_BankName"].ToString().Replace("'", "''");
                    string Branch_BankAddr1 = row["Branch_BankAddr1"].ToString().Replace("'", "''");
                    string Branch_BankAddr2 = row["Branch_BankAddr2"].ToString().Replace("'", "''");
                    string Branch_BankAcNo = row["Branch_BankAcNo"].ToString().Replace("'", "''");
                    string Branch_IFSCCODE = row["Branch_IFSCCODE"].ToString().Replace("'", "''");
                    string Branch_PanCardNo = row["Branch_PanCardNo"].ToString().Replace("'", "''");
                    string Branch_QRCode = row["Branch_QRCode"].ToString().Replace("'", "''");
                    string Branch_Declaration1 = row["Branch_Declaration1"].ToString().Replace("'", "''");
                    string Branch_Declaration2 = row["Branch_Declaration2"].ToString().Replace("'", "''");
                    string Branch_Declaration3 = row["Branch_Declaration3"].ToString().Replace("'", "''");
                    string Branch_Declaration4 = row["Branch_Declaration4"].ToString().Replace("'", "''");
                    string Branch_BankHolderName = row["Branch_BankHolderName"].ToString().Replace("'", "''");
                    string Branch_OrderUserName = row["Branch_OrderUserName"].ToString().Replace("'", "''");
                    string Branch_OrderPwd = row["Branch_OrderPwd"].ToString().Replace("'", "''");
                    string Branch_WhatsAppNo = row["Branch_WhatsAppNo"].ToString().Replace("'", "''");
                    string Branch_WhatsAppTokenNo = row["Branch_WhatsAppTokenNo"].ToString().Replace("'", "''");
                    string Branch_WhatsAppUrl = row["Branch_WhatsAppUrl"].ToString().Replace("'", "''");
                    string Branch_SecurePwd = row["Branch_SecurePwd"].ToString().Replace("'", "''");
                    string Branch_BarCodeDesign = row["Branch_BarCodeDesign"].ToString().Replace("'", "''");
                    string AcId = row["AcId"].ToString();

                    nBillNo = Convert.ToInt64(row["NextBillNo"].ToString());

                    strUpdateQuery += "\n UPDATE branch SET " +
                        "branchcode = '" + BranchCode + "', " +
                        "branchname = '" + BranchName + "', " +
                        "branchadr1 = '" + BranchAdr1 + "', " +
                        "branchadr2 = '" + BranchAdr2 + "', " +
                        "branchadr3 = '" + BranchAdr3 + "', " +
                        "branchftr1 = '" + BranchFtr1 + "', " +
                        "branchftr2 = '" + BranchFtr2 + "', " +
                        "branchftr3 = '" + BranchFtr3 + "', " +
                        "branchphone = '" + Phone + "', " +
                        "branchmail = '" + Mail + "', " +
                        "branchactive = " + Active + ", " +
                        "branchtinno1 = '" + TinNo1 + "', " +
                        "branchtinno2 = '" + TinNo2 + "', " +
                        "branchdlno1 = '" + DLNo1 + "', " +
                        "branchdlno2 = '" + DLNo2 + "', " +
                        "branchmobileno = '" + MobileNo + "', " +
                        "branchmailid = '" + MailId + "', " +
                        "branchmailpwd = '" + MailPwd + "', " +
                        "branchbarcodename = '" + BarCodeName + "', " +
                        "barcodeheadername = '" + BarCodeHeaderName + "', " +
                        "branchcomimage = '" + ComImage + "', " +
                        "branchstatecode = " + (string.IsNullOrEmpty(Branch_StateCode) ? 0 : Branch_StateCode) + ", " +
                        "branchstatename = '" + Branch_StateName + "', " +
                        "branchbankname = '" + Branch_BankName + "', " +
                        "branchbankaddr1 = '" + Branch_BankAddr1 + "', " +
                        "branchbankaddr2 = '" + Branch_BankAddr2 + "', " +
                        "branchbankacno = '" + Branch_BankAcNo + "', " +
                        "branchifsccode = '" + Branch_IFSCCODE + "', " +
                        "branchpancardno = '" + Branch_PanCardNo + "', " +
                        "branchqrcode = '" + Branch_QRCode + "', " +
                        "branchdeclaration1 = '" + Branch_Declaration1 + "', " +
                        "branchdeclaration2 = '" + Branch_Declaration2 + "', " +
                        "branchdeclaration3 = '" + Branch_Declaration3 + "', " +
                        "branchdeclaration4 = '" + Branch_Declaration4 + "', " +
                        "branchbankholdername = '" + Branch_BankHolderName + "', " +
                        "branchorderusername = '" + Branch_OrderUserName + "', " +
                        "branchorderpwd = '" + Branch_OrderPwd + "', " +
                        "branchwhatsappno = '" + Branch_WhatsAppNo + "', " +
                        "branchwhatsapptokenno = '', " +
                        "branchwhatsappurl = '', " +
                        "branchsecurepwd = '" + Branch_SecurePwd + "', " +
                        "branchbarcodedesign = '" + Branch_BarCodeDesign + "', " +
                        "acid = " + (string.IsNullOrEmpty(AcId) ? 0 : AcId) +
                        " WHERE mainbranchid = " + nMainBranchId +
                        " AND branchid = " + nBranchId + ";";
                }
                
              
                ExecPgNonQuery(strUpdateQuery);
                ReportProgress("Updating Branch successfully", 2);

            }
            catch (Exception ex)
            {
                ReportProgress($"Updating failed: {ex.Message}", 2);
                throw; // abort so the branch transaction is rolled back
            }
        }


        public static List<AccountIdMigration> GetAccountIdMigrations()
        {
            return new List<AccountIdMigration>
              {
            new AccountIdMigration { AccountId = 34, HeadName = "TCS OUT", AcId = -8, PosgresAcId = -50 },
        new AccountIdMigration { AccountId = 35, HeadName = "TCS IN", AcId = -7, PosgresAcId = -51 },
        new AccountIdMigration { AccountId = 36, HeadName = "COURIER CHARGE", AcId = -6, PosgresAcId = -52 },
        new AccountIdMigration { AccountId = 37, HeadName = "Output Cess", AcId = -5, PosgresAcId = -43 },
        new AccountIdMigration { AccountId = 38, HeadName = "Input Cess", AcId = -4, PosgresAcId = -42 },
        new AccountIdMigration { AccountId = 1, HeadName = "PURCHASE RETUN DEBIT NOT", AcId = 3, PosgresAcId = -5 },
        new AccountIdMigration { AccountId = 2, HeadName = "SALES RETUN CREDITNOTE", AcId = 4, PosgresAcId = -4 },
        new AccountIdMigration { AccountId = 3, HeadName = "AMOUNT 12%", AcId = 6, PosgresAcId = -53 },
        new AccountIdMigration { AccountId = 4, HeadName = "TAX 12%", AcId = 7, PosgresAcId = -54 },
        new AccountIdMigration { AccountId = 5, HeadName = "AMOUNT 18%", AcId = 8, PosgresAcId = -55 },
        new AccountIdMigration { AccountId = 6, HeadName = "DISCOUNT PAID", AcId = 9, PosgresAcId = -56 },
        new AccountIdMigration { AccountId = 7, HeadName = "PURCHASE NON TAXABLE", AcId = 11, PosgresAcId = -57 },
        new AccountIdMigration { AccountId = 8, HeadName = "SALES NON TAXABLE", AcId = 12, PosgresAcId = -58 },
        new AccountIdMigration { AccountId = 9, HeadName = "PURCHASE 5%", AcId = 15, PosgresAcId = -59 },
        new AccountIdMigration { AccountId = 10, HeadName = "SALES 5%", AcId = 16, PosgresAcId = -60 },
        new AccountIdMigration { AccountId = 11, HeadName = "CREDIT NOTE TAX 5%", AcId = 17, PosgresAcId = -61 },
        new AccountIdMigration { AccountId = 12, HeadName = "CREDIT NOTE TAX 12%", AcId = 18, PosgresAcId = -62 },
        new AccountIdMigration { AccountId = 13, HeadName = "DEBIT NOTE TAX 5%", AcId = 19, PosgresAcId = -63 },
        new AccountIdMigration { AccountId = 14, HeadName = "DEBIT NOTE TAX 12%", AcId = 20, PosgresAcId = -64 },
        new AccountIdMigration { AccountId = 15, HeadName = "TAX 18%", AcId = 22, PosgresAcId = -65 },
        new AccountIdMigration { AccountId = 16, HeadName = "BANK CHARGE", AcId = 23, PosgresAcId = -49 },
        new AccountIdMigration { AccountId = 17, HeadName = "AMOUNT 28%", AcId = 24, PosgresAcId = -66 },
        new AccountIdMigration { AccountId = 18, HeadName = "TAX 28 %", AcId = 25, PosgresAcId = -67 },
        new AccountIdMigration { AccountId = 19, HeadName = "CASH ACCOUNTS", AcId = 26, PosgresAcId = -46 },
        new AccountIdMigration { AccountId = 20, HeadName = "CREDIT NOTE AMOUNT", AcId = 27, PosgresAcId = -68 },
        new AccountIdMigration { AccountId = 21, HeadName = "PURCHASE AMOUNT 5 %", AcId = 31, PosgresAcId = -69 },
        new AccountIdMigration { AccountId = 22, HeadName = "SALES AMOUNT 5 %", AcId = 32, PosgresAcId = -70 },
        new AccountIdMigration { AccountId = 23, HeadName = "CREDIT NOTE TAX 18%", AcId = 33, PosgresAcId = -140 },
        new AccountIdMigration { AccountId = 24, HeadName = "CREDIT NOTE TAX 28%", AcId = 34, PosgresAcId = -72 },
        new AccountIdMigration { AccountId = 25, HeadName = "DEBIT NOTE AMOUNT", AcId = 35, PosgresAcId = -80 },
        new AccountIdMigration { AccountId = 26, HeadName = "ROUND OFF", AcId = 39, PosgresAcId = -47 },
        new AccountIdMigration { AccountId = 27, HeadName = "OTHER CHARGE", AcId = 40, PosgresAcId = -73 },
        new AccountIdMigration { AccountId = 28, HeadName = "EXCISEDUTY", AcId = 41, PosgresAcId = -74 },
        new AccountIdMigration { AccountId = 29, HeadName = "STAMPING CHARGE", AcId = 42, PosgresAcId = -75 },
        new AccountIdMigration { AccountId = 30, HeadName = "INTERSTATE AMOUNT", AcId = 44, PosgresAcId = -76 },
        new AccountIdMigration { AccountId = 31, HeadName = "TDS", AcId = 48, PosgresAcId = -77 },
        new AccountIdMigration { AccountId = 32, HeadName = "DEBIT NOTE TAX 18%", AcId = 49, PosgresAcId = -78 },
        new AccountIdMigration { AccountId = 33, HeadName = "DEBIT NOTE TAX 28%", AcId = 50, PosgresAcId = -79 },
    };

        }


        public void fnSqlMainBranchValueUpdate()
        {
            var strQuery = "";
            strQuery += "\n update Hsn set BranchId=17 where BranchId is null";
            strQuery += "\n update Category set BranchId=17 where BranchId is null";

            using var conn = SqlServerConnection.Create();
            conn.Open();
            using var cmd = new SqlCommand(strQuery, conn);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Sets the default manufacturer on the branch's UNMAPPED products only
        /// (manufacture_id = 0): they are assigned the most-recently-added manufacturer
        /// (highest manufacture_id) of their main branch. Products that already have a
        /// mapped manufacturer are left untouched. If no manufacturer exists, the value
        /// is left unchanged (COALESCE guard) so a NULL is never written.
        /// Runs on the shared branch transaction via ExecPgNonQuery, so any failure
        /// reports the table, column, function and the exact query.
        /// </summary>
        public void fnDefaultValueUpdate(long nFromBranchId, long nMainBranchId, long nBranchId)
        {
            ReportProgress("Updating product default manufacturer...", 0);

            // Scalar subquery picks the latest manufacturer; COALESCE keeps the existing
            // value when manufacture{nMainBranchId} has no rows (avoids writing NULL).
            string strQuery =
                $"\n UPDATE productmain{nMainBranchId} pm " +
                $"\n SET manufacture_id = COALESCE(" +
                $"\n (SELECT mn.manufacture_id FROM manufacture{nMainBranchId} mn " +
                $"\n ORDER BY mn.manufacture_id DESC LIMIT 1), pm.manufacture_id) " +
                $"\n WHERE pm.manufacture_id = 0 and pm.branchid = {nBranchId} AND pm.mainbranchid = {nMainBranchId};";

            strQuery += $"\n UPDATE productmain{nMainBranchId}  pm" +
                        $"\n SET productsearch = regexp_replace(itemdesc, '[^a-zA-Z0-9]', '', 'g')" +
                        $"\n where COALESCE(NULLIF(pm.producttype, ''), 'product') = 'serviceitem';";
            ExecPgNonQuery(strQuery);

            ReportProgress("Product default manufacturer updated successfully", 2);
        }


        public void fnControOrderUpdate(long nMainBranchId)
        {

            ReportProgress("Updating mainsetting in SQL Server...", 0);

            string strQuery = @"select * from ControlOrder";

            try
            {
                System.Data.DataTable dtsql = new System.Data.DataTable();
                using var connection = SqlServerConnection.Create();
                connection.Open();
                var query = string.Format(strQuery);
                using var command = new SqlCommand(query, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                adapter.Fill(dtsql);
                connection.Close();

                //strQuery = @"select * from mainsetting";
                //DataTable  dtposgres = new DataTable();
                //using var posconnection = PostgresConnection.Create();
                //posconnection.Open();
                //var posquery = string.Format(strQuery);
                //using var poscommand = new SqlCommand(query, connection);
                //adapter = new SqlDataAdapter(command);
                //adapter.Fill(dtposgres);
                //posconnection.Close();
                string strUpdateQuery = "";
                foreach (DataRow row in dtsql.Rows)
                {
                    string ControlName = row["ControlName"].ToString();
                    string ControlType = row["ControlType"].ToString();
                    Int32 ControlOrder = Convert.ToInt32(row["ControlOrder"].ToString());
                    bool Active = Convert.ToString(row["Active"].ToString()) == "0" ? false : true;

                    if (ControlType == "Return")
                    {
                        ControlType = "SalesReturn";
                    }
                    else if (ControlType == "Expiry")
                    {
                        ControlType = "ExpiryReturn";
                    }
                    strUpdateQuery += $"\n update controlorder set controlorder ='{ControlOrder}' , active ={Active} where controlname = '{ControlName}' and controltype ='{ControlType}' ;";

                }


                foreach (DataRow row in dtsql.Rows)
                {
                    string ControlName = row["ControlName"].ToString();
                    string ControlType = row["ControlType"].ToString();
                    Int32 ControlOrder = Convert.ToInt32(row["ControlOrder"].ToString());
                    bool Active = Convert.ToString(row["Active"].ToString()) == "0" ? false : true;

                    if (ControlType == "Return")
                    {
                        ControlType = "SalesReturn";
                    }
                    else if (ControlType == "Expiry")
                    {
                        ControlType = "ExpiryReturn";
                    }

                    strUpdateQuery += $"\n update controlordermainbranch set controlorder ='{ControlOrder}' , active ={Active} where controlname = '{ControlName}' and controltype ='{ControlType}' and mainbranchid={nMainBranchId} ;";

                }



                ExecPgNonQuery(strUpdateQuery);

                ReportProgress("Updating controlorder successfully", 2);
            }
            catch (Exception ex)
            {
                ReportProgress($"Updating controlorder failed: {ex.Message}", 2);
                throw; // abort so the branch transaction is rolled back
            }
        }


        public void fnBillNosUpdate(long nFromBranchId, long nMainBranchId, long nBranchId)
        {

            string strQuery = @"select * from branch where branchid=" + nFromBranchId;
            strQuery += @"select * from branchsetting where branchid=" + nFromBranchId;

            try
            {
                System.Data.DataSet dsDataSet = new System.Data.DataSet();
                using var connection = SqlServerConnection.Create();
                connection.Open();
                var query = string.Format(strQuery);
                using var command = new SqlCommand(query, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                adapter.Fill(dsDataSet);
                connection.Close();

                System.Data.DataTable dtbranch = new System.Data.DataTable();
                if (dsDataSet.Tables.Count > 0)
                {
                    dtbranch = dsDataSet.Tables[0];
                }


                System.Data.DataTable dtBranchSetting = new System.Data.DataTable();
                if (dsDataSet.Tables.Count > 1)
                {
                    dtBranchSetting = dsDataSet.Tables[1];
                }

                string strUpdateQuery = "";
                long nBillNo = 1;
                string strSettingName = "";

                foreach (DataRow dr in dtBranchSetting.Rows)
                {
                    strSettingName = Convert.ToString(dr["SettingName"]);
                    switch (strSettingName)
                    {
                        case "StockTransferNextNo":
                            nBillNo = Convert.ToInt64(dr["Value"].ToString());
                            strUpdateQuery += $"\n UPDATE billseries SET billsercurrentbillno = '{nBillNo}'";
                            strUpdateQuery += $"\n WHERE mainbranchid = '{nMainBranchId}' and branchid = {nBranchId} and billsersource = 'STOCK TRANSFER';";
                            break;
                        default:
                            break;
                    }
                }

                foreach (DataRow row in dtbranch.Rows)
                {
                    nBillNo = Convert.ToInt64(row["DNSlNo"].ToString());

                    strUpdateQuery += $"\n UPDATE billseries SET billsercurrentbillno = '{nBillNo}'";
                    strUpdateQuery += $"\n WHERE mainbranchid = '{nMainBranchId}' and branchid = {nBranchId} and billsersource = 'DEBIT NOTE';";

                    nBillNo = Convert.ToInt64(row["EDSlNo"].ToString());

                    strUpdateQuery += $"\n UPDATE billseries SET billsercurrentbillno = '{nBillNo}'";
                    strUpdateQuery += $"\n WHERE mainbranchid = '{nMainBranchId}' and branchid = {nBranchId} and billsersource = 'EXPIRY/DAMAGE DEBITNOTE';";

                    nBillNo = Convert.ToInt64(row["PurReturnNo"].ToString());

                    strUpdateQuery += $"\n UPDATE branchsetting SET settingbillno = '{nBillNo}'";
                    strUpdateQuery += $"\n WHERE mainbranchid = '{nMainBranchId}' and branchid = {nBranchId} and settingname = 'PurchaseReturnNo';";

                    nBillNo = Convert.ToInt64(row["QuoSlNo"].ToString());
                    strUpdateQuery += $"\n UPDATE billseries SET billsercurrentbillno = '{nBillNo}'";
                    strUpdateQuery += $"\n WHERE mainbranchid = '{nMainBranchId}' and branchid = {nBranchId} and billsersource = 'QUOTATION';";

                    nBillNo = Convert.ToInt64(row["OrderNo"].ToString());
                    strUpdateQuery += $"\n UPDATE billseries SET billsercurrentbillno = '{nBillNo}'";
                    strUpdateQuery += $"\n WHERE mainbranchid = '{nMainBranchId}' and branchid = {nBranchId} and billsersource = 'SALES ORDER';";


                    nBillNo = Convert.ToInt64(row["SRSlNo"].ToString());
                    strUpdateQuery += $"\n UPDATE billseries SET billsercurrentbillno = '{nBillNo}'";
                    strUpdateQuery += $"\n WHERE mainbranchid = '{nMainBranchId}' and branchid = {nBranchId} and billsersource = 'CREDIT NOTE';";



                    nBillNo = Convert.ToInt64(row["ERSlNo"].ToString());
                    strUpdateQuery += $"\n UPDATE billseries SET billsercurrentbillno = '{nBillNo}'";
                    strUpdateQuery += $"\n WHERE mainbranchid = '{nMainBranchId}' and branchid = {nBranchId} and billsersource = 'EXPIRY RETURN';";

                    nBillNo = Convert.ToInt64(row["NextBillNo"].ToString());
                    strUpdateQuery += $"\n update branchsetting set settingbillno = {nBillNo} where settingname ='SalesNexBillNo' and branchid = {nBranchId} and mainbranchid = {nMainBranchId};";


                    nBillNo = 1;

                    if (dsDataSet.Tables.Count > 1)
                    {
                        DataRow[] datarows = dsDataSet.Tables[1].Select("SettingName = 'OpeningStockBillNo' and branchid = " + nFromBranchId);
                        foreach (DataRow rows in datarows)
                        {
                            nBillNo = Convert.ToInt64(rows["Value"].ToString());
                        }
                    }

                    strUpdateQuery += $"\n UPDATE billseries SET billsercurrentbillno = '{nBillNo}'";
                    strUpdateQuery += $"\n WHERE mainbranchid = '{nMainBranchId}' and branchid = {nBranchId} and billsersource = 'OPENING STOCK ENTRY';";

                }

                // ServiceBillNextNo: continue service bill numbering from the highest migrated uniquebillno.
                // Reads through the shared transaction so it sees rows migrated earlier in the same transaction.
                long nServiceBillNo = 0;
                {
                    string strServiceQuery = $"select coalesce(max(uniquebillno), 0) from servicemain{nMainBranchId} where branchid = {nBranchId}";
                    using var poscommand = PgCmd(strServiceQuery);
                    var oResult = poscommand.ExecuteScalar();
                    if (oResult != null && oResult != DBNull.Value)
                    {
                        nServiceBillNo = Convert.ToInt64(oResult);
                    }
                }

                strUpdateQuery += $"\n UPDATE branchsetting SET settingbillno = '{nServiceBillNo + 10}'";
                strUpdateQuery += $"\n WHERE mainbranchid = '{nMainBranchId}' and branchid = {nBranchId} and settingname = 'ServiceBillNextNo';";

                ExecPgNonQuery(strUpdateQuery);

                ReportProgress("Updating BranchSetting  successfully", 2);
            }
            catch (Exception ex)
            {
                ReportProgress($"Updating BranchSetting  failed: {ex.Message}", 2);
                throw; // abort so the branch transaction is rolled back
            }
        }


        public void fnPrintFileNameUpdate(long nFromBranchId, long nMainBranchId, long nBranchId)
        {


            string strQuery = "\n select * from settings";
            strQuery += "\n select * from branchsetting where branchid=" + nFromBranchId;

            try
            {
                System.Data.DataSet dsDataSet = new System.Data.DataSet();
                using var connection = SqlServerConnection.Create();
                connection.Open();
                var query = string.Format(strQuery);
                using var command = new SqlCommand(query, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                adapter.Fill(dsDataSet);
                connection.Close();

                System.Data.DataTable dtbranch = new System.Data.DataTable();
                if (dsDataSet.Tables.Count > 0)
                {
                    dtbranch = dsDataSet.Tables[0];
                }


                System.Data.DataTable dtBranchSetting = new System.Data.DataTable();
                if (dsDataSet.Tables.Count > 1)
                {
                    dtBranchSetting = dsDataSet.Tables[1];
                }

                //strQuery = @"select * from branchsetting where branchid="+nBranchId+ " and mainbranchid=";
                //DataTable dtposgres = new DataTable();
                //using var posconnection = PostgresConnection.Create();
                //posconnection.Open();
                //var posquery = string.Format(strQuery);
                //using var poscommand = new SqlCommand(query, connection);
                //adapter = new SqlDataAdapter(command);
                //adapter.Fill(dtposgres);
                //posconnection.Close();

                string strUpdateQuery = "";
                string strPrintFileName = "", strPrintPreviewName = "";


                if (dsDataSet.Tables.Count > 1)
                {
                    strPrintFileName = ""; strPrintPreviewName = "";
                    DataRow[] datarows = dsDataSet.Tables[1].Select("SettingName = 'DebitNotePrint' and branchid = " + nFromBranchId);
                    foreach (DataRow rows in datarows)
                    {
                        strPrintFileName = Convert.ToString(rows["Value"].ToString());
                    }

                    datarows = dsDataSet.Tables[1].Select("SettingName = 'DebitNotePrintPreview' and branchid = " + nFromBranchId);
                    foreach (DataRow rows in datarows)
                    {
                        strPrintPreviewName = Convert.ToString(rows["Value"].ToString());
                    }

                    strUpdateQuery += $"\n UPDATE billseries SET printfilename = '{strPrintFileName}',printfilepreview='{strPrintPreviewName}'";
                    strUpdateQuery += $"\n WHERE mainbranchid = '{nMainBranchId}' and branchid = {nBranchId} and billsersource = 'DEBIT NOTE';";


                    strPrintFileName = ""; strPrintPreviewName = "";
                    datarows = dsDataSet.Tables[1].Select("SettingName = 'DeliveryChallanPrintFileName' and branchid = " + nFromBranchId);
                    foreach (DataRow rows in datarows)
                    {
                        strPrintFileName = Convert.ToString(rows["Value"].ToString());
                    }



                    strUpdateQuery += $"\n UPDATE billseries SET printfilename = '{strPrintFileName}',printfilepreview='{strPrintFileName}'";
                    strUpdateQuery += $"\n WHERE mainbranchid = '{nMainBranchId}' and branchid = {nBranchId} and billsersource = 'DELIVERYOUT';";


                    strPrintFileName = ""; strPrintPreviewName = "";
                    datarows = dsDataSet.Tables[1].Select("SettingName = 'ExpiryDebitNotePrint' and branchid = " + nFromBranchId);
                    foreach (DataRow rows in datarows)
                    {
                        strPrintFileName = Convert.ToString(rows["Value"].ToString());
                    }

                    datarows = dsDataSet.Tables[1].Select("SettingName = 'ExpiryDebitNotePrintPreview' and branchid = " + nFromBranchId);
                    foreach (DataRow rows in datarows)
                    {
                        strPrintPreviewName = Convert.ToString(rows["Value"].ToString());
                    }

                    strUpdateQuery += $"\n UPDATE billseries SET printfilename = '{strPrintFileName}',printfilepreview='{strPrintPreviewName}'";
                    strUpdateQuery += $"\n WHERE mainbranchid = '{nMainBranchId}' and branchid = {nBranchId} and billsersource = 'EXPIRY/DAMAGE DEBITNOTE';";


                    strPrintFileName = ""; strPrintPreviewName = "";
                    datarows = dsDataSet.Tables[1].Select("SettingName = 'SalesReturnPrint' and branchid = " + nFromBranchId);
                    foreach (DataRow rows in datarows)
                    {
                        strPrintFileName = Convert.ToString(rows["Value"].ToString());
                    }


                    strUpdateQuery += $"\n UPDATE billseries SET printfilename = '{strPrintFileName}',printfilepreview='{strPrintFileName}'";
                    strUpdateQuery += $"\n WHERE mainbranchid = '{nMainBranchId}' and branchid = {nBranchId} and billsersource = 'CREDIT NOTE';";


                    strPrintFileName = ""; strPrintPreviewName = "";
                    datarows = dsDataSet.Tables[1].Select("SettingName = 'ExpiryReturnPrintName' and branchid = " + nFromBranchId);
                    foreach (DataRow rows in datarows)
                    {
                        strPrintFileName = Convert.ToString(rows["Value"].ToString());
                    }

                    datarows = dsDataSet.Tables[1].Select("SettingName = 'ExpiryReturnPreview' and branchid = " + nFromBranchId);
                    foreach (DataRow rows in datarows)
                    {
                        strPrintPreviewName = Convert.ToString(rows["Value"].ToString());
                    }

                    strUpdateQuery += $"\n UPDATE billseries SET printfilename = '{strPrintFileName}',printfilepreview='{strPrintFileName}'";
                    strUpdateQuery += $"\n WHERE mainbranchid = '{nMainBranchId}' and branchid = {nBranchId} and billsersource = 'EXPIRY RETURN';";

                }

                //  strUpdateQuery += $"\n update printdisplaysettings set printname = 'CreditNotePrintModelOne' where printname = 'PrintModelCreditNote';";

                strUpdateQuery += $"\n UPDATE billseries SET printfilename = 'CreditNotePrintModelOne',printfilepreview='CreditNotePrintModelOne'";
                strUpdateQuery += $"\n WHERE printfilename = 'PrintModelCreditNote' and mainbranchid = '{nMainBranchId}' and branchid = {nBranchId} and billsersource = 'CREDIT NOTE';";

                ExecPgNonQuery(strUpdateQuery);

                ReportProgress("Updating BranchSetting  successfully", 2);
            }
            catch (Exception ex)
            {
                ReportProgress($"Updating BranchSetting  failed: {ex.Message}", 2);
                throw; // abort so the branch transaction is rolled back
            }
        }

        public void fnHsnUpdate(long nMainBranchId, long nBranchId)
        {
            try
            {
                string strUpdateQuery = "";

                strUpdateQuery += $@"
                    INSERT INTO public.hsn1(
                        hsn_code,hsn_gstpers,hsn_description1,hsn_description2,unitid,hsn_cess,hsn_additionalcess,hsn_othercharge,
                        hsn_addrateperunit,hsn_thousandrateperunit,branchid, mainbranchid, taxid,tempid
                    )
                    select distinct
                    hsncode,0 hsn_gstpers,'' hsn_description1,'' hsn_description2,0 unitid,0 hsn_cess,0 hsn_additionalcess,0 hsn_othercharge,
                    0 hsn_addrateperunit,0 hsn_thousandrateperunit, branchid, mainbranchid, taxid,0 tempid
                    from productmain{nMainBranchId} pm where pm.hsnid=0 and pm.branchid={nBranchId} and pm.mainbranchid={nMainBranchId};";

                strUpdateQuery += $@"
                    update productmain{nMainBranchId} pm set hsnid = hs.hsn_id from hsn{nMainBranchId} hs where pm.hsncode=hs.hsn_code and pm.taxid=hs.taxid and pm.hsnid=0
                    and pm.branchid={nBranchId} and pm.mainbranchid={nMainBranchId};";

                ExecPgNonQuery(strUpdateQuery);

                ReportProgress("Updating HSN successfully", 2);
            }
            catch (Exception ex)
            {
                ReportProgress($"Updating HSN failed: {ex.Message}", 2);
                throw; // abort so the branch transaction is rolled back
            }
        }

        public void fnServiceItemInsertProductSub(long nMainBranchId, long nBranchId)
        {
            try
            {
                string strUpdateQuery = "";

                strUpdateQuery += $@"
                    INSERT INTO productsub{nMainBranchId}(
                	productid, location, purrate, selrate, whrate, mrp, sprate1, sprate2, sprate3, sprate4, sprate5, 
	                pcsselrate, pcswhrate, pcsmrp, pcssprate1, pcssprate2, pcssprate3, pcssprate4, pcssprate5, bonline, neethidis, binventoryitem, 
	                salesdiscount, rolqty, branchid, mainbranchid, tempid)
	                select 	productid,'' location,0 purrate,prodweight selrate,0 whrate,0 mrp,0 sprate1,0 sprate2,0 sprate3,0 sprate4,0 sprate5, 
	                0 pcsselrate,0  pcswhrate,0  pcsmrp,0  pcssprate1,0  pcssprate2,0  pcssprate3,0  pcssprate4,0  pcssprate5,False bonline,0  neethidis,True binventoryitem, 
	                0 salesdiscount,0  rolqty, branchid, mainbranchid,0  tempid
	               from productmain{nMainBranchId} pm where pm.producttype='serviceitem' and pm.branchid={nBranchId} and pm.mainbranchid={nMainBranchId};";

                ExecPgNonQuery(strUpdateQuery);

                ReportProgress("Updating HSN successfully", 2);
            }
            catch (Exception ex)
            {
                ReportProgress($"Updating HSN failed: {ex.Message}", 2);
                throw; // abort so the branch transaction is rolled back
            }
        }


        public void fnSalesRetLogInsert(long nFromBranchId, long nMainBranchId, long nBranchId)
        {

            ReportProgress("Updating SalesReturnLog in SQL Server...", 0);
            string strUpdateQuery = "";

            string strQuery = $@"select CrditNoteNos,ExpCrNoteDates,Trans_VoucherNo SalesVoucherNo,SalesVoucherUniqueId SalesVoucherId,BillSerId,Issue_SlNo issueno,UniqueBillNo,AcId
                                from Issue where isnull(CrditNoteNos,'') <> '' or isnull(ExpCrNoteDates,'') <> '' and BranchId={nFromBranchId};";

            try
            {
                System.Data.DataTable dtIssue = new System.Data.DataTable();
                using var connection = SqlServerConnection.Create();
                connection.Open();

                using (var command = new SqlCommand(strQuery, connection))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    adapter.Fill(dtIssue);
                }

                foreach (DataRow row in dtIssue.Rows)
                {
                    string CrditNoteNos = row["CrditNoteNos"].ToString();
                    string ExpCrNoteDates = row["ExpCrNoteDates"].ToString();
                    string SalesVoucherNo = row["SalesVoucherNo"].ToString();
                    string SalesVoucherId = row["SalesVoucherId"].ToString();
                    string BillSerId = row["BillSerId"].ToString();
                    string issueno = row["issueno"].ToString();
                    string UniqueBillNo = row["UniqueBillNo"].ToString();
                    string AcId = row["AcId"].ToString();

                    // Credit-note list drives a SalesReturn lookup; otherwise the expiry list drives an ExpiryReturn lookup.
                    bool bIsCreditNote = CrditNoteNos.Trim() != "";
                    string strNoteList = bIsCreditNote ? CrditNoteNos : ExpCrNoteDates;

                    // Format is 'a@b','c@d' ... : split into pairs, then split each pair on '@'.
                    foreach (string strPair in strNoteList.Split(','))
                    {
                        string strClean = strPair.Trim().Trim('\'').Trim();
                        if (strClean == "")
                            continue;

                        string[] parts = strClean.Split('@');
                        if (parts.Length < 2)
                            continue;

                        string strRetSlNo = parts[0].Trim();
                        string strRetUniqueNo = parts[1].Trim();
                        if (strRetSlNo == "" || strRetUniqueNo == "")
                            continue;

                        string strRetQuery = bIsCreditNote
                            ? $"select Trans_VoucherNo,SalesVoucherUniqueId,Issue_CrAmt returnamt,'SalesReturn' returntype from IssueReturn where IssueRetSlNo={strRetSlNo} and UniqueNo={strRetUniqueNo} and branchid={nFromBranchId}"
                            : $"select Trans_VoucherNo,SalesVoucherUniqueId,Expiry_Total returnamt,'ExpiryReturn' returntype from ExpiryReturn where ExpiryRetSlNo={strRetSlNo} and Expiry_Id={strRetUniqueNo} and BranchId={nFromBranchId}";

                        System.Data.DataTable dtRet = new System.Data.DataTable();
                        using (var retcommand = new SqlCommand(strRetQuery, connection))
                        {
                            SqlDataAdapter retadapter = new SqlDataAdapter(retcommand);
                            retadapter.Fill(dtRet);
                        }

                        foreach (DataRow retrow in dtRet.Rows)
                        {
                            string RetVoucherNo = retrow["Trans_VoucherNo"].ToString();
                            string RetUniqueVoucherId = retrow["SalesVoucherUniqueId"].ToString();
                            string returnamt = retrow["returnamt"].ToString();
                            string returntype = retrow["returntype"].ToString();

                            strUpdateQuery += "\n INSERT INTO saleretlog" + nMainBranchId + "("
                                + "salvprefixid, salvoucherno, saluniquevoucherid, retvprefixid, retvoucherno, retuniquevoucherid, "
                                + "amount, adjamount, branchid, mainbranchid, acid, returntype, billserid, issueno, uniquebillno) VALUES ("
                                + "5,'" + SalesVoucherNo + "','" + SalesVoucherId + "',5,'" + RetVoucherNo + "','" + RetUniqueVoucherId + "',"
                                + "'" + returnamt + "','" + returnamt + "','" + nBranchId + "','" + nMainBranchId + "','" + AcId + "','" + returntype + "','"
                                + BillSerId + "','" + issueno + "','" + UniqueBillNo + "');";
                        }
                    }
                }



                strUpdateQuery += $"\n UPDATE saleretlog{nMainBranchId} im SET acid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = im.acid AND im.branchid = {nBranchId} AND im.mainbranchid = {nMainBranchId};";


                strUpdateQuery += $"\n update saleretlog{nMainBranchId} im set billserid =  bs.billserid from billseries bs where bs.tempid = im.billserid";
                strUpdateQuery += $"\n and bs.branchid = im.branchid and bs.mainbranchid = im.mainbranchid";
                strUpdateQuery += $"\n and im.branchid ={nBranchId}    and im.mainbranchid ={nMainBranchId} and bs.billsersource='SALES' and  bs.branchid = {nBranchId} and bs.mainbranchid = {nMainBranchId}";


                connection.Close();

                if (strUpdateQuery != "")
                    ExecPgNonQuery(strUpdateQuery);

                ReportProgress("Updating SalesReturnLog successfully", 2);
            }
            catch (Exception ex)
            {
                ReportProgress($"Updating SalesReturnLog failed: {ex.Message} Update Query {strUpdateQuery}", 2);
                throw; // abort so the branch transaction is rolled back
            }
        }


        public void fnTotalQtyUpdateTransaction(long nMainBranchId, long nBranchId)
        {

            ReportProgress("Updating SalesReturnLog in SQL Server...", 0);
            string strUpdateQuery = "";

            try
            {

                strUpdateQuery += $"\n update issuesubdetails{nMainBranchId} set totqty=qty+freqty+advfre-rqty where branchid={nBranchId} and mainbranchid={nMainBranchId};";
                strUpdateQuery += $"\n update issuereturndetails{nMainBranchId} set totqty=qty+freqty+advfre where branchid={nBranchId} and mainbranchid={nMainBranchId};";
                strUpdateQuery += $"\n update deliveryoutdetails{nMainBranchId} set totqty=qty+freqty+advfre where branchid={nBranchId} and mainbranchid={nMainBranchId};";
                strUpdateQuery += $"\n update receiptreturndetails{nMainBranchId} set totqty=receiptretqty where branchid={nBranchId} and mainbranchid={nMainBranchId};";


                if (strUpdateQuery != "")
                    ExecPgNonQuery(strUpdateQuery);

                ReportProgress("Updating SalesReturnLog successfully", 2);
            }
            catch (Exception ex)
            {
                ReportProgress($"Updating SalesReturnLog failed: {ex.Message} Update Query {strUpdateQuery}", 2);
                throw; // abort so the branch transaction is rolled back
            }
        }


        public void fnUserPrevilegeMainUpdate(long nMainBranchId, long nBranchId)
        {

            ReportProgress("Updating user privilege in PostgreSQL...", 0);

            string strQuery = $"select distinct accesslevel from accounthead{nMainBranchId} where accesslevel>0;";
            strQuery += $"\n select * from accesslevel where mainbranchid={nMainBranchId} and branchid={nBranchId};";

            string strUpdateQuery = "";

            try
            {
                System.Data.DataSet dsDataSet = new System.Data.DataSet();
                using (var command = PgCmd(strQuery))
                {
                    NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(command);
                    adapter.Fill(dsDataSet);
                }

                System.Data.DataTable dtAccessLevelUsed = new System.Data.DataTable();
                if (dsDataSet.Tables.Count > 0)
                {
                    dtAccessLevelUsed = dsDataSet.Tables[0];
                }

                System.Data.DataTable dtAccessLevel = new System.Data.DataTable();
                if (dsDataSet.Tables.Count > 1)
                {
                    dtAccessLevel = dsDataSet.Tables[1];
                }

                DataRow[] accessRows;
                long nAccessId = 0;

                foreach (DataRow row in dtAccessLevelUsed.Rows)
                {
                    long nAccessLevel = Convert.ToInt64(row["accesslevel"]);

                    string strAccessName;
                    switch (nAccessLevel)
                    {
                        case 1:
                            strAccessName = "SuperAdmin";
                            accessRows = dtAccessLevel.Select($"accessname = '{strAccessName}'");
                            if (accessRows.Length != 0)
                            {
                                nAccessId = Convert.ToInt64(accessRows[0]["accessid"]);
                                strUpdateQuery += $"\n update accounthead{nMainBranchId} set accesslevel={nAccessId} where accesslevel={nAccessLevel} and branchid={nBranchId} and mainbranchid={nMainBranchId};";
                            }
                            break;
                        case 2:
                            strAccessName = "Admin";
                            accessRows = dtAccessLevel.Select($"accessname = '{strAccessName}'");
                            if (accessRows.Length != 0)
                            {
                                nAccessId = Convert.ToInt64(accessRows[0]["accessid"]);
                                strUpdateQuery += $"\n update accounthead{nMainBranchId} set accesslevel={nAccessId} where accesslevel={nAccessLevel} and branchid={nBranchId} and mainbranchid={nMainBranchId};";
                            }
                            break;
                        case 3:
                            strAccessName = "Manager";
                            accessRows = dtAccessLevel.Select($"accessname = '{strAccessName}'");
                            if (accessRows.Length != 0)
                            {
                                nAccessId = Convert.ToInt64(accessRows[0]["accessid"]);
                                strUpdateQuery += $"\n update accounthead{nMainBranchId} set accesslevel={nAccessId} where accesslevel={nAccessLevel} and branchid={nBranchId} and mainbranchid={nMainBranchId};";
                            }
                            break;
                        case 4:
                            strAccessName = "Staff";
                            accessRows = dtAccessLevel.Select($"accessname = '{strAccessName}'");
                            if (accessRows.Length != 0)
                            {
                                nAccessId = Convert.ToInt64(accessRows[0]["accessid"]);
                                strUpdateQuery += $"\n update accounthead{nMainBranchId} set accesslevel={nAccessId} where accesslevel={nAccessLevel} and branchid={nBranchId} and mainbranchid={nMainBranchId};";
                            }
                            break;
                        case 5:
                            strAccessName = "Rep";
                            accessRows = dtAccessLevel.Select($"accessname = '{strAccessName}'");
                            if (accessRows.Length != 0)
                            {
                                nAccessId = Convert.ToInt64(accessRows[0]["accessid"]);
                                strUpdateQuery += $"\n update accounthead{nMainBranchId} set accesslevel={nAccessId} where accesslevel={nAccessLevel} and branchid={nBranchId} and mainbranchid={nMainBranchId};";
                            }
                            break;
                        default:
                            continue;
                    }

                }

                if (strUpdateQuery != "")
                    ExecPgNonQuery(strUpdateQuery);

                ReportProgress("Updating user privilege successfully", 2);
            }
            catch (Exception ex)
            {
                ReportProgress($"Updating user privilege failed: {ex.Message} Update Query {strUpdateQuery}", 2);
                throw; // abort so the branch transaction is rolled back
            }
        }


        public void fnUserPrevilegeUpdate(long nMainBranchId, long nBranchId)
        {

            ReportProgress("Updating user privilege...", 0);

            // ---- SQL Server (source): privilege header + detail rows ----
            string strSqlQuery = "select * from UserPrevilage where PrevilageName>0;";
            strSqlQuery += "\n select * from UserPrevilageDetails where AccessLevelId>0;";

            string strUpdateQuery = "";

            try
            {
                System.Data.DataSet dsSql = new System.Data.DataSet();
                using (var connection = SqlServerConnection.Create())
                {
                    connection.Open();
                    using (var command = new SqlCommand(strSqlQuery, connection))
                    {
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        adapter.Fill(dsSql);
                    }
                    connection.Close();
                }

                System.Data.DataTable dtUserPrevilage = new System.Data.DataTable();
                if (dsSql.Tables.Count > 0)
                    dtUserPrevilage = dsSql.Tables[0];

                System.Data.DataTable dtUserPrevilageDetails = new System.Data.DataTable();
                if (dsSql.Tables.Count > 1)
                    dtUserPrevilageDetails = dsSql.Tables[1];

                // ---- PostgreSQL (target): access levels for this branch ----
                System.Data.DataTable dtAccessLevel = new System.Data.DataTable();
                using (var pgCommand = PgCmd($"select * from accesslevel where mainbranchid={nMainBranchId} and branchid={nBranchId};"))
                {
                    NpgsqlDataAdapter pgAdapter = new NpgsqlDataAdapter(pgCommand);
                    pgAdapter.Fill(dtAccessLevel);
                }

                // Boolean flag columns on UserPrevilage; each column that is set becomes a privilagedetail row.
                string[] arrPrivilegeKeys = new string[]
                {
                    "PurEdit", "SalesEdit", "PurCancel", "SalesCancel", "AccountEdit",
                    "AccountCancel", "PRateVisible", "SalesRateEdit", "AddStaff", "NameEdit"
                };

                // ---- Pass 1: map each privilege level -> access id, update accounthead, save the true flags ----
                foreach (DataRow row in dtUserPrevilage.Rows)
                {
                    long nAccessLevel = Convert.ToInt64(row["PrevilageName"]);
                    string strAccessName = fnGetAccessName(nAccessLevel);
                    if (strAccessName == "")
                        continue;

                    DataRow[] accessRows = dtAccessLevel.Select($"accessname = '{strAccessName}'");
                    if (accessRows.Length == 0)
                        continue;

                    long nAccessId = Convert.ToInt64(accessRows[0]["accessid"]);

                    //strUpdateQuery += $"\n update accounthead{nMainBranchId} set accesslevel={nAccessId} where accesslevel={nAccessLevel} and branchid={nBranchId} and mainbranchid={nMainBranchId};";

                    foreach (string strKey in arrPrivilegeKeys)
                    {
                        if (fnIsFlagTrue(row, strKey))
                            strUpdateQuery += fnUserPrevilegeDetailsSave(strKey, nMainBranchId, nBranchId, nAccessId, strAccessName, 0);
                    }
                }

                // ---- Pass 2: save the per-key detail rows already present in UserPrevilageDetails ----
                foreach (DataRow row in dtUserPrevilageDetails.Rows)
                {
                    long nAccessLevel = Convert.ToInt64(row["AccessLevelId"]);
                    string strAccessName = fnGetAccessName(nAccessLevel);
                    if (strAccessName == "")
                        continue;

                    DataRow[] accessRows = dtAccessLevel.Select($"accessname = '{strAccessName}'");
                    if (accessRows.Length == 0)
                        continue;

                    long nAccessId = Convert.ToInt64(accessRows[0]["accessid"]);
                    long nEditDays = Convert.ToInt64(row["AccessLevelValue"].ToString());

                    strUpdateQuery += fnUserPrevilegeDetailsSave(row["AccessLevelKey"].ToString(), nMainBranchId, nBranchId, nAccessId, strAccessName, nEditDays);
                }

                if (strUpdateQuery != "")
                    ExecPgNonQuery(strUpdateQuery);

                ReportProgress("Updating user privilege successfully", 2);
            }
            catch (Exception ex)
            {
                ReportProgress($"Updating user privilege failed: {ex.Message} Update Query {strUpdateQuery}", 2);
                throw; // abort so the branch transaction is rolled back
            }
        }

        /// <summary>Maps a numeric privilege level (1-5) to its access-level name.</summary>
        private string fnGetAccessName(long nAccessLevel)
        {
            switch (nAccessLevel)
            {
                case 1: return "SuperAdmin";
                case 2: return "Admin";
                case 3: return "Manager";
                case 4: return "Staff";
                case 5: return "Rep";
                default: return "0";
            }
        }

        /// <summary>True when a bit/boolean flag column on the row is set.</summary>
        private bool fnIsFlagTrue(DataRow row, string strColumn)
        {
            if (!row.Table.Columns.Contains(strColumn))
                return false;
            object val = row[strColumn];
            if (val == null || val == DBNull.Value)
                return false;
            if (val is bool b)
                return b;

            string s = val.ToString().Trim();
            return s == "1" || s.Equals("true", StringComparison.OrdinalIgnoreCase) || s.Equals("yes", StringComparison.OrdinalIgnoreCase);
        }


        private string fnUserPrevilegeDetailsSave(string strPrivilegeKey, long nMainBranchId, long nBranchId, long nAccessId, string strPrivilegeName, long nEditDays)
        {
            string sql = $@"
                   WITH up AS (
                      INSERT INTO userprivilage(privilagename, branchid, mainbranchid, tempid, accessid)
                      SELECT '{strPrivilegeName}', {nBranchId}, {nMainBranchId}, 0, {nAccessId}
                      WHERE NOT EXISTS (SELECT 1 FROM userprivilage WHERE accessid={nAccessId} AND branchid={nBranchId} AND mainbranchid={nMainBranchId})
                      RETURNING privilageid
                    )
                   INSERT INTO privilagedetail(privilageid, privilegekey, privilegeeditdays, privilegevalue, branchid, mainbranchid, tempid)
                   SELECT COALESCE((SELECT privilageid FROM up),
                  (SELECT privilageid FROM userprivilage WHERE accessid={nAccessId} AND branchid={nBranchId} AND mainbranchid={nMainBranchId})),
                 '{strPrivilegeKey}', {nEditDays}, 'Yes', {nBranchId}, {nMainBranchId}, 0;";
            return sql;
        }


        public void fnSqlServerTableValueUpdate(long nFromBranchId)
        {
            // Remove ProductSub rows whose ProductId no longer exists in Product for this branch.
            string strQuery =
                "\n delete from ProductSub where productid not in (select ProductId from Product)" +
                $"\n and BranchId={nFromBranchId}";

            using var conn = SqlServerConnection.Create();
            conn.Open();
            using var cmd = new SqlCommand(strQuery, conn);
            cmd.ExecuteNonQuery();
        }
    }


}
