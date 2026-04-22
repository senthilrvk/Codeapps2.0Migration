using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Primitives;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using CodeAppsDataMigration.Data;
using CodeAppsDataMigration.Migration;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
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



        private void ExecuteBulkUpdates(Int64 nMainBranchId, Int64 nBranchId)
        {
            string testquerytemplate = "";
            try
            {

                List<string> stringBuilder = new List<string>();
                stringBuilder.Add($"UPDATE accounthead{nMainBranchId} ah SET areaid = a.area_id FROM area a WHERE a.tempid = ah.areaid AND ah.branchid = {nBranchId}");
                stringBuilder.Add($"UPDATE productmain{nMainBranchId} pm SET categoryid = ca.categoryid FROM category ca WHERE ca.tempid = pm.categoryid AND pm.branchid = {nBranchId}");
                stringBuilder.Add($"UPDATE productmain{nMainBranchId} pm SET taxid = tx.taxid FROM tax tx WHERE tx.taxpercent = pm.prodlinkeshopid AND pm.branchid = {nBranchId}");
                stringBuilder.Add($"UPDATE productmain{nMainBranchId} pm SET manufacture_id = mf.manufacture_id FROM manufacture{nMainBranchId} mf WHERE mf.tempid = pm.manufacture_id AND pm.branchid = {nBranchId}");
                stringBuilder.Add($"UPDATE productmain{nMainBranchId} pm SET hsnid = hs.hsn_id FROM hsn{nMainBranchId} hs WHERE hs.tempid = pm.hsnid AND pm.branchid = {nBranchId}");
                stringBuilder.Add($"UPDATE hsn{nMainBranchId} hs SET taxid = pm.taxid FROM productmain{nMainBranchId} pm WHERE hs.hsn_id = pm.hsnid AND pm.branchid = {nBranchId}");

                // Sales
                stringBuilder.Add($"UPDATE issuesubdetails{nMainBranchId} isub SET productid = pm.productid FROM productmain{nMainBranchId} pm WHERE pm.tempid = isub.productid AND isub.branchid = {nBranchId}");
                stringBuilder.Add($"UPDATE issuemain{nMainBranchId} im SET acid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = im.acid AND im.branchid = {nBranchId}");
                stringBuilder.Add($"UPDATE issuemain{nMainBranchId} im SET salesexeid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = im.salesexeid AND im.branchid = {nBranchId}");
                stringBuilder.Add($"UPDATE issuemain{nMainBranchId} im SET staffid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = im.staffid AND im.branchid = {nBranchId}");
                var strQuery = $"update issuemain{nMainBranchId} im set billserid =  bs.billserid from billseries bs where bs.tempid = im.billserid";
                strQuery += $"\n and bs.branchid = im.branchid and bs.mainbranchid = im.mainbranchid";
                strQuery += $"\n and im.branchid ={nBranchId}    and im.mainbranchid ={nMainBranchId} and bs.billsersource='SALES'";
                stringBuilder.Add(strQuery);

                stringBuilder.Add($"UPDATE issuesubdetails{nMainBranchId} pm SET taxid = tx.taxid FROM tax tx WHERE tx.taxpercent = pm.taxpers AND pm.branchid = {nBranchId}");

                strQuery = $"update issuesubdetails{nMainBranchId} im set billserid =  bs.billserid from billseries bs where bs.tempid = im.billserid";
                strQuery += $"\n and bs.branchid = im.branchid and bs.mainbranchid = im.mainbranchid";
                strQuery += $"\n and im.branchid ={nBranchId}    and im.mainbranchid ={nMainBranchId} and bs.billsersource='SALES'";
                stringBuilder.Add(strQuery);

                // Purchase
                stringBuilder.Add($"UPDATE receiptdetails{nMainBranchId} isub SET productid = pm.productid FROM productmain{nMainBranchId} pm WHERE pm.tempid = isub.productid AND isub.branchid = {nBranchId}");
                stringBuilder.Add($"UPDATE receiptmain{nMainBranchId} im SET acid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = im.acid AND im.branchid = {nBranchId}");
                stringBuilder.Add($"UPDATE receiptmain{nMainBranchId} im SET staffid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = im.staffid AND im.branchid = {nBranchId}");
                strQuery = $"update receiptmain{nMainBranchId} im set billserid =  bs.billserid from billseries bs where bs.tempid = im.billserid";
                strQuery += $"\n and bs.branchid = im.branchid and bs.mainbranchid = im.mainbranchid";
                strQuery += $"\n and im.branchid ={nBranchId}    and im.mainbranchid ={nMainBranchId} and bs.billsersource='PURCHASE'";
                stringBuilder.Add(strQuery);

                strQuery = $"update receiptdetails{nMainBranchId} im set billserid =  bs.billserid from billseries bs where bs.tempid = im.billserid";
                strQuery += $"\n and bs.branchid = im.branchid and bs.mainbranchid = im.mainbranchid";
                strQuery += $"\n and im.branchid ={nBranchId}    and im.mainbranchid ={nMainBranchId} and bs.billsersource='PURCHASE'";
                stringBuilder.Add(strQuery);

                stringBuilder.Add($"UPDATE receiptdetails{nMainBranchId} pm SET taxid = tx.taxid FROM tax tx WHERE tx.taxpercent = pm.taxpers AND pm.branchid = {nBranchId}");

                strQuery = $"update receiptdetails{nMainBranchId} rsub set receiptid =  rm.receiptid from receiptmain{nMainBranchId} rm where rm.paytermsid = rsub.priceid";
                strQuery += $"\n and rsub.branchid = rm.branchid and rsub.mainbranchid = rm.mainbranchid and rsub.receiptno=rm.receiptno";
                strQuery += $"\n and rm.branchid ={nBranchId}    and rm.mainbranchid ={nMainBranchId}";
                stringBuilder.Add(strQuery);

                //store
                stringBuilder.Add($"UPDATE store{nMainBranchId} isub SET productid = pm.productid FROM productmain{nMainBranchId} pm WHERE pm.tempid = isub.productid AND isub.branchid = {nBranchId}");
                stringBuilder.Add($"UPDATE store{nMainBranchId} im SET acid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = im.acid AND im.branchid = {nBranchId}");

                strQuery = $"update store{nMainBranchId} st set receiptid =  rsub.receiptid from receiptdetails{nMainBranchId} rsub where rsub.priceid = st.receiptid";
                strQuery += $"\n and st.branchid = rsub.branchid and st.mainbranchid = rsub.mainbranchid and st.receiptno=rsub.receiptno and st.batchslno=rsub.batchslno";
                strQuery += $"\n and st.productid = rsub.productid and rsub.branchid ={nBranchId}    and rsub.mainbranchid ={nMainBranchId}";
                stringBuilder.Add(strQuery);

                stringBuilder.Add($"update receiptmain{nMainBranchId}    set paytermsid = 0  where branchid = {nBranchId};");
                stringBuilder.Add($"update receiptdetails{nMainBranchId} set priceid    = 0  where branchid = {nBranchId}");

                

                ///opening stock main
                stringBuilder.Add($"UPDATE openingstockmain{nMainBranchId} os SET acid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = os.acid AND os.branchid = {nBranchId}");


                ///opening stock details
                stringBuilder.Add($"UPDATE openingstockdetails{nMainBranchId} os SET productid = pm.productid FROM productmain{nMainBranchId} pm WHERE pm.tempid = os.productid AND os.branchid = {nBranchId}");
                stringBuilder.Add($"UPDATE openingstockdetails{nMainBranchId} pm SET taxid = tx.taxid FROM tax tx WHERE tx.taxpercent = pm.taxpers AND pm.branchid = {nBranchId}");
                strQuery = $"update openingstockdetails{nMainBranchId} rsub set openingstockid =  rm.openingstockid from openingstockmain{nMainBranchId} rm where rm.paytermsid = rsub.dcinno";
                strQuery += $"\n and rsub.branchid = rm.branchid and rsub.mainbranchid = rm.mainbranchid and rsub.openingstockno=rm.openingstockno";
                strQuery += $"\n and rm.branchid ={nBranchId}    and rm.mainbranchid ={nMainBranchId}";
                stringBuilder.Add(strQuery);

                stringBuilder.Add($"update openingstockmain{nMainBranchId}    set paytermsid = 0  where branchid = {nBranchId};");
                stringBuilder.Add($"update openingstockdetails{nMainBranchId}    set dcinno = 0  where branchid = {nBranchId};");
                
                
                /// receipt return main
                stringBuilder.Add($"UPDATE receiptreturnmain{nMainBranchId} rm SET staffid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.staffid AND rm.branchid = {nBranchId}");
                stringBuilder.Add($"UPDATE receiptreturnmain{nMainBranchId} rm SET reasonid = ah.categoryid FROM category ah WHERE ah.tempid = rm.reasonid AND rm.branchid = {nBranchId}");

                stringBuilder.Add($"UPDATE receiptreturndetails{nMainBranchId} rm SET staffid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.staffid AND rm.branchid = {nBranchId}");
                stringBuilder.Add($"UPDATE receiptreturndetails{nMainBranchId} os SET productid = pm.productid FROM productmain{nMainBranchId} pm WHERE pm.tempid = os.productid AND os.branchid = {nBranchId}");
                stringBuilder.Add($"UPDATE receiptreturndetails{nMainBranchId} pm SET taxid = tx.taxid FROM tax tx WHERE tx.taxpercent = pm.receiptsubtaxpers AND pm.branchid = {nBranchId}");
                stringBuilder.Add($"UPDATE receiptreturndetails{nMainBranchId} rm SET reasonid = ah.categoryid FROM category ah WHERE ah.tempid = rm.reasonid AND rm.branchid = {nBranchId}");

                strQuery = $"update receiptreturndetails{nMainBranchId} rsub set receiptreturnmainid =  rm.receiptreturnmainid from receiptreturnmain{nMainBranchId} rm where rm.billserid = rsub.receiptreturnmainid";
                strQuery += $"\n and rsub.branchid = rm.branchid and rsub.mainbranchid = rm.mainbranchid and rsub.receiptreturnno=rm.receiptreturnno";
                strQuery += $"\n and rm.branchid ={nBranchId}    and rm.mainbranchid ={nMainBranchId}";
                stringBuilder.Add(strQuery);


                stringBuilder.Add($"update receiptreturnmain{nMainBranchId}    set billserid = 0  where branchid = {nBranchId};");

                //debitnotemain
                stringBuilder.Add($"UPDATE debitnotemain{nMainBranchId} rm SET staffid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.staffid AND rm.branchid = {nBranchId}");
                stringBuilder.Add($"UPDATE debitnotemain{nMainBranchId} rm SET acid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.acid AND rm.branchid = {nBranchId}");

                //debitnotedetails
                stringBuilder.Add($"UPDATE debitnotedetails{nMainBranchId} os SET productid = pm.productid FROM productmain{nMainBranchId} pm WHERE pm.tempid = os.productid AND os.branchid = {nBranchId}");
                strQuery = $"update debitnotedetails{nMainBranchId} rsub set debitnoteid =  rm.debitnoteid from debitnotemain{nMainBranchId} rm where rm.billserid = rsub.debitnoteid";
                strQuery += $"\n and rsub.branchid = rm.branchid and rsub.mainbranchid = rm.mainbranchid and rsub.debitnoteno=rm.debitnoteno";
                strQuery += $"\n and rm.branchid ={nBranchId}    and rm.mainbranchid ={nMainBranchId}";
                stringBuilder.Add(strQuery);

                stringBuilder.Add($"UPDATE debitnotedetails{nMainBranchId} pm SET taxid = tx.taxid FROM tax tx WHERE tx.taxpercent = pm.taxper AND pm.branchid = {nBranchId}");

                //expirydebitnotemain

                stringBuilder.Add($"UPDATE expirydebitnotemain{nMainBranchId} rm SET staffid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.staffid AND rm.branchid = {nBranchId}");
                stringBuilder.Add($"UPDATE expirydebitnotemain{nMainBranchId} rm SET acid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.acid AND rm.branchid = {nBranchId}");

                //expirydebitnotedetails
                stringBuilder.Add($"UPDATE expirydebitnotedetails{nMainBranchId} os SET productid = pm.productid FROM productmain{nMainBranchId} pm WHERE pm.tempid = os.productid AND os.branchid = {nBranchId}");
                strQuery = $"update expirydebitnotedetails{nMainBranchId} rsub set expirydebitnoteid =  rm.expirydebitnoteid from expirydebitnotemain{nMainBranchId} rm where rm.billserid = rsub.expirydebitnoteid";
                strQuery += $"\n and rsub.branchid = rm.branchid and rsub.mainbranchid = rm.mainbranchid and rsub.expirydebitnotemainno=rm.expirydebitnoteno";
                strQuery += $"\n and rm.branchid ={nBranchId}    and rm.mainbranchid ={nMainBranchId}";
                stringBuilder.Add(strQuery);

                stringBuilder.Add($"UPDATE expirydebitnotedetails{nMainBranchId} pm SET taxid = tx.taxid FROM tax tx WHERE tx.taxpercent = pm.taxper AND pm.branchid = {nBranchId}");


                /// voucherdetails
                stringBuilder.Add($"UPDATE voucherdetails{nMainBranchId} rm SET staffid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.staffid AND rm.branchid = {nBranchId}");
                stringBuilder.Add($"UPDATE voucherdetails{nMainBranchId} rm SET repid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.repid AND rm.branchid = {nBranchId}");

                stringBuilder.Add($"UPDATE voucherdetails{nMainBranchId} rm SET acid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.acid and rm.acid>55 AND rm.branchid = {nBranchId}");
                stringBuilder.Add($"UPDATE voucherdetails{nMainBranchId} rm SET revacid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.revacid and rm.revacid>55 AND rm.branchid = {nBranchId}");

                stringBuilder.Add($"UPDATE voucherdetails{nMainBranchId} rm SET acid = -46 FROM accounthead{nMainBranchId} ah WHERE rm.acid=26 AND rm.branchid = {nBranchId}");
                stringBuilder.Add($"UPDATE voucherdetails{nMainBranchId} rm SET revacid = -46 FROM accounthead{nMainBranchId} ah WHERE rm.revacid =26  AND rm.branchid = {nBranchId}");



                strQuery  = $"INSERT INTO vouchermain{nMainBranchId}(";
                strQuery += $"\n vouchermaindate, vprefixid, voucherno, uniquevoucherid, vouchergroupid, voucherprefix,";
                strQuery += $"\n chequeno, chequedate, voucheramt, bankname, acid, repid, staffid, vouchertime, remarks, enterdate, tdspers,";
                strQuery += $"\n tdsamt, transtype, bvouchercancel, branchid, mainbranchid, revacid, refno, headtype, balanceamt";
                strQuery += $"\n )";
                strQuery += $"\n select distinct     vd.voucherdate, vd.vprefixid, vd.voucherno, vd.uniquevoucherid,0 vouchergroupid, vd.voucherprefix,";
                strQuery += $"\n vd.chequeno, vd.chequedate, 0 voucheramt, vd.bankname, 0 acid, vd.repid, vd.staffid, vd.vouchertime, vd.remarks,";
                strQuery += $"\n vd.enterdate, vd.tdspers, ";
                strQuery += $"\n vd.tdsamt,'' transtype,False bvouchercancel, vd.branchid, vd.mainbranchid, 0 revacid, vd.refno,'' headtype, vd.balanceamt";
                strQuery += $"\n from voucherdetails{nMainBranchId} vd where vd.vprefixid in (1, 2, 3, 4);";
                stringBuilder.Add(strQuery);

                strQuery  = $" update vouchermain{nMainBranchId} vm set voucheramt = vd.voucheramt,acid = vd.acid,revacid = vd.revacid from voucherdetails{nMainBranchId} vd WHERE";
                strQuery += $"\n vm.vprefixid = vd.vprefixid and vm.voucherno = vd.voucherno";
                strQuery += $"\n and vm.uniquevoucherid = vd.uniquevoucherid";
                strQuery += $"\n and vd.voucheramt > 0 and vd.vprefixid in (1, 3);";
                stringBuilder.Add(strQuery);

                strQuery = $" update vouchermain{nMainBranchId} vm set voucheramt = vd.voucheramt,acid = vd.acid,revacid = vd.revacid from voucherdetails{nMainBranchId} vd WHERE";
                strQuery += $"\n vm.vprefixid = vd.vprefixid and vm.voucherno = vd.voucherno";
                strQuery += $"\n and vm.uniquevoucherid = vd.uniquevoucherid";
                strQuery += $"\n and vd.voucheramt < 0 and vd.vprefixid in (2, 4);";
                stringBuilder.Add(strQuery);

                //returnadjustmentlog
                stringBuilder.Add($"UPDATE returnadjustmentlog{nMainBranchId} rm SET acid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.acid and rm.acid>55 AND rm.branchid = {nBranchId}");

                //accountlogfile
                stringBuilder.Add($"UPDATE accountlogfile{nMainBranchId} rm SET acid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.acid and rm.acid>55 AND rm.branchid = {nBranchId}");

                //chequeentry
                stringBuilder.Add($"UPDATE chequeentry{nMainBranchId} rm SET staffid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.staffid AND rm.branchid = {nBranchId}");
               
                stringBuilder.Add($"UPDATE chequeentry{nMainBranchId} rm SET acid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.acid and rm.acid>55 AND rm.branchid = {nBranchId}");
                stringBuilder.Add($"UPDATE chequeentry{nMainBranchId} rm SET recid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.recid and rm.recid>55 AND rm.branchid = {nBranchId}");

                //outstanding
                stringBuilder.Add($"UPDATE outstanding{nMainBranchId} rm SET acid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.acid and rm.acid>55 AND rm.branchid = {nBranchId}");
                stringBuilder.Add($"UPDATE outstanding{nMainBranchId} rm SET salesmanid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = rm.salesmanid AND rm.branchid = {nBranchId}");
                stringBuilder.Add($"update outstanding{nMainBranchId}    set sourcetype = 'Sales'  where vprefixid=5 and branchid = {nBranchId};");
                stringBuilder.Add($"update outstanding{nMainBranchId}    set sourcetype = 'Purchase'  where vprefixid=6 and branchid = {nBranchId};");

                //issuereturnmain
             
                stringBuilder.Add($"UPDATE issuereturnmain{nMainBranchId} im SET acid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = im.acid AND im.branchid = {nBranchId}");
                stringBuilder.Add($"UPDATE issuereturnmain{nMainBranchId} im SET salesexeid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = im.salesexeid AND im.branchid = {nBranchId}");
                stringBuilder.Add($"UPDATE issuereturnmain{nMainBranchId} im SET staffid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = im.staffid AND im.branchid = {nBranchId}");
                //strQuery = $"update issuereturnmain{nMainBranchId} im set billserid =  bs.billserid from billseries bs where bs.tempid = im.billserid";
                //strQuery += $"\n and bs.branchid = im.branchid and bs.mainbranchid = im.mainbranchid";
                //strQuery += $"\n and im.branchid ={nBranchId}    and im.mainbranchid ={nMainBranchId} and bs.billsersource='SALES'";
                //stringBuilder.Add(strQuery);


                //issuereturndetails
                stringBuilder.Add($"UPDATE issuereturndetails{nMainBranchId} pm SET taxid = tx.taxid FROM tax tx WHERE tx.taxpercent = pm.taxpers AND pm.branchid = {nBranchId}");
                //strQuery = $"update issuesubdetails{nMainBranchId} im set billserid =  bs.billserid from billseries bs where bs.tempid = im.billserid";
                //strQuery += $"\n and bs.branchid = im.branchid and bs.mainbranchid = im.mainbranchid";
                //strQuery += $"\n and im.branchid ={nBranchId}    and im.mainbranchid ={nMainBranchId} and bs.billsersource='SALES'";
                //stringBuilder.Add(strQuery);
                stringBuilder.Add($"UPDATE issuereturndetails{nMainBranchId} isub SET productid = pm.productid FROM productmain{nMainBranchId} pm WHERE pm.tempid = isub.productid AND isub.branchid = {nBranchId}");


                //expiryreturnmain

                stringBuilder.Add($"UPDATE expiryreturnmain{nMainBranchId} im SET acid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = im.acid AND im.branchid = {nBranchId}");
                stringBuilder.Add($"UPDATE expiryreturnmain{nMainBranchId} im SET salesexeid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = im.salesexeid AND im.branchid = {nBranchId}");
                stringBuilder.Add($"UPDATE expiryreturnmain{nMainBranchId} im SET staffid = ah.acid FROM accounthead{nMainBranchId} ah WHERE ah.tempid = im.staffid AND im.branchid = {nBranchId}");
                //strQuery = $"update issuereturnmain{nMainBranchId} im set billserid =  bs.billserid from billseries bs where bs.tempid = im.billserid";
                //strQuery += $"\n and bs.branchid = im.branchid and bs.mainbranchid = im.mainbranchid";
                //strQuery += $"\n and im.branchid ={nBranchId}    and im.mainbranchid ={nMainBranchId} and bs.billsersource='SALES'";
                //stringBuilder.Add(strQuery);


                //issuereturndetails
                stringBuilder.Add($"UPDATE expiryreturndetails{nMainBranchId} pm SET taxid = tx.taxid FROM tax tx WHERE tx.taxpercent = pm.taxpers AND pm.branchid = {nBranchId}");
                //strQuery = $"update issuesubdetails{nMainBranchId} im set billserid =  bs.billserid from billseries bs where bs.tempid = im.billserid";
                //strQuery += $"\n and bs.branchid = im.branchid and bs.mainbranchid = im.mainbranchid";
                //strQuery += $"\n and im.branchid ={nBranchId}    and im.mainbranchid ={nMainBranchId} and bs.billsersource='SALES'";
                //stringBuilder.Add(strQuery);
                stringBuilder.Add($"UPDATE expiryreturndetails{nMainBranchId} isub SET productid = pm.productid FROM productmain{nMainBranchId} pm WHERE pm.tempid = isub.productid AND isub.branchid = {nBranchId}");


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

        public void UpdatePrimaryKeyColumns(Int64 nMainBranchId, Int64 nBranchId)
        {
            ReportProgress("Updating primary keys & foreign keys...", 85);
            ExecuteBulkUpdates(nMainBranchId, nBranchId);
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

    }

}
