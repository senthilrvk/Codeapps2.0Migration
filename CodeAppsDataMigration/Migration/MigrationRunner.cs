using CodeAppsDataMigration.Data;
using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.Data.SqlClient;
using Npgsql;
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;

namespace CodeAppsDataMigration.Migration
{
    public class MigrationRunner
    {
        private readonly string _sql;
        private readonly string _pg;
        private Action<string, int>? _onProgress;

        public MigrationRunner(string sql, string pg)
        {
            _sql = sql;
            _pg = pg;
        }

        public void SetProgressCallback(Action<string, int> onProgress)
        {
            _onProgress = onProgress;
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

                    int rows = migrator.Run(table);

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



        private static readonly string[] UpdateQueries =
         [
          "UPDATE accounthead{0} ah SET areaid = a.area_id FROM area a WHERE a.tempid = ah.areaid AND ah.branchid = {1}",
          "UPDATE issuesubdetails{0} isub SET productid = pm.productid FROM productmain pm WHERE pm.tempid = isub.productid AND isub.branchid = {1}",
          "UPDATE issuemain{0} im SET acid = ah.acid FROM accounthead ah WHERE ah.tempid = im.acid AND im.branchid = {1}",
          "UPDATE productmain{0} pm SET categoryid = ca.categoryid FROM category ca WHERE ca.tempid = pm.categoryid AND pm.branchid = {1}",
          "UPDATE productmain{0} pm SET taxid = tx.taxid FROM tax tx WHERE tx.taxpercent = pm.prodlinkeshopid AND pm.branchid = {1}",
          "UPDATE productmain{0} pm SET manufacture_id = mf.manufacture_id FROM manufacture mf WHERE mf.tempid = pm.manufacture_id AND pm.branchid = {1}",
          "UPDATE productmain{0} pm SET hsnid = hs.hsn_id FROM hsn hs WHERE hs.tempid = pm.hsnid AND pm.branchid = {1}",
          "UPDATE hsn{0} hs SET taxid = pm.taxid FROM productmain{0} pm WHERE hs.hsn_id = pm.hsnid AND pm.branchid = {1}",
         ];



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

            try
            {
                using var pg = new NpgsqlConnection(_pg);
                pg.Open();

                foreach (var (table, extra) in cleanups)
                {
                    string sql = $"DELETE FROM {table} WHERE branchid = {nBranchId} AND mainbranchid = {nMainBranchId}{extra}";
                    using var cmd = new NpgsqlCommand(sql, pg) { CommandTimeout = 120 };
                    int affected = cmd.ExecuteNonQuery();
                    ReportProgress($"Cleaned {table}: {affected} row(s) removed", 0);
                    Console.WriteLine($" {table} → {affected} row(s) deleted");
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(" Pre-migration cleanup failed: " + ex.Message);
                Console.ResetColor();
                throw;
            }
        }

        private void ExecuteBulkUpdates(Int64 nMainBranchId, Int64 nBranchId,Int64 nFromBranchId)
        {
            string testquerytemplate = "";
            try
            {

                List<string> stringBuilder = new List<string>();
                stringBuilder.Add($"UPDATE accounthead{nMainBranchId} ah SET areaid = a.area_id FROM area a WHERE a.tempid = ah.areaid AND ah.branchid = {nBranchId} and ah.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"UPDATE accounthead{nMainBranchId} ah SET bankflag = true  WHERE ah.supplytype = 'Yes' AND ah.branchid = {nBranchId} and ah.mainbranchid = {nMainBranchId}");

                stringBuilder.Add($"UPDATE productmain{nMainBranchId} pm SET categoryid = ca.categoryid FROM category ca WHERE ca.tempid = pm.categoryid AND pm.branchid = {nBranchId} and pm.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"UPDATE productmain{nMainBranchId} pm SET taxid = tx.taxid FROM tax tx WHERE tx.taxpercent = pm.prodlinkeshopid AND pm.branchid = {nBranchId} and pm.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"UPDATE productmain{nMainBranchId} pm SET manufacture_id = mf.manufacture_id FROM manufacture{nMainBranchId} mf WHERE mf.tempid = pm.manufacture_id AND pm.branchid = {nBranchId} and pm.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"UPDATE productmain{nMainBranchId} pm SET hsnid = hs.hsn_id FROM hsn{nMainBranchId} hs WHERE hs.tempid = pm.hsnid AND pm.branchid = {nBranchId} and pm.mainbranchid = {nMainBranchId}");

                stringBuilder.Add($"UPDATE productsub{nMainBranchId} isub SET productid = pm.productid FROM productmain{nMainBranchId} pm WHERE pm.tempid = isub.productid AND isub.branchid = {nBranchId} and isub.mainbranchid = {nMainBranchId}");

                stringBuilder.Add($"UPDATE hsn{nMainBranchId} hs SET taxid = pm.taxid FROM productmain{nMainBranchId} pm WHERE hs.hsn_id = pm.hsnid AND pm.branchid = {nBranchId} and pm.mainbranchid = {nMainBranchId}");

                // Sales
                stringBuilder.Add($"UPDATE issuesubdetails{nMainBranchId} isub SET productid = pm.productid FROM productmain{nMainBranchId} pm WHERE pm.tempid = isub.productid AND isub.branchid = {nBranchId} and isub.mainbranchid = {nMainBranchId}");
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



                // Purchase
                stringBuilder.Add($"UPDATE receiptdetails{nMainBranchId} isub SET productid = pm.productid FROM productmain{nMainBranchId} pm WHERE pm.tempid = isub.productid AND isub.branchid = {nBranchId} and isub.mainbranchid = {nMainBranchId}");
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
                stringBuilder.Add($"UPDATE store{nMainBranchId} isub SET productid = pm.productid FROM productmain{nMainBranchId} pm WHERE pm.tempid = isub.productid AND isub.branchid = {nBranchId} and isub.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"UPDATE store{nMainBranchId} im SET acid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = im.acid AND im.branchid = {nBranchId} and im.mainbranchid = {nMainBranchId}");

                strQuery = $"update store{nMainBranchId} st set receiptid =  rsub.receiptid from receiptdetails{nMainBranchId} rsub where rsub.priceid = st.receiptid";
                strQuery += $"\n and st.branchid = rsub.branchid and st.mainbranchid = rsub.mainbranchid and st.receiptno=rsub.receiptno and st.batchslno=rsub.batchslno";
                strQuery += $"\n and st.productid = rsub.productid and rsub.branchid ={nBranchId}    and rsub.mainbranchid ={nMainBranchId}";
                stringBuilder.Add(strQuery);

                stringBuilder.Add($"update receiptmain{nMainBranchId}    set paytermsid = 0  where branchid = {nBranchId} and mainbranchid = {nMainBranchId};");
                stringBuilder.Add($"update receiptdetails{nMainBranchId} set priceid    = 0  where branchid = {nBranchId} and mainbranchid = {nMainBranchId}");



                ///opening stock main
                stringBuilder.Add($"UPDATE openingstockmain{nMainBranchId} os SET acid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = os.acid AND os.branchid = {nBranchId} and os.mainbranchid = {nMainBranchId}");


                ///opening stock details
                stringBuilder.Add($"UPDATE openingstockdetails{nMainBranchId} os SET productid = pm.productid FROM productmain{nMainBranchId} pm WHERE pm.tempid = os.productid AND os.branchid = {nBranchId} and os.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"UPDATE openingstockdetails{nMainBranchId} pm SET taxid = tx.taxid FROM tax tx WHERE tx.taxpercent = pm.taxpers AND pm.branchid = {nBranchId} and pm.mainbranchid = {nMainBranchId}");
                strQuery = $"update openingstockdetails{nMainBranchId} rsub set openingstockid =  rm.openingstockid from openingstockmain{nMainBranchId} rm where rm.paytermsid = rsub.dcinno";
                strQuery += $"\n and rsub.branchid = rm.branchid and rsub.mainbranchid = rm.mainbranchid and rsub.openingstockno=rm.openingstockno";
                strQuery += $"\n and rm.branchid ={nBranchId}    and rm.mainbranchid ={nMainBranchId}";
                stringBuilder.Add(strQuery);

                stringBuilder.Add($"update openingstockmain{nMainBranchId}    set paytermsid = 0  where branchid = {nBranchId} and mainbranchid = {nMainBranchId};");
                stringBuilder.Add($"update openingstockdetails{nMainBranchId}    set dcinno = 0  where branchid = {nBranchId} and mainbranchid = {nMainBranchId};");


                /// receipt return main
                stringBuilder.Add($"UPDATE receiptreturnmain{nMainBranchId} rm SET staffid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.staffid AND rm.branchid = {nBranchId} and rm.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"UPDATE receiptreturnmain{nMainBranchId} rm SET reasonid = ah.categoryid FROM category ah WHERE ah.tempid = rm.reasonid AND rm.branchid = {nBranchId} and rm.mainbranchid = {nMainBranchId}");

                stringBuilder.Add($"UPDATE receiptreturndetails{nMainBranchId} rm SET staffid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.staffid AND rm.branchid = {nBranchId} and rm.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"UPDATE receiptreturndetails{nMainBranchId} os SET productid = pm.productid FROM productmain{nMainBranchId} pm WHERE pm.tempid = os.productid AND os.branchid = {nBranchId} and os.mainbranchid = {nMainBranchId}");
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
                stringBuilder.Add($"UPDATE debitnotemain{nMainBranchId} rm SET entrytype='product' WHERE rm.branchid = {nBranchId} and rm.mainbranchid = {nMainBranchId} ");

                //debitnotedetails
                stringBuilder.Add($"UPDATE debitnotedetails{nMainBranchId} os SET productid = pm.productid FROM productmain{nMainBranchId} pm WHERE pm.tempid = os.productid AND os.branchid = {nBranchId} And os.mainbranchid = {nMainBranchId}");
                strQuery = $"update debitnotedetails{nMainBranchId} rsub set debitnoteid =  rm.debitnoteid from debitnotemain{nMainBranchId} rm where rm.billserid = rsub.debitnoteid";
                strQuery += $"\n and rsub.branchid = rm.branchid and rsub.mainbranchid = rm.mainbranchid and rsub.debitnoteno=rm.debitnoteno";
                strQuery += $"\n and rm.branchid ={nBranchId}    and rm.mainbranchid ={nMainBranchId}";
                stringBuilder.Add(strQuery);

                stringBuilder.Add($"UPDATE debitnotedetails{nMainBranchId} pm SET taxid = tx.taxid FROM tax tx WHERE tx.taxpercent = pm.taxper AND pm.branchid = {nBranchId} and pm.mainbranchid = {nMainBranchId}");


                strQuery = $"update debitnotemain{nMainBranchId} erm set billserid = bs.billserid from billseries bs";
                strQuery += $"\n  where bs.branchid = {nBranchId} and bs.mainbranchid = {nMainBranchId} and billsersource  = 'DEBIT NOTE'";
                strQuery += $"\n  and erm.branchid = {nBranchId} and erm.mainbranchid = {nMainBranchId}; and  bs.branchid = {nBranchId} and bs.mainbranchid = {nMainBranchId}";
                stringBuilder.Add(strQuery);

                //expirydebitnotemain

                stringBuilder.Add($"UPDATE expirydebitnotemain{nMainBranchId} rm SET staffid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.staffid AND rm.branchid = {nBranchId} and rm.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"UPDATE expirydebitnotemain{nMainBranchId} rm SET acid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.acid AND rm.branchid = {nBranchId} and rm.mainbranchid = {nMainBranchId}");

                //expirydebitnotedetails
                stringBuilder.Add($"UPDATE expirydebitnotedetails{nMainBranchId} os SET productid = pm.productid FROM productmain{nMainBranchId} pm WHERE pm.tempid = os.productid AND os.branchid = {nBranchId}");
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
                stringBuilder.Add($"UPDATE returnadjustmentlog{nMainBranchId} rm SET acid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.acid and rm.acid>55 AND rm.branchid = {nBranchId} and rm.mainbranchid = {nMainBranchId}");

                //accountlogfile
                stringBuilder.Add($"UPDATE accountlogfile{nMainBranchId} rm SET acid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.acid and rm.acid>55 AND rm.branchid = {nBranchId} and rm.mainbranchid = {nMainBranchId}");

                //chequeentry
                stringBuilder.Add($"UPDATE chequeentry{nMainBranchId} rm SET staffid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.staffid AND rm.branchid = {nBranchId} and rm.mainbranchid= {nMainBranchId}");

                stringBuilder.Add($"UPDATE chequeentry{nMainBranchId} rm SET acid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.acid and rm.acid>55 AND rm.branchid = {nBranchId} And rm.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"UPDATE chequeentry{nMainBranchId} rm SET recid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.recid and rm.recid>55 AND rm.branchid = {nBranchId} And rm.mainbranchid = {nMainBranchId}");

                //outstanding
                stringBuilder.Add($"UPDATE outstanding{nMainBranchId} rm SET billserid = bs.billserid FROM billseries bs WHERE bs.tempid = rm.billserid AND rm.branchid = {nBranchId} and rm.mainbranchid = {nMainBranchId} and rm.billserid<>0 and  bs.branchid = {nBranchId} and bs.mainbranchid = {nMainBranchId} ");
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
                //strQuery = $"update issuesubdetails{nMainBranchId} im set billserid =  bs.billserid from billseries bs where bs.tempid = im.billserid";
                //strQuery += $"\n and bs.branchid = im.branchid and bs.mainbranchid = im.mainbranchid";
                //strQuery += $"\n and im.branchid ={nBranchId}    and im.mainbranchid ={nMainBranchId} and bs.billsersource='SALES'";
                //stringBuilder.Add(strQuery);
                stringBuilder.Add($"update issuereturndetails{nMainBranchId} set totqty = qty + freqty + advfre where branchid = {nBranchId} AND mainbranchid ={nMainBranchId}");
                stringBuilder.Add($"UPDATE issuereturndetails{nMainBranchId} isub SET productid = pm.productid FROM productmain{nMainBranchId} pm WHERE pm.tempid = isub.productid AND isub.branchid = {nBranchId} AND isub.mainbranchid = {nMainBranchId}");

                strQuery = $"update issuereturndetails{nMainBranchId} im set salesbillserid =  bs.billserid from billseries bs where bs.tempid = im.salesbillserid";
                strQuery += $"\n and bs.branchid = im.branchid and bs.mainbranchid = im.mainbranchid";
                strQuery += $"\n and im.branchid ={nBranchId}    and im.mainbranchid ={nMainBranchId} and bs.billsersource='CREDIT NOTE' and  bs.branchid = {nBranchId} and bs.mainbranchid = {nMainBranchId}";

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
                stringBuilder.Add($"UPDATE expiryreturndetails{nMainBranchId} isub SET productid = pm.productid FROM productmain{nMainBranchId} pm WHERE pm.tempid = isub.productid AND isub.branchid = {nBranchId} AND isub.mainbranchid = {nMainBranchId}");
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
                stringBuilder.Add($"UPDATE deliveryoutdetails{nMainBranchId} isub SET productid = pm.productid FROM productmain{nMainBranchId} pm WHERE pm.tempid = isub.productid AND isub.branchid = {nBranchId} AND isub.mainbranchid = {nMainBranchId}");
                stringBuilder.Add($"update deliveryoutdetails{nMainBranchId} set totqty = qty + freqty + advfre where branchid = {nBranchId} and mainbranchid = {nMainBranchId}");


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

                stringBuilder.Add($"UPDATE category ct SET categoryhead_id = ch.categoryheadid FROM categoryhead ch WHERE ct.categoryhead_id = ch.tempid AND ct.branchid = {nBranchId} and ct.mainbranchid = {nMainBranchId}");
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



             

                int totalQueries = stringBuilder.Count;
                int queryIndex = 1;
                foreach (string queryTemplate in stringBuilder)
                {
                    int pct = 85 + (int)((double)queryIndex / totalQueries * 15); // 85-100%
                    ReportProgress($"FK Update [{queryIndex}/{totalQueries}]", pct);
                    testquerytemplate = queryTemplate;
                    using var connection = PostgresConnection.Create();
                    connection.Open();
                    var query = string.Format(queryTemplate, nMainBranchId, nBranchId);
                    using var command = new NpgsqlCommand(query, connection);
                    command.ExecuteNonQuery();
                    connection.Close();
                    queryIndex++;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(testquerytemplate);
                MessageBox.Show("Error " + ex.Message.ToString() + " , " + testquerytemplate);
            }
        }

        public void UpdatePrimaryKeyColumns(Int64 nMainBranchId, Int64 nBranchId,Int64 nFromBranchId)
        {
            ReportProgress("Updating primary keys & foreign keys...", 85);
            ExecuteBulkUpdates(nMainBranchId, nBranchId, nFromBranchId);

          
            ReportProgress("Primary key updates completed", 100);
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



                using var posconnection1 = PostgresConnection.Create();
                posconnection1.Open();
                using var poscommand1 = new NpgsqlCommand(strUpdateQuery, posconnection1);
                poscommand1.ExecuteNonQuery();
                posconnection1.Close();

                ReportProgress("Updating mainsetting successfully", 2);
            }
            catch (Exception ex)
            {
                ReportProgress($"Updating mainsetting failed: {ex.Message}", 2);
            }
        }

        public void fnBranchSettingUpdate(long nMainBranchId, long nBranchId, long nFromBranchId)
        {
            ReportProgress("Updating Branchsetting in SQL Server...", 0);

            string strQuery = @"\n select * from branchsetting where branchid=" + nFromBranchId;

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
                    string KeyValue = row["KeyValue"].ToString();
                    string Value = row["Value"].ToString();
                    switch (KeyValue)
                    {
                        case "AccountHeadUpperCase":
                            strUpdateQuery += "\n Update branchsettings set settingvalue = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingkey='AccountHeadUpperCase' and branchid='" + nBranchId + "' ;";
                            break;
                        case "AgeRange1":
                            strUpdateQuery += "\n Update branchsettings set settingbillno = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingkey='AgeRange1' and branchid='" + nBranchId + "';";
                            break;
                        case "AgeRange2":
                            strUpdateQuery += "\n Update branchsettings set settingbillno = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingkey='AgeRange2' and branchid='" + nBranchId + "';";
                            break;
                        case "AgeRange3":
                            strUpdateQuery += "\n Update branchsettings set settingbillno = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingkey='AgeRange3' and branchid='" + nBranchId + "';";
                            break;
                        case "AgeRange4":
                            strUpdateQuery += "\n Update branchsettings set settingbillno = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingkey='AgeRange4' and branchid='" + nBranchId + "' ;";
                            break;
                        case "AgeRange5":
                            strUpdateQuery += "\n Update branchsettings set settingbillno = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingkey='AgeRange5' and branchid='" + nBranchId + "';";
                            break;
                        case "AgeRange6":
                            strUpdateQuery += "\n Update branchsettings set settingbillno = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingkey='AgeRange6' and branchid='" + nBranchId + "' ;";
                            break;
                        case "AmtPerPoint":
                            strUpdateQuery += "\n Update branchsettings set settingbillno = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingkey='AmtPerPoint' and branchid='" + nBranchId + "' ;";
                            break;
                        case "BarCodeBoxVisibleInSales":
                            strUpdateQuery += "\n Update branchsettings set settingvalue = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingkey='BarCodeBoxVisibleInSales' and branchid='" + nBranchId + "';";
                            break;
                        case "BillPrintSaveOrder":
                            strUpdateQuery += "\n Update branchsettings set settingvalue = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingkey='BillPrintSaveOrder' and branchid='" + nBranchId + "';";
                            break;
                        case "CancelledBillRemove":
                            strUpdateQuery += "\n Update branchsettings set settingvalue = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingkey='CancelledBillRemove' and branchid='" + nBranchId + "';";
                            break;
                        case "BillPost":
                            strUpdateQuery += "\n Update branchsettings set settingvalue = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingkey='BillPost' and branchid='" + nBranchId + "';";
                            break;
                        case "DcAccountsPost":
                            strUpdateQuery += "\n Update branchsettings set settingvalue = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingkey='DeliveryOutAccountsPost' and branchid='" + nBranchId + "';";
                            break;
                        case "ExpiryDebitNoteItemFromExpiryReceive":
                            strUpdateQuery += "\n Update branchsettings set settingbillno = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingkey='ExpiryDebitNoteItemFromExpiryReceive' and branchid='" + nBranchId + "';";
                            break;
                        case "GodownTransferNextNo":
                            strUpdateQuery += "\n Update branchsettings set settingbillno = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingkey='GodownTransferBillNo' and branchid='" + nBranchId + "';";
                            break;
                        case "GodownReturnNextNo":
                            strUpdateQuery += "\n Update branchsettings set settingbillno = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingkey='GodownTransferReturnNo' and branchid='" + nBranchId + "' ;";
                            break;
                        case "JobCardNo":
                            strUpdateQuery += "\n Update branchsettings set settingbillno = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingkey='JobCardNo' and branchid='" + nBranchId + "';";
                            break;
                        case "PointSystem":
                            strUpdateQuery += "\n Update branchsettings set settingvalue= '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingkey='PointSystem' and branchid='" + nBranchId + "';";
                            break;
                        case "PointValue":
                            strUpdateQuery += "\n Update branchsettings set settingbillno = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingkey='PointValue' and branchid='" + nBranchId + "';";
                            break;
                        case "SalesmanFixedInSales":
                            strUpdateQuery += "\n Update branchsettings set settingvalue = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingkey='SalesmanFixedInSales' and branchid='" + nBranchId + "';";
                            break;
                        case "ExpiryDebitNoteSameItemPrintOneLine":
                            strUpdateQuery += "\n Update branchsettings set settingvalue = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingkey='ExpiryDebitNoteSameItemPrintOneLine' and branchid='" + nBranchId + "';";
                            break;
                        case "TemporaryPurchaseNo":
                            strUpdateQuery += "\n Update branchsettings set settingbillno = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingkey='TemporaryPurchaseNo' and branchid='" + nBranchId + "';";
                            break;
                        case "InclusiveInSales":
                            strUpdateQuery += "\n Update branchsettings set settingbillno = '" + Value + "' where mainbranchid = '" + nMainBranchId + "' and settingkey='InclusiveInSales' and branchid='" + nBranchId + "';";
                            break;

                    }

                }

                using var posconnection1 = PostgresConnection.Create();
                posconnection1.Open();
                using var poscommand1 = new NpgsqlCommand(strUpdateQuery, posconnection1);
                poscommand1.ExecuteNonQuery();
                posconnection1.Close();

                ReportProgress("Updating BranchSetting  successfully", 2);
            }
            catch (Exception ex)
            {
                ReportProgress($"Updating BranchSetting  failed: {ex.Message}", 2);
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

                using var posconnection1 = PostgresConnection.Create();
                posconnection1.Open();
                using var poscommand1 = new NpgsqlCommand(strUpdateQuery, posconnection1);
                poscommand1.ExecuteNonQuery();
                posconnection1.Close();

                ReportProgress("Updating voucher prefix successfully", 2);
            }
            catch (Exception ex)
            {
                ReportProgress($"Updating voucher prefix failed: {ex.Message}", 2);
            }
        }

        public void fnBillSeriesInclusiveUpdate(long nMainBranchId, long nBranchId, long nFromBranchId)
        {
            ReportProgress("Updating BillSeries in SQL Server...", 0);

            string strQuery = @"select * from BillSeriesSalesInclusiveSet where branchid=" + nFromBranchId;

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
                    string BillSerId = row["BillSerId"].ToString();
                    string InclusiveSales = row["InclusiveSales"].ToString();
                    string TaxCondition = row["TaxCondition"].ToString();


                    strUpdateQuery += "\n UPDATE billseries SET billserbillinclusive = '" + InclusiveSales +
                                      "', billsertaxadd = '" + TaxCondition +
                                      "' WHERE tempid = '" + BillSerId +
                                      "' AND mainbranchid = '" + nMainBranchId +
                                      "' AND branchid = '" + nBranchId +
                                      "' AND billsersource = 'SALES';";
                }

                using var posconnection1 = PostgresConnection.Create();
                posconnection1.Open();

                using var poscommand1 = new NpgsqlCommand(strUpdateQuery, posconnection1);
                poscommand1.ExecuteNonQuery();
                posconnection1.Close();

                ReportProgress("Updating BillSeries successfully", 2);
            }
            catch (Exception ex)
            {
                ReportProgress($"Updating failed: {ex.Message}", 2);
            }
        }

        public void fnBranchUpdate(long nMainBranchId, long nBranchId, long nFromBranchId)
        {
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
                     //   "branchwhatsapptokenno = '" + Branch_WhatsAppTokenNo + "', " +
                       // "branchwhatsappurl = '" + Branch_WhatsAppUrl + "', " +
                        "branchsecurepwd = '" + Branch_SecurePwd + "', " +
                        "branchbarcodedesign = '" + Branch_BarCodeDesign + "', " +
                        "acid = " + (string.IsNullOrEmpty(AcId) ? 0 : AcId) +
                        " WHERE mainbranchid = " + nMainBranchId +
                        " AND branchid = " + nBranchId + ";";
                }

                using var posconnection1 = PostgresConnection.Create();
                posconnection1.Open();
                using var poscommand1 = new NpgsqlCommand(strUpdateQuery, posconnection1);
                poscommand1.ExecuteNonQuery();
                posconnection1.Close();
                ReportProgress("Updating Branch successfully", 2);
            }
            catch (Exception ex)
            {
                ReportProgress($"Updating failed: {ex.Message}", 2);
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

                    strUpdateQuery += $"\n update controlorder set controlorder ='{ControlOrder}' , active ={Active} where controlname = '{ControlName}' and controltype ='{ControlType}' ;";

                }


                foreach (DataRow row in dtsql.Rows)
                {
                    string ControlName = row["ControlName"].ToString();
                    string ControlType = row["ControlType"].ToString();
                    Int32 ControlOrder = Convert.ToInt32(row["ControlOrder"].ToString());
                    bool Active = Convert.ToString(row["Active"].ToString()) == "0" ? false : true;
                    strUpdateQuery += $"\n update controlordermainbranch set controlorder ='{ControlOrder}' , active ={Active} where controlname = '{ControlName}' and controltype ='{ControlType}' ;";

                }



                using var posconnection1 = PostgresConnection.Create();
                posconnection1.Open();
                using var poscommand1 = new NpgsqlCommand(strUpdateQuery, posconnection1);
                poscommand1.ExecuteNonQuery();
                posconnection1.Close();

                ReportProgress("Updating controlorder successfully", 2);
            }
            catch (Exception ex)
            {
                ReportProgress($"Updating controlorder failed: {ex.Message}", 2);
            }
        }


        public void fnBillNosUpdate(long nFromBranchId,long nMainBranchId,long nBranchId)
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
                long nBillNo = 1;
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

                    nBillNo = 1;

                    if (dsDataSet.Tables.Count > 1)
                    {
                        DataRow[] datarows = dsDataSet.Tables[1].Select("SettingName = 'OpeningStockBillNo' and branchid = "+nFromBranchId);
                        foreach(DataRow rows in datarows)
                        {
                            nBillNo = Convert.ToInt64(rows["Value"].ToString());
                        }
                    }

                    strUpdateQuery += $"\n UPDATE billseries SET billsercurrentbillno = '{nBillNo}'";
                    strUpdateQuery += $"\n WHERE mainbranchid = '{nMainBranchId}' and branchid = {nBranchId} and billsersource = 'OPENING STOCK ENTRY';";

                }

                using var posconnection1 = PostgresConnection.Create();
                posconnection1.Open();
                using var poscommand1 = new NpgsqlCommand(strUpdateQuery, posconnection1);
                poscommand1.ExecuteNonQuery();
                posconnection1.Close();

                ReportProgress("Updating BranchSetting  successfully", 2);
            }
            catch (Exception ex)
            {
                ReportProgress($"Updating BranchSetting  failed: {ex.Message}", 2);
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
                string strPrintFileName = "",strPrintPreviewName="";


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

                }

                strUpdateQuery += $"\n update printdisplaysettings set printname = 'CreditNotePrintModelOne' where printname = 'PrintModelCreditNote';";

                strUpdateQuery += $"\n UPDATE billseries SET printfilename = 'CreditNotePrintModelOne',printfilepreview='CreditNotePrintModelOne'";
                strUpdateQuery += $"\n WHERE printfilename = 'PrintModelCreditNote' and mainbranchid = '{nMainBranchId}' and branchid = {nBranchId} and billsersource = 'CREDIT NOTE';";

                using var posconnection1 = PostgresConnection.Create();
                posconnection1.Open();
                using var poscommand1 = new NpgsqlCommand(strUpdateQuery, posconnection1);
                poscommand1.ExecuteNonQuery();
                posconnection1.Close();

                ReportProgress("Updating BranchSetting  successfully", 2);
            }
            catch (Exception ex)
            {
                ReportProgress($"Updating BranchSetting  failed: {ex.Message}", 2);
            }
        }

    }

}
