using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.VariantTypes;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;

namespace CodeAppsDataMigration.Migration
{
    public static class MigrationConfig
    {
        public static Int64 nMainBranchId = 0;
        public static Int64 nBranchId = 0;
        public static Int64 nFromBranchId = 0;
        public static List<TableMap> Tables => new()
        {
            new TableMap
            {
                SqlTable = "AccountHead",
                PgTable  = "accounthead"+nMainBranchId.ToString(),
                Columns = new[]
                {
                    // ---------- Identity ----------
                    ("AC_Id", "tempid", "bigint"),

                    // ---------- Amounts ----------
                    ("AdjAmount", "adjamount", "numeric"),
                    ("OBalance", "obalance", "numeric"),
                    ("CrLmtDays", "crlmtdays", "integer"),
                    ("CrLmtAmt", "crlmtamt", "numeric"),
                    ("MaxBillAmount", "maxbillamount", "numeric"),

                    // ---------- Account ----------
                    ("AC_Name", "acname", "text"),
                    ("Alias", "aliasname", "text"),
                    ("Type", "typename", "text"),
                    ("CustOrSupp", "custorsupp", "text"),
                    ("PurType", "purtype", "text"),

                    // ---------- Address ----------
                    ("Addr1", "addr1", "text"),
                    ("Addr2", "addr2", "text"),
                    ("Addr3", "addr3", "text"),
                    ("AcLocation", "aclocation", "text"),

                    // ---------- Custom mappings ----------
                    ("DescEditFlag", "statename", "text"),
                    ("Schedule1", "statecode", "integer"),
                    ("Schedule2", "salesmanid", "bigint"),
                    ("Schedule3", "taxid", "integer"),
                    ("Schedule4", "underid", "bigint"),

                    ("Field1", "pincode", "text"),
                    ("Field2", "displayname", "text"),
                    ("Field3", "displayaddress", "text"),

                    // ---------- Contact ----------
                    ("Phone", "phone", "text"),
                    ("Mobile", "mobile", "text"),
                    ("WhatsAppNo", "whatsappno", "text"),
                    ("Email", "email", "text"),
                    ("Web", "web", "text"),
                    ("Transporter", "transporter", "text"),
                    ("Fax", "accountno", "text"),

                    // ---------- Licenses ----------
                    ("DLNo1", "dlno1", "text"),
                    ("DLNo2", "dlno2", "text"),
                    ("Tin1", "gstno", "text"),
                    ("Tin2", "aadharno", "text"),
                    ("CstNo1", "panno", "text"),

                    // ---------- Dates ----------
                    ("CreateDate", "createdate", "date"),
                    ("StartDate", "startdate", "date"),
                    ("ExpiryDate", "expirydate", "date"),
                    ("DateOfBirth", "dateofbirth", "date"),

                    // ---------- Schedule ----------
                    ("ScheduleType", "scheduletype", "integer"),
                    ("EntryMatch", "entrymatch", "text"),

                    // ---------- Banking ----------
                  //  ("BankYesNo", "bankflag", "boolean"),

                    // ---------- User ----------
                    ("UserID", "userid", "bigint"),
                    ("UserName", "username", "text"),
                    ("Pwd", "pwd", "text"),
                    ("UniqueDeviceId", "uniquedeviceid", "text"),
                    ("AccessLevel", "accesslevel", "integer"),

                    // ---------- Category ----------
                    ("CategoryId", "categoryid", "bigint"),
                    ("AreaId", "areaid", "integer"),
                    ("MediaId", "mediaid", "bigint"),
                    ("CurrencyId", "currencyid", "bigint"),

                    // ---------- Currency ----------
                    ("Currency", "currency", "text"),
                    ("IFSCode", "ifscode", "text"),

                    // ---------- Model ----------
                    ("ModelPoint", "modelpoint", "numeric"),
                    ("ModelPointAmt", "modelpointamt", "numeric"),

                    // ---------- Pricing ----------
                    ("PriceMenuId", "pricemenuid", "numeric"),
                    ("AgentPriceMenuId", "agentpricemenuid", "integer"),
                    ("AgentMarginPers", "agentmarginpers", "numeric"),

                    // ---------- Flags ----------
                    ("CustomerFlag", "customerflag", "boolean"),
                    ("SupplierFlag", "supplierflag", "boolean"),
                    ("StaffFlag", "staffflag", "boolean"),
                    ("LoginFlag", "loginflag", "boolean"),
                    ("AgentFlag", "agentflag", "boolean"),
                    ("OtherFlag", "otherflag", "boolean"),
                    ("ActiveFlag", "activeflag", "boolean"),
                    ("SalesmanFlag", "salesmanflag", "boolean"),

                    // ---------- Special ----------
                    ("Flag", "boutstanding", "boolean"),
                    ("bSelect", "bmodelpoint", "boolean"),

                    // ---------- Discount ----------
                    ("ServiceDisPers", "servicedispers", "numeric"),
                    ("AcDisPers", "dispers", "numeric"),

                    // ---------- Geo ----------
                    ("Latitude", "latitude", "numeric"),
                    ("Longitude", "longitude", "numeric"),

                    // ---------- UPI ----------
                    ("UPIFlag", "upiflag", "boolean"),
                    ("AcUPIOrderNo", "upiorderno", "integer"),

                    // ---------- Expense ----------
                    ("AccountHead_MonthExpenseFlag", "monthexpenseflag", "boolean"),
                    ("AccountHead_ExpenseAmt", "expenseamt", "numeric"),

                    // ---------- Supply ----------
                    ("AcField5", "supplytypecode", "text"),

                    // ---------- Others ----------
                    ("AcNoField2", "nextremainder", "integer"),
                     ("Type", "distance", "numeric"),

                     //---------- remaining field --------

                     
                  
                     ("BankYesNo", "supplytype", "text"),

                    // ---------- Branch ----------
                    ("branchid", "branchid", "bigint"),
                    ("mainbranchid", "mainbranchid", "bigint")
                },
               condition="where ac_id>55 and  branchid ="+nFromBranchId.ToString(),
               Constants = new Dictionary<string, object>{
                   {"bankflag",false},
                   {"district", ""},
                   {"statetype",""},
                   {"license1", ""},
                   {"license2", ""},
                   {"license3", ""},
                   {"license4", ""},
                   {"schedule1", "0"},
                   {"schedule2", "0"},
                   {"schedule4", "0"},
                   {"introducedby", ""},
                   {"acmapheadid", "0"}
               }
            },
            new TableMap
            {
                SqlTable = "Area",
                PgTable  = "area",

                Columns = new[]
                {
                    ("Area_Id", "tempid", "bigint"),
                    ("Area_Name", "area_name", "text"),
                    ("Area_ShortName", "area_shortname", "text"),
                    ("Area_Description", "area_description", "text"),
                    ("CategoryId", "categoryid", "numeric"),
                    ("branchid", "branchid", "bigint"),
                    ("mainbranchid", "mainbranchid", "bigint")
                },
                 condition="where   branchid ="+nFromBranchId.ToString()
            },
            new TableMap
            {
                SqlTable = "Chemical",
                PgTable  = "chemical"+nMainBranchId.ToString(),

                Columns = new[]
                {
                    ("ChemicalId", "tempid", "bigint"),
                    ("Chemical1", "chemical1", "text"),
                    ("Chemical2", "chemical2", "text"),
                    ("Chemical3", "chemical3", "text"),
                    ("Chemical_MapId", "chemical_mapid", "bigint"),
                    ("branchid", "branchid", "bigint"),
                    ("mainbranchid", "mainbranchid", "bigint")
                },
                 condition="where   branchid ="+nFromBranchId.ToString()
            },

           new TableMap
           {
                SqlTable = "Category",
                PgTable  = "category",

                Columns = new[]
                {
                    ("CategoryID", "tempid", "bigint"),
                    ("CategoryDesc", "categoryname", "text"),
                    ("CategoryType", "categorytypeid", "integer"),
                    ("Active", "active", "boolean"),
                    ("CategoryHead_Id", "categoryhead_id", "integer"),
                    ("Category_QtyDecPlace", "category_qtydecplace", "integer"),
                    ("Category_OrderNo", "category_orderno", "integer"),
                    ("", "category_imageloc", "integer"),
                    // constant value
                    ("branchid", "branchid", "bigint"),
                    ("mainbranchid", "mainbranchid", "bigint")
                },
               condition ="where   branchid ="+nFromBranchId.ToString(),
               Constants = new Dictionary<string, object>{
                   {"printerid",0}
               }
           },
           new TableMap
            {
                SqlTable = "CategoryHead",
                PgTable  = "categoryhead",

                Columns = new[]
                {
                    ("CategoryHead_Id", "tempid", "bigint"),
                    ("CategoryHead_Name", "headname", "text"),
                    ("CategoryHead_Description", "headdesc", "text"),
                    ("CategoryHead_Field1", "headfield1", "text"),
                    ("CategoryHeadType", "headtypeid", "integer"),
                    ("ProductType_Id", "producttypeid", "integer"),
                    ("CategoryHead_ImageLoc", "imageloc", "text"),
                    ("CategoryHead_OrderNo", "orderno", "integer"),
                    ("CategoryHead_DisplayName", "displayname", "text"),
                    ("CategoryHead_Remarks", "remarks", "text"),
                    ("branchid", "branchid", "bigint"),
                    ("mainbranchid", "mainbranchid", "integer")
                },
                 condition="where   branchid ="+nFromBranchId.ToString()
            },

            new TableMap
            {
                SqlTable = "Manufacture",
                PgTable  = "manufacture"+nMainBranchId.ToString(),

                Columns = new[]
                {
                    ("Manufacture_Name","manufacture_name","text"),
                    ("Manufacture_MFRNo","manufacture_mfrno","text"),
                    ("Manufacture_Addr1","manufacture_addr1","text"),
                    ("Manufacture_Addr2","manufacture_addr2","text"),
                    ("Manufacture_Addr3","manufacture_addr3","text"),
                    ("Manufacture_Phone","manufacture_phone","text"),
                    ("Manufacture_Fax","manufacture_fax","text"),
                    ("Manufacture_Email","manufacture_email","text"),
                    ("Manufacture_Webaddress","manufacture_webaddress","text"),
                    ("Manufacture_Transporter","manufacture_transporter","text"),
                    ("Manufacture_Bank","manufacture_bank","text"),
                    ("Manufacture_BankAddr1","manufacture_bankaddr1","text"),
                    ("Manufacture_BankAddr2","manufacture_bankaddr2","text"),
                    ("Manufacture_BankAddr3","manufacture_bankaddr3","text"),
                    ("Manufacture_DLno1","manufacture_dlno1","text"),
                    ("Manufacture_DLno2","manufacture_dlno2","text"),
                    ("Manufacture_TinNo1","manufacture_tinno1","text"),
                    ("Manufacture_TinNo2","manufacture_tinno2","text"),
                    ("Manufacture_CSTNo1","manufacture_cstno1","text"),
                    ("Manufacture_CSTNo2","manufacture_cstno2","text"),
                    ("Manufacture_GrpId","manufacture_grpid","numeric"),
                    ("Active","active","boolean"),
                    ("StaffId","staffid","bigint"),
                    ("EnterDate","enterdate","date"),
                    ("Manufacture_Code","manufacture_code","text"),
                    ("Manufacture_ProdCodeNextNo","manufacture_prodcodenextno","numeric"),
                    ("Manufacture_InclusiveSales","manufacture_inclusivesales","text"),
                    ("2","sms","boolean"),
                    ("3","mail","boolean"),
                    ("4","manufacture_image","text"),
                    ("Manufacture_Id","tempid","bigint"),
                    ("branchid","branchid","bigint"),
                    ("mainbranchid","mainbranchid","bigint"),
                }
                ,condition="where   branchid ="+nFromBranchId.ToString()
           },
           new TableMap
           {
            SqlTable = "Hsn",
            PgTable  = "hsn"+nMainBranchId.ToString(),
            Columns = new[]
            {
                ("Hsn_Id", "tempid","bigint"),
                ("Hsn_Code", "hsn_code","text"),
                ("Hsn_GstPers", "hsn_gstpers","numeric"),
                ("hsn_description1", "hsn_description1","text"),
                ("hsn_description2", "hsn_description2","text"),
                ("unitid", "unitid","bigint"),
                ("Hsn_Cess", "hsn_cess","numeric"),
                ("Hsn_AdditionalCess", "hsn_additionalcess","numeric"),
                ("Hsn_OtherCharge", "hsn_othercharge","numeric"),
                ("Hsn_AddRatePerUnit", "hsn_addrateperunit","numeric"),
                ("Hsn_ThousantRatePerUnit", "hsn_thousandrateperunit","numeric"),
                ("TaxGroupId", "taxid","bigint"),
                ("branchid", "branchid","bigint"),
                ("mainbranchid", "mainbranchid","bigint"),
            },
            condition="where   branchid ="+nFromBranchId.ToString()
           },
           new TableMap
           {
            SqlTable = "BillSeries",
            PgTable  = "billseries",
            Columns = new[]
            {
                // ("billserid","","bigint"),
                 ("BillSerDescription","billserdescription","text"),
                 ("BillSerPrefix","billserprefix","text"),
                 ("BillSerStartNo","billserstartno","bigint"),
                 ("BillSerCurrentBillNo","billsercurrentbillno","bigint"),
                 ("BillSerPayTerms","billserpayterms","text"),
                 ("BillType","billtype","text"),
                 ("BillwithTIN","billwithtin", "text"),
                 ("BillHide","billhide","text"),
                 ("PrintFileName","printfilename","text"),
                 ("printfilenameone","printfilenameone","text"),
                 ("PrintFilePreview","printfilepreview","text"),
                 ("printfilemobile","printfilemobile","text"),
                 ("Active","active","boolean"),
                 ("PriceMenuId","pricemenuid", "integer"),
                 ("PrintType","printtype", "text"),
                 ("BillSeriesAddCess","billseriesaddcess", "boolean"),
                 ("BillPriceMenuActive","billpricemenuactive","boolean"),
                 ("BillSeriesCustomerList","billseriescustomerlist","text"),
                 ("BillSeriesOrderNo","billseriesorderno","integer"),
                 ("BillSeriesStartDate","billseriesstartdate","date"),
                 ("BillSeriesEndDate","billseriesenddate","date"),
                 ("BillSerSuffix","billsersuffix", "text"),
                 ("BillSerMinusSymbol","billserminussymbol","text"),
                 ("BillSerBillNoFormat","billserbillnoformat","text"),
                 ("billserbillinclusive","billserbillinclusive","text"),
                 ("billsertaxadd","billsertaxadd","text"),
                 ("BillSerField1","billalternativesymbol","text"),
                 ("branchid","branchid", "integer"),
                 ("mainbranchid","mainbranchid","integer"),
                 ("BillSerId","tempid","bigint"),

            },
            Constants = new Dictionary<string, object>
            {
                 {"billsersource", "SALES" },
                 {"bdataclear","False"},
                 {"bbilled","False"},
                 {"bdownloadprint","False"},
                 {"billserbillinclusive","No" },
                 {"billsertaxadd","No" }
            },
            condition="where   branchid ="+nFromBranchId.ToString()
           },
           new TableMap
           {
                SqlTable = "Notes",
                PgTable  = "notes",

                Columns = new[]
                {
                    ("Note_Name", "notename", "text"),
                    ("Note_Description", "description", "text"),
                    ("Note_UserName", "username", "text"),
                    ("Note_Password", "pwd", "text"),
                    ("Note_Link", "urllink", "text"),
                    ("Note_ContactNo", "contactno", "text"),
                    ("Note_EnterDate", "enterdate", "date"),
                    ("Note_RemainterDate", "remainterdate", "date"),
                    ("Note_RemainterFlag", "remainterflag", "boolean"),
                    ("Note_RemainterCancel", "remaintercancel", "boolean"),
                    ("Note_ImageOne", "imageone", "text"),
                    ("Note_ImageTwo", "imagetwo", "text"),
                    ("Note_RenewalDate", "renewaldate", "date"),
                    ("CategoryId", "categoryid", "integer"),
                    ("StaffId", "staffid", "bigint"),
                    ("branchid", "branchid", "bigint"),
                    ("mainbranchid", "mainbranchid", "bigint"),
                    ("Note_Id", "tempid", "bigint")
                },
                 condition="where   branchid ="+nFromBranchId.ToString()
           },
           new TableMap
            {
                SqlTable = "Transporter",
                PgTable  = "transporter",

                Columns = new[]
                {
                    ("TransporterId", "transporterno", "text"),
                    ("TransporterName", "transportername", "text"),
                    ("branchid", "branchid", "bigint"),
                    ("mainbranchid", "mainbranchid", "bigint"),
                    ("UniqueKey", "tempid", "bigint")
                },
                 condition="where   branchid ="+nFromBranchId.ToString()
            },
           new TableMap
            {
                SqlTable = "ProductType",
                PgTable  = "producttype",

                Columns = new[]
                {
                    ("ProductType_Name", "producttypename", "text"),
                    ("ProductType_Active", "productactive", "boolean"),
                    ("ProductType_OnlineFlag", "producttypeonlineflag", "boolean"),
                    ("branchid", "branchid", "bigint"),
                    ("mainbranchid", "mainbranchid", "bigint")
                },
                 //condition="where   branchid ="+nFromBranchId.ToString()
            },
          new TableMap
           {
            SqlTable = "PurBillSeries",
            PgTable  = "billseries",
            Columns = new[]
            {
                // ("billserid","","bigint"),
                 ("PurBillSerDescription","billserdescription","text"),
                 ("PurBillSerPrefix","billserprefix","text"),
                 ("PurBillSerStartNo","billserstartno","bigint"),
                 ("PurBillSerCurrentBillNo","billsercurrentbillno","bigint"),
                 ("PurBillSerPayTerms","billserpayterms","text"),
                 ("PurBillType","billtype","text"),
                 ("PurBillwithTIN","billwithtin", "text"),
                 ("PurBillHide","billhide","text"),
                 ("PurPrintFileName","printfilename","text"),
                 ("PurPrintFilePreview","printfilepreview","text"),
                 ("Active","active","boolean"),
                 ("PurBillSerAddCess","billseriesaddcess", "boolean"),
                 ("branchid","branchid", "integer"),
                 ("mainbranchid","mainbranchid","integer"),
                 ("PurBillSerId","tempid","bigint"),

            },
            Constants = new Dictionary<string, object>
            {
                 {"printfilemobile","" },
                 {"printfilenameone","" },
                 {"pricemenuid", "0" },
                 {"printtype", "" },
                 {"billsersource", "PURCHASE" },
                 {"bdataclear","False"},
                 {"bbilled","False"},
                 {"bdownloadprint","False"},

                 {"billseriescustomerlist",""},
                 {"billseriesorderno","0"},
                 {"billseriesstartdate",DateTime.Now.ToString("yyyy-MM-dd")},
                 {"billseriesenddate",DateTime.Now.ToString("yyyy-MM-dd")},
                 {"billsersuffix", ""},
                 {"billserminussymbol",""},
                 {"billserbillnoformat",""},
                 {"billserbillinclusive","No"},
                 {"billsertaxadd",""},
            },
            condition="where   branchid ="+nFromBranchId.ToString()
           },

            new TableMap
            {
                SqlTable = "Product",
                PgTable  = "productmain"+nMainBranchId.ToString(),
                Columns = new[]
                {
                    // ---------- Identity ----------
                    ("ProductId", "tempid", "bigint"),

                    // ---------- Basic ----------
                    ("ItemCode", "itemcode", "text"),
                    ("SkuCode", "hsncode", "text"),
                    ("ItemDesc", "itemdesc", "text"),

                    ("CategoryCode", "categoryid", "integer"),
                    ("VendorCode", "vendorcode", "text"),
                    ("Brand", "brand", "text"),
                    ("Model", "model", "text"),

                    // ---------- Packing ----------
                    ("PackQty", "packqty", "integer"),
                    ("PackType", "unitid", "text"),

                    // ---------- Images ----------
                    ("ImageLoc", "imageloc", "text"),
                    ("FullItemDesc", "fullitemdesc", "text"),

                    // ---------- Manufacture ----------
                    ("Manufacture_Id", "manufacture_id", "bigint"),

                    // ---------- Flags ----------
                    ("bActive", "bactive", "boolean"),
                    ("bConsignment", "bconsignment", "boolean"),

                    // ---------- Edit ----------
                    ("EditDate", "editdate", "date"),
                    ("EditStaff", "editstaff", "text"),

                    // ---------- Color ----------
                    ("ColorCode", "colorcode", "text"),
                    ("ProductUsed", "productused", "integer"),
                    ("Color", "colorname", "text"),

                    // ---------- Tax ----------
                    ("TaxGroupId", "taxid", "integer"),

                    // ---------- Storage ----------
                    ("ColdeStorage", "coldestorage", "text"),

                    // ---------- Entry ----------
                    ("EnterDate", "enterdat", "date"),
                    ("Times", "times", "text"),

                    // ---------- Group ----------
                    ("ChemicalId", "chemicalid", "bigint"),
                    ("GroupId", "groupid", "integer"),

                    // ---------- Specification ----------
                    ("ProdTitle", "prodtitle", "text"),
                    ("ProdLumen", "prodlumen", "text"),
                    ("ProdMaterial", "prodmaterial", "text"),
                    ("ProdBeamAngle", "prodbeamangle", "text"),
                    ("ProdIPRating", "prodiprating", "text"),

                    // ---------- HSN ----------
                    ("Hsn_Id", "hsnid", "bigint"),

                    // ---------- Mapping ----------
                    ("ProdMapId", "prodmapid", "bigint"),
                    ("ProductLinkId", "productlinkid", "bigint"),

                    // ---------- Type ----------
                   

                    // ---------- More ----------
                    ("ProdSpecification", "prodspecification", "text"),
                    ("ProdDimension", "proddimension", "text"),
                    ("ProdMapItemName", "prodmapitemname", "text"),

                    // ---------- Weight ----------
                    ("ProdWeight", "prodweight", "numeric"),

                    // ---------- Grouping ----------
                    ("ProductGrpId", "productgrpid", "bigint"),
                    ("ProdWgtTypeId", "prodwgttypeid", "bigint"),

                    // ---------- Components ----------
                    ("ProdComponents", "prodcomponents", "text"),

                    // ---------- Links ----------
                    ("ProdLinkOrderId", "prodlinkorderid", "bigint"),
                    ("ProdLinkEShopId", "prodlinkeshopid", "bigint"),

                    // ---------- Custom ----------
                    ("SecDiscount", "sizeid", "bigint"),
                    ("ProdField1", "productsearch", "text"),
                    ("Field1", "stockchecking", "text"),

                    // ---------- Branch ----------
                   

                    // ---------- PostgreSQL only ----------
                    ("", "barcode", "text"),
                    ("NOS", "qtytype", "text"),

                    // ---------- Constant ----------
                    ("branchid", "branchid", "bigint"),
                    ("mainbranchid", "mainbranchid", "bigint")
                },
                 condition="where   branchid ="+nFromBranchId.ToString()
                ,
               Constants = new Dictionary<string, object>
               {
                    {  "producttype", "product"},
               }
            },
            new TableMap
            {
                SqlTable = "ServiceEntry",
                PgTable  = "productmain"+nMainBranchId.ToString(),
                Columns = new[]
                {
                    ("Service_Id", "tempid", "bigint"),
                    ("ItemCode", "itemcode", "text"),
                    ("SkuCode", "hsncode", "text"),
                    ("ItemDesc", "itemdesc", "text"),
                    ("CategoryId", "categoryid", "integer"),
                    ("UnitId", "unitid", "text"),
                    ("TaxGroupId", "taxid", "integer"),
                    ("HsnId", "hsnid", "bigint"),
                    ("UnitPrice", "prodweight", "numeric"),
                    ("branchid", "branchid", "bigint"),
                    ("mainbranchid", "mainbranchid", "bigint")
                },
               condition="where   branchid ="+nFromBranchId.ToString(),
               Constants = new Dictionary<string, object>
               {
                    {  "producttype", "serviceitem"},
                    {  "vendorcode", ""},
                    {  "brand", ""},
                    {  "model", ""},
                    {  "packqty", "0"},
                    {  "imageloc", ""},
                    {  "fullitemdesc", ""},
                    {  "manufacture_id", "0"},
                    {  "bactive", "True"},
                    {  "bconsignment", "False"},
                    {  "editdate", DateTime.Now.ToString("yyyy-MM-dd")},
                    {  "editstaff", "tet"},
                    {  "colorcode", ""},
                    {  "productused", "0"},
                    {  "colorname", ""},
                    {  "coldestorage", ""},
                    {  "enterdat",DateTime.Now.ToString("yyyy-MM-dd")},
                    {  "times", ""},
                    {  "chemicalid", "0"},
                    {  "groupid", "0"},
                    {  "prodtitle", ""},
                    {  "prodlumen", ""},
                    {  "prodmaterial", ""},
                    {  "prodbeamangle", ""},
                    {  "prodiprating", ""},
                    {  "prodmapid", "0"},
                    {  "productlinkid", "0"},
                    {  "prodspecification", ""},
                    {  "proddimension", ""},
                    {  "prodmapitemname", ""},
                    {  "productgrpid", "0"},
                    {  "prodwgttypeid", "0"},
                    {  "prodcomponents", ""},
                    {  "prodlinkorderid", "0"},
                    {  "prodlinkeshopid", "0"},
                    {  "sizeid", "0"},
                    {  "productsearch", ""},
                    {  "stockchecking", ""},
                    {  "barcode", ""},
                    {  "qtytype", "NOS"},
               }
            },
           new TableMap
            {
                SqlTable = "ProductSub",
                PgTable  = "productsub"+nMainBranchId.ToString(),

                Columns = new[]
                {
                    // ---------- Identity ----------
                    ("UniqueKey", "tempid", "bigint"),

                    // ---------- Keys ----------
                    ("ProductId", "productid", "bigint"),
                    

                    // ---------- Location ----------
                    ("ProdLocation", "location", "text"),

                    // ---------- Rates ----------
                    ("ProdPurRate", "purrate", "numeric"),
                    ("ProdSelRate", "selrate", "numeric"),
                    ("ProdWhRate", "whrate", "numeric"),
                    ("ProdMrp", "mrp", "numeric"),

                    // ---------- Special rates ----------
                    ("ProdSpRate1", "sprate1", "numeric"),
                    ("ProdSpRate2", "sprate2", "numeric"),
                    ("ProdSpRate3", "sprate3", "numeric"),
                    ("ProdSpRate4", "sprate4", "numeric"),
                    ("ProdSpRate5", "sprate5", "numeric"),

                    // ---------- Pack rates (not in SQL → default 0) ----------
                    ("0", "pcsselrate", "numeric"),
                    ("0", "pcswhrate", "numeric"),
                    ("0", "pcsmrp", "numeric"),

                    ("", "pcssprate1", "numeric"),
                    ("", "pcssprate2", "numeric"),
                    ("", "pcssprate3", "numeric"),
                    ("", "pcssprate4", "numeric"),
                    ("", "pcssprate5", "numeric"),

                    // ---------- Discounts ----------
                    ("0", "salesdiscount", "numeric"),

                    // ---------- Stock ----------
                    ("ProdRol", "rolqty", "numeric"),

                    // ---------- Neethi ----------
                    ("ProdNeethiDis", "neethidis", "numeric"),

                    // ---------- Flags ----------
                    ("bOnline", "bonline", "boolean"),
                    ("bInventoryItem", "binventoryitem", "boolean"),

                    // ---------- Constant ----------
                    ("branchid", "branchid", "bigint"),
                    ("mainbranchid", "mainbranchid", "bigint")
                },
                 condition="where   branchid ="+nFromBranchId.ToString()
            },

            new TableMap
            {
                SqlTable = "Issue",
                PgTable  = "issuemain"+nMainBranchId.ToString(),

                Columns = new[]
                {
                    // ---------- Identity ----------
                    // ("Issue_Id", "tempid", "bigint"),

                    // ---------- Bill ----------
                    ("BillSerId", "billserid", "bigint"),
                    ("Issue_SlNo", "issueno", "bigint"),
                    ("UniqueBillNo", "uniquebillno", "bigint"),
                    ("Issue_BillDate", "issuedate", "date"),

                    // ---------- Discount ----------
                    ("Issue_DisPers", "dispers", "numeric"),
                    ("Issue_DisAmt", "disamt", "numeric"),

                    // ---------- Account ----------
                    ("AcId", "acid", "bigint"),
                    ("Issue_CustName", "custname", "text"),

                    // ---------- Doctor / Sales ----------
                    ("Issue_DoctId", "doctid", "bigint"),
                    ("Issue_DoctName", "doctname", "text"),
                    ("SalesExeId", "salesexeid", "bigint"),

                    // ---------- Payment ----------
                    ("Issue_PayTerms", "payterms", "text"),

                    // ---------- Card ----------
                    ("Issue_CardNo", "cardno", "text"),
                    ("Issue_CardExpDate", "cardexpdate", "date"),
                    ("Issue_CardName", "cardname", "text"),

                    // ---------- Delivery ----------
                    ("Issue_Transporter", "transporter", "text"),
                    ("Issue_DispDate", "dispdate", "date"),
                    ("Issue_DueDate", "duedate", "date"),
                    ("Issue_OrderNo", "orderno", "text"),

                    // ---------- Charges ----------
                    ("Issue_BankCharge", "bankcharge", "numeric"),
                    ("Issue_Postage", "postage", "numeric"),
                    ("Issue_CrAmt", "cramt", "numeric"),
                    ("Issue_DbAmt", "dbamt", "numeric"),
                    ("Issue_Freight", "freight", "numeric"),
                    ("Issue_OtherCharge", "othercharge", "numeric"),

                    ("Issue_ExpiryAmt", "expiryamt", "numeric"),
                    ("Issue_ExpiryId", "expiryid", "bigint"),
                    ("Issue_RepAmt", "repamt", "numeric"),

                    // ---------- Totals ----------
                    ("Issue_DTotal", "dtotal", "numeric"),
                    ("Issue_ATotal", "atotal", "numeric"),

                    ("Issue_CSTPers", "cstpers", "numeric"),
                    ("Issue_CSTAmt", "cstamt", "numeric"),
                    ("Issue_ROF", "rof", "numeric"),
                    ("Issue_Total", "total", "numeric"),

                    // ---------- Status ----------
                    ("Issue_Cancel", "issuecancel", "text"),
                    ("Issue_Type", "issuepurtype", "text"),
                    ("DelFlag", "delflag", "text"),

                    // ---------- Notes ----------
                    ("StaffId", "staffid", "bigint"),
                    ("CrditNoteNos", "crditnotenos", "text"),
                    ("ExpCrNoteNos", "expcrnotenos", "text"),
                    ("CrditNoteDates", "crditnotedates", "text"),
                    ("ExpCrNoteDates", "expcrnotedates", "text"),

                    // ---------- Agent ----------
                    ("AgentId", "agentid", "bigint"),
                    ("BillCancelDate", "billcanceldate", "date"),
                    ("Issue_Print", "issueprint", "boolean"),
                    ("CancelStaffId", "cancelstaffid", "integer"),

                    // ---------- Payment ----------
                    ("Issue_PaidAmount", "paidamount", "numeric"),
                    ("Issue_OrderDate", "orderdate", "date"),
                    ("Issue_ChellaNo", "chellano", "text"),

                    ("Issue_AddDis", "adddis", "numeric"),
                    ("Issue_MinusDis", "minusdis", "numeric"),
                    ("Remarks", "remarks", "text"),

                    // ---------- Points ----------
                    ("Issue_RetValue", "retvalue", "numeric"),
                    ("Issue_PointSaleValue", "pointsalevalue", "numeric"),
                    ("Issue_PointAmount", "pointamount", "numeric"),
                    ("Issue_NoOfPoints", "noofpoints", "numeric"),

                    // ---------- Bank ----------
                    ("Issue_BankId", "bankid", "bigint"),
                    ("Issue_Address1", "address1", "text"),

                    // ---------- Voucher ----------
                    ("VType_SlNo", "vprefixid", "bigint"),
                    ("Trans_VoucherNo", "voucherno", "bigint"),
                    ("SalesVoucherUniqueId", "uniquevoucherid", "bigint"),
                    ("Issue_CRVoucherNo", "crvoucherno", "bigint"),
                    ("CRUniqueVoucherId", "cruniquevoucherid", "bigint"),

                    // ---------- Agent calc ----------
                    ("AgentPers", "agentpers", "numeric"),
                    ("AgentRateType", "agentratetype", "numeric"),
                    ("AgentSalesVaue", "agentsalesvaue", "numeric"),
                    ("AgentMarginAmt", "agentmarginamt", "numeric"),

                    // ---------- Transport ----------
                    ("Issue_VechileNo", "vechileno", "text"),
                    ("GodownId", "godownid", "numeric"),
                    ("Issue_GSTinNo", "gstinno", "text"),

                    // ---------- More ----------
                    ("Issue_AgentAmt", "agentamt", "numeric"),
                    ("Issue_AgentTDSAmt", "agenttdsamt", "numeric"),
                    ("Issue_CreditCardAmt", "creditcardamt", "numeric"),

                    ("Issue_AddCessFlag", "addcessflag", "boolean"),
                    ("Issue_CardServicePers", "cardservicepers", "numeric"),
                    ("Issue_CardServiceAmt", "cardserviceamt", "numeric"),

                    // ---------- TCS ----------
                    ("Issue_TenderReceiveAmt", "tenderreceiveamt", "numeric"),
                    ("Issue_TCSPers", "tcspers", "numeric"),
                    ("Issue_TCSAmt", "tcsamt", "numeric"),

                    // ---------- Transport ----------
                    ("Issue_TransportId", "transportid", "text"),
                    ("Issue_TransportName", "transportname", "text"),

                    // ---------- Order ----------
                    ("Issue_TempOrderNo", "temporderno", "numeric"),
                    ("Kot_Id", "kotid", "numeric"),
                    ("Table_Id", "tableid", "numeric"),
                    ("TableDetail_Id", "tabledetailid", "numeric"),
                    ("RoomRegistration_Id", "roomregistrationid", "numeric"),

                    // ---------- Currency ----------
                    ("Issue_CurrencyId", "currencyid", "numeric"),
                    ("Issue_CurrencyRate", "currencyrate", "numeric"),   
                    
                    // ---------- PostgreSQL-only ----------
                 
                    ("Issue_ShippingName", "shippingname", "text"),
                    ("Issue_ShippingAddr1", "shippingaddr1", "text"),
                    ("Issue_ShippingAddr2", "shippingaddr2", "text"),
                    ("Issue_Shippinggstno", "shippinggstno", "text"),
                    ("Issue_ShippingTransporter", "shippingtransporter", "text"),
                    ("Issue_ShippingState", "shippingstate", "text"),
                    ("Issue_ShippingStateCode", "shippingstatecode", "text"),
                    ("Field1", "smsno", "text"),
                    ("PhoneNo", "phoneno", "text"),
                    ("Field2", "otherdisplayname", "text"),
                    ("Issue_NoField1", "cashamt", "numeric"),
                    ("Issue_SaleType", "pricemenuid", "numeric"),
                    ("Issue_Block", "discname", "text"),
                    ("DirectRBank", "issuetime","text"),
                    //----- other ------
                    ("0","paytermsid","numeric"),
                    ("","orderfrom","text"),
                    ("","remarks1","text"),
                    ("","inclusivesales","text"),
                    ("","sourcefrom","text"),

                    // ---------- Constant ----------
                    ("branchid", "branchid", "bigint"),
                    ("mainbranchid", "mainbranchid", "bigint")
                },
                condition="where   branchid ="+nFromBranchId.ToString()
               ,Constants = new Dictionary<string, object>
               {
                  {"rewardcardid","0" }
               }
             },
             new TableMap
             {
                SqlTable = "IssueSubDetails",
                PgTable  = "issuesubdetails"+nMainBranchId.ToString(),

                Columns = new[]
                {
                    ("BillSerId", "billserid", "bigint"),
                    ("Issue_SlNo", "issueno", "bigint"),
                    ("UniqueBillNo", "uniquebillno", "bigint"),
                    ("Issue_BillDate", "issuedate", "date"),
                    ("IssueSub_Batch", "batch", "text"),
                    ("IssueSub_ExpDate", "expdate", "date"),
                    ("IssueSub_OriginalRate", "originalrate", "numeric"),
                    ("IssueSub_SelRate", "selrate", "numeric"),
                    ("IssueSub_DistRate", "whrate", "numeric"),
                    ("IssueSub_Mrp", "mrp", "numeric"),
                    ("IssueSub_Qty", "qty", "numeric"),
                    ("IssueSub_FreeQty", "freqty", "numeric"),
                    ("IssueSub_NoField3", "advfre", "numeric"),
                    ("IssueSub_RQty", "rqty", "numeric"),
                    ("IssueSub_RFreeQty", "rfreqty", "numeric"),
                    ("IssueSub_LQty", "lqty", "numeric"),
                    ("IssueSub_LooseFree", "loosefree", "numeric"),
                    ("IssueSub_TaxPers", "taxpers", "numeric"),
                    ("IssueSub_TaxAmt", "taxamt", "numeric"),
                    ("IssueSub_PdodDis", "itemdispers", "numeric"),
                    ("IssueSub_Amount", "amount", "numeric"),
                    ("ProductId", "productid", "bigint"),
                    ("Store_BatchSlNo", "batchslno", "bigint"),
                    ("IssueSub_Repl", "repl", "text"),
                    ("IssueSub_AmountBeforeDis", "amoutbefortax", "numeric"),
                    ("IssueSub_FlgSpecialRate", "flgspecialrate", "text"),
                    ("IssueSub_ActualRate", "actualrate", "numeric"),
                    ("DcSlNo", "dcslno", "numeric"),
                    ("Color", "color", "text"),
                    ("Unit", "unit", "text"),
                    ("Weight", "itemweight", "text"),
                    ("TaxId", "taxid", "integer"),
                    ("IssueSub_ProdDisAmt", "itemdisamt", "numeric"),
                    ("IssueSub_SchmPers", "schmpers", "numeric"),
                    ("IssueSub_SchmAmt", "schmamt", "numeric"),
                    ("IssueSub_Pack", "pack", "integer"),
                    ("IssueSub_PerRate", "perrate", "numeric"),
                    ("IssueSub_ProdType", "prodtype", "text"),
                    ("IssueSub_AmountBeforeDis", "amountbeforedis", "numeric"),
                    ("IssueSub_AddDisPers", "adddispers", "numeric"),
                    ("Field1", "pricemenuid", "integer"),
                    ("Field2", "inclusivesales", "text"),
                    ("SalesmanId", "salesmanid", "bigint"),
                    ("AgentPrice", "agentprice", "numeric"),
                    ("SalesmanPrice", "salesmanprice", "numeric"),
                    ("IssueSub_RMrp", "rmrp", "numeric"),
                    ("IssueSub_SpRate1", "sprate1", "numeric"),
                    ("IssueSub_SpRate2", "sprate2", "numeric"),
                    ("IssueSub_SpRate3", "sprate3", "numeric"),
                    ("IssueSub_SpRate4", "sprate4", "numeric"),
                    ("IssueSub_SpRate5", "sprate5", "numeric"),
                    ("IssueSub_SGSTTaxPers", "sgsttaxpers", "numeric"),
                    ("IssueSub_SGSTTaxAmount", "sgsttaxamount", "numeric"),
                    ("IssueSub_SGSTAmount", "sgstamount", "numeric"),
                    ("IssueSub_CGSTTaxPers", "cgsttaxpers", "numeric"),
                    ("IssueSub_CGSTTaxAmount", "cgsttaxamount", "numeric"),
                    ("IssueSub_CGSTAmount", "cgstamount", "numeric"),
                    ("IssueSub_IGSTTaxPers", "igsttaxpers", "numeric"),
                    ("IssueSub_IGSTTaxAmount", "igsttaxamount", "numeric"),
                    ("IssueSub_IGSTAmount", "igstamount", "numeric"),
                    ("IssueSub_GodownId", "godownid", "numeric"),
                    ("IssueSub_CessPers", "cesspers", "numeric"),
                    ("IssueSub_CessAmt", "cessamt", "numeric"),
                    ("IssueSub_NoField2", "neethidispers", "numeric"),
                    ("IssueSub_PackageId", "packageid", "numeric"),
                    ("IssueSub_PackageUniqueNo", "packageuniqueno", "numeric"),
                    ("IssueSub_ExtraCessPers", "extracesspers", "numeric"),
                    ("IssueSub_ExtraCessAmt", "extracessamt", "numeric"),
                    ("IssueSub_SpecialOrgRate", "specialorgrate", "numeric"),
                    ("IssueSub_NoField4", "extraschemeamt", "numeric"),
                    ("IssueSub_NoField5", "addrateperunit", "numeric"),
                    ("IssueSub_NoField6", "addrateunitamt", "numeric"),
                    ("branchid", "branchid", "bigint"),
                    ("mainbranchid", "mainbranchid", "bigint")
                },
                 condition="where   branchid ="+nFromBranchId.ToString()
            },

            new TableMap
            {
               SqlTable = "Receipt",
               PgTable  = "receiptmain"+nMainBranchId.ToString(),

               Columns = new[]
               {
                 ("Receipt_SlNo","receiptno","bigint"),
                 ("AC_Id","acid","bigint"),
                 ("Receipt_Date1","billdate1","date"),
                 ("Receipt_Date2","billdate2","date"),
                 ("Receipt_InvoNo","invono","text"),
                 ("Receipt_InvoDate","invodate","date"),
                 ("Receipt_InvoAmt","invoamt","numeric"),
                 ("Receipt_Total","total","numeric"),
                 ("Receipt_PayTerms","payterms","text"),
                 ("PurchaseId","paytermsid","bigint"),
                 ("Receipt_Type","purtype","text"),
                 ("Receipt_Freight","freight","numeric"),
                 ("Receipt_Discount","dispers","numeric"),
                 ("DisAmt","discamt","numeric"),
                 ("Field3","disctype","text"),
                 ("Receipt_Othercharge","othercharge1","numeric"),
                 ("Receipt_CrNo","crno","text"),
                 ("Receipt_CrDate","crdate","date"),
                 ("Receipt_CrAmt","cramt","numeric"),
                 ("Receipt_NoOfDbNote","noofdbnote","numeric"),
                 ("Receipt_DbNo","dbno","bigint"),
                 ("Receipt_DbDate","dbdate","date"),
                 ("Receipt_DbAmt","dbamt","numeric"),
                 ("Receipt_RepAmt","repamt","numeric"),
                 ("Receipt_ROF","rof","numeric"),
                 ("StaffId","staffid","bigint"),
                 ("PurBillSerId","billserid","bigint"),
                 ("Receipt_SupplierName","suppliername","text"),
                 ("Receipt_Address","addr","text"),
                 ("Receipt_Tin1","gstno","text"),
                 ("Receipt_AddCess","baddcess","boolean"),
                 ("Receipt_CurrencyId","currencyid","bigint"),
                 ("Receipt_CurrencyRate","currencyrate","numeric"),
                 ("Receipt_ExchangeAmt","exchangeamt","numeric"),
                 ("Receipt_TCSInPers","tcsinpers","numeric"),
                 ("Receipt_TCSInAmt","tcsinamt","numeric"),
                 ("Receipt_TCSCalValue","tcscalvalue","numeric"),
                 ("Receipt_TDSAmt","tdsamt","numeric"),
                 ("PDvoucherno","voucherno","bigint"),
                 ("UniqueVoucherId","uniquevoucherid","bigint"),
                 ("LandingCost","exciseduty","numeric"),
                 ("Field1","remarks","text"),
                 ("Receipt_Date2","duedate","date"),
                 ("branchid","branchid","bigint"),
                 ("mainbranchid","mainbranchid","bigint"),

               },
               Constants = new Dictionary<string, object>
               {
                 {"receipttime", TimeOnly.FromDateTime(DateTime.Now) },
                 {"invdiffamt","0"},
                 {"othercharge2","0"},
                 {"vprefixid","6"},
                 {"receiptcancel","False" },
                 {"invotype","Purchase" }
               },
               condition="where   branchid ="+nFromBranchId.ToString()
          },
          new TableMap
          {
            SqlTable = "ReceiptDetails",
            PgTable  = "receiptdetails"+nMainBranchId.ToString(),
            Columns = new[]
            {
              ("PurchaseId","receiptid","bigint"),
              ("ReceiptMain_SlNo","receiptno","bigint"),
              ("PurBillSerId","billserid","integer"),
              ("ReceiptSub_Date","receiptdate","date"),
              ("ReceiptSub_BatchSlNo","batchslno","bigint"),
              ("ReceiptSub_Batch","batch","text"),
              ("ReceiptSub_Pack","pack","numeric(18,0)"),
              ("ReceiptSub_ExpDate","expdate","date"),
              ("ReceiptSub_ReceiptRate","receiptrate","numeric"),
              ("ReceiptSub_SellRate","sellrate","numeric"),
              ("ReceiptSub_MRP","mrp","numeric"),
              ("ReceiptSub_PerRate","perrate","numeric"),
              ("ReceiptSub_PerSelRate","perselrate","numeric"),
              ("ReceiptSub_PerMRP","permrp","numeric"),
              ("ReceiptSub_SaleQty","saleqty","numeric"),
              ("ReceiptSub_SaleFree","salefree","numeric"),
              ("ReceiptSub_ReceiptQty","receiptqty","numeric"),
              ("ReceiptSub_ReceiptFree","receiptfree","numeric"),
              ("ReceiptSub_ReplaceQty","replaceqty","numeric"),
              ("ReceiptSub_LooseQty","looseqty","numeric"),
              ("ReceiptSub_TotalQty","totalqty","numeric"),
              ("ReceiptSub_NetAmtPerProd","netamtperprod","numeric"),
              ("ReceiptSub_Amount","amount","numeric"),
              ("ReceiptSub_BarCode","actpurrate","numeric"),
              ("ReceiptSub_WholeSaleRate","whrate","numeric"),
              ("ReceiptSub_SpRate1","sprate1","numeric"),
              ("ReceiptSub_SpRate2","sprate2","numeric"),
              ("ReceiptSub_SpRate3","sprate3","numeric"),
              ("ReceiptSub_SpRate4","sprate4","numeric"),
              ("ReceiptSub_SpRate5","sprate5","numeric"),
              ("ReceiptSub_ActualTaxPers","taxpers","numeric"),
              ("ReceiptSub_TaxAmt","taxamt","numeric"),
              ("ReceiptSub_WholSalMag","wholsalmag","numeric"),
              ("ReceiptSub_RetlMargin","retlmargin","numeric"),
              ("ReceiptSub_Period","schmeperiod","date"),
              ("ProductId","productid","bigint"),
              ("TaxId","taxid","integer"),
              ("ReceiptSub_LandCost","landcost","numeric"),
              ("ReceiptSub_ProdDiscount","dispers","numeric"),
              ("ReceiptSub_ProdDisAmt","disamt","numeric"),
              ("ReceiptSub_SchemePers","schemepers","numeric"),
              ("ReceiptSub_SchemeAmt","schemeamt","numeric"),
              ("ReceiptSub_Freight","freight","numeric"),
              ("ReceiptSub_TotLQty","totlqty","numeric"),
              ("ReceiptSub_NeethiDisPers","neethidispers","numeric"),
              ("ReceiptSub_AmtBeforeTax","amtbeforetax","numeric"),
              ("ReceiptSub_WRateDis","wratedis","numeric"),
              ("ReceiptSub_PerLandCost","perlandcost","numeric"),
              ("ReceiptSub_Field2","hsncode","text"),
              ("ReceiptSub_SGSTTaxPers","sgsttaxpers","numeric"),
              ("ReceiptSub_SGSTTaxAmount","sgsttaxamount","numeric"),
              ("ReceiptSub_SGSTAmount","sgstamount","numeric"),
              ("ReceiptSub_CGSTTaxPers","cgsttaxpers","numeric"),
              ("ReceiptSub_CGSTTaxAmount","cgsttaxamount","numeric"),
              ("ReceiptSub_CGSTAmount","cgstamount","numeric"),
              ("ReceiptSub_IGSTTaxPers","igsttaxpers","numeric"),
              ("ReceiptSub_IGSTTaxAmount","igsttaxamount","numeric"),
              ("ReceiptSub_IGSTAmount","igstamount","numeric"),
              ("ReceiptSub_CessPers","cesspers","numeric"),
              ("ReceiptSub_CessAmt","cessamt","numeric"),
              ("ReceiptSub_NoField2","imppurrate","numeric"),
              ("ReceiptSub_ExtraCessPers","extracesspers","numeric"),
              ("ReceiptSub_ExtraCessAmt","extracessamt","numeric"),
              ("ReceiptSub_ProdRemarks","prodremarks","text"),
              ("ReceiptSub_DcInNo","dcinno","bigint"),
              ("ReceiptSub_ActRateWithOutFre","actratewithoutfre","numeric"),
              ("ReceiptSub_NoField3","hsnrateperunit","numeric"),
              ("branchid","branchid","bigint"),
              ("mainbranchid","mainbranchid","bigint"),
              ("PurchaseId","priceid","bigint"),

             },
             Constants = new Dictionary<string, object>
             {
                  {"qtytype","NOS" },
                  {"prodpack","0"},
                  {"pcssellrate","0"},
                  {"pcswhrate","0"},
                  {"pcsmrp","0"},
                  {"pcssprate1","0"},
                  {"pcssprate2","0"},
                  {"pcssprate3","0"},
                  {"pcssprate4","0"},
                  {"pcssprate5","0"},
                  {"purratewithtax","0"},
                  {"receiptid","0"},
                 { "pcsactpurrate","0" },
             },
             condition="where   branchid ="+nFromBranchId.ToString()
          }
          ,
          new TableMap
          {
            SqlTable = "Store",
            PgTable  = "store"+nMainBranchId.ToString(),
            Columns = new[]
            {
                 ("PurchaseId","receiptid","bigint"),
                 ("Store_ReceiptSlNo","receiptno","bigint"),
                 ("PurBillSerId","billserid","integer"),
                 ("Store_ReceiptDate","receiptdate","date"),
                 ("Store_BatchSlNo","batchslno","bigint"),
                 ("Store_Batch","batch","text"),
                 ("Store_Pack","pack","numeric"),
                 ("Store_ExpDate","expdate","date"),
                 ("Store_ReceiptRate","receiptrate","numeric"),
                 ("Store_SellRate","sellrate","numeric"),
                 ("Store_MRP","mrp","numeric"),
                 ("Store_PerRate","perrate","numeric"),
                 ("Store_PerSellRate","perselrate","numeric"),
                 ("Store_PerMRP","permrp","numeric"),
                 ("Store_SaleQty","saleqty","numeric"),
                 ("Store_SaleFreeQty","salefree","numeric"),
                 ("Store_ReceiptQty","receiptqty","numeric"),
                 ("Store_ReceiptFreeQty","receiptfree","numeric"),
                 ("Store_AvgRate","perlandcost","numeric"),
                 ("Store_BarCode","actpurrate","numeric"),
                 ("Store_DisributRate","whrate","numeric"),
                 ("SpRate1","sprate1","numeric"),
                 ("SpRate2","sprate2","numeric"),
                 ("SpRate3","sprate3","numeric"),
                 ("SpRate4","sprate4","numeric"),
                 ("SpRate5","sprate5","numeric"),
                 ("Store_ProdTaxPers","taxpers","numeric"),
                 ("Store_ProdTaxAmt","taxamt","numeric"),
                 ("Store_WholSalMag","wholsalmag","numeric"),
                 ("Store_RetlMargin","retlmargin","numeric"),
                 ("Store_SchemeQty","schmeqty","numeric"),
                 ("Store_SchemeFreQty","schmefree","numeric"),
                 ("Store_SchemePeriod","schmeperiod","date"),
                 ("ProductId","productid","bigint"),
                 ("LandingCost","landcost","numeric"),
                 ("Store_ProdDiscount","dispers","numeric"),
                 ("Store_NeethiDisPers","neethidispers","numeric"),
                 ("Store_SGSTTaxPers","sgsttaxpers","numeric"),
                 ("Store_SGSTTaxAmount","sgsttaxamount","numeric"),
                 ("Store_SGSTAmount","sgstamount","numeric"),
                 ("Store_CGSTTaxPers","cgsttaxpers","numeric"),
                 ("Store_CGSTTaxAmount","cgsttaxamount","numeric"),
                 ("Store_CGSTAmount","cgstamount","numeric"),
                 ("Store_IGSTTaxPers","igsttaxpers","numeric"),
                 ("Store_IGSTTaxAmount","igsttaxamount","numeric"),
                 ("Store_IGSTAmount","igstamount","numeric"),
                 ("Store_CessPers","cesspers","numeric"),
                 ("Store_CessAmt","cessamt","numeric"),
                 ("Store_BalQty","balanceqty","numeric"),
                 ("TotQty","totqty","numeric"),
                 ("Store_Transaction","sourcefrom","text"),
                 ("Store_InvoDate","invdate","date"),
                 ("Store_InvoNo","invno","text"),
                 ("ACId","acid","bigint"),
                 ("branchid","branchid","bigint"),
                 ("mainbranchid","mainbranchid","bigint"),

            },
            Constants = new Dictionary<string, object>
            {
                {"qtytype","NOS" },
                 {"prodpack","1"},
                   {"replaceqty","0"},
                {"looseqty","0"},
                 {"netamtperprod","0"},
                   {"amount","0"},
                   {"pcssellrate","0"},
                 {"pcswhrate","0"},
                {"pcsmrp","0"},
                 {"pcssprate1","0"},
                 {"pcssprate2","0"},
                 {"pcssprate3","0"},
                 {"pcssprate4","0"},
                 {"pcssprate5","0"},
                  {"taxid","0"},
                  {"disamt","0"},
                  {"schemepers","0"},
                 {"schemeamt","0"},
                 {"purratewithtax","0"},
                 {"freight","0"},
                  {"transfrom",""},
                  {"godownid","0"},
                   {"priceid","0"},
                {"openingstkno","0"},
                 {"openingstkid","0"},
                  {"billdisc","0"},
                  {"amtbeforetax","0"},
                  {"wratedis","0"},
                   {"imppurrate","0"},
                 {"extracesspers","0"},
                 {"extracessamt","0"},
                {"actratewithoutfre","0"},
                 {"hsnrateperunit","0"},
                { "pcsactpurrate","0" },
                 { "stkbilledqty","0" },
            },
                 condition="where   branchid ="+nFromBranchId.ToString()
            },
            new TableMap
            {
               SqlTable = "OpeningStockMain",
               PgTable  = "openingstockmain"+nMainBranchId.ToString(),
               Columns = new[]
               {
                  //  ("","openingstockid","bigint"),
                      ("OpeningStockMain_No","openingstockno","bigint"),
                      ("AcId","acid","bigint"),
                      ("OpeningStockMain_BillDate","billdate1","date"),
                      ("OpeningStockMain_EnterDate","billdate2","date"),
                      ("OpeningStockMain_Id","paytermsid","bigint"),
                      ("OpeningStockMain_PurType","purtype","text"),
                      ("OpeningStockMain_Cancel","crno","text"),
                      ("OpeningStockMain_Total","total","numeric"),
                      ("StaffId","staffid","bigint"),
                      ("branchid","branchid","bigint"),
                      ("mainbranchid","mainbranchid","bigint")
               },
               Constants = new Dictionary<string, object>
               {
                      {"receipttime",TimeOnly.FromDateTime(DateTime.Now) },
                      {"invdiffamt","0"},
                      {"payterms",""},
                      {"billserid","0"},
                      {"suppliername",""},
                      {"addr",""},
                      {"gstno",""},
                      {"baddcess","False"},
                      {"currencyid","0"},
                      {"currencyrate","0"},
                      {"exchangeamt","0"},
                      {"tcsinpers","0"},
                      {"tcsinamt","0"},
                      {"tcscalvalue","0"},
                      {"tdsamt","0"},
                      {"voucherno","0"},
                      {"uniquevoucherid","0"},
                      {"vprefixno","0"},
                      {"exciseduty","0"},
                      {"remarks",""},
                      {"duedate",DateTime.Now.ToString("yyyy-MM-dd")},
                      {"receiptcancel","False"},
                      {"crdate",DateTime.Now.ToString("yyyy-MM-dd")},
                      {"cramt","0"},
                      {"noofdbnote","0"},
                      {"dbno","0"},
                      {"dbdate","date"},
                      {"dbamt","0"},
                      {"repamt","0"},
                      {"rof","0"},
                      {"freight","0"},
                      {"dispers","0"},
                      {"discamt","0"},
                      {"disctype",""},
                      {"othercharge1","0"},
                      {"othercharge2","0"},

               },
               condition="where   branchid ="+nFromBranchId.ToString()
            },
            new TableMap
            {
              SqlTable = "OpeningStock",
              PgTable  = "openingstockdetails"+nMainBranchId.ToString(),
              Columns = new[]
              {
                   ("OpeningStock_ReceiptSlNo","openingstockno","bigint"),
                   ("OpeningStock_ReceiptDate","openingstockdate","date"),
                   ("OpeningStock_BatchSlNo","batchslno","bigint"),
                   ("OpeningStock_Batch","batch","text"),
                   ("OpeningStock_Pack","pack","numeric"),
                   ("OpeningStock_ExpDate","expdate","date"),
                   ("OpeningStock_ReceiptRate","receiptrate","numeric"),
                   ("OpeningStock_SellRate","sellrate","numeric"),
                   ("OpeningStock_MRP","mrp","numeric"),
                   ("OpeningStock_PerRate","perrate","numeric"),
                   ("OpeningStock_PerSellRate","perselrate","numeric"),
                   ("OpeningStock_PerMRP","permrp","numeric"),
                   ("OpeningStock_SaleQty","saleqty","numeric"),
                   ("OpeningStock_SaleFreeQty","salefree","numeric"),
                   ("OpeningStock_ReceiptQty","receiptqty","numeric"),
                   ("OpeningStock_ReceiptFreeQty","receiptfree","numeric"),
                   ("OpeningStock_Amount","amount","numeric"),
                   ("OpeningStock_BarCode","actpurrate","numeric"),
                   ("OpeningStock_BarCode","packactpurrate","numeric"),
                   ("OpeningStock_DisributRate","whrate","numeric"),
                   ("SpRate1","sprate1","numeric"),
                   ("SpRate2","sprate2","numeric"),
                   ("SpRate3","sprate3","numeric"),
                   ("SpRate4","sprate4","numeric"),
                   ("SpRate5","sprate5","numeric"),
                   ("OpeningStock_ProdTaxPers","taxpers","numeric"),
                   ("OpeningStock_ProdTaxAmt","taxamt","numeric"),
                   ("LandingCost","landcost","numeric"),
                   ("OpeningStock_ProdDiscount","dispers","numeric"),
                   ("OpeningStock_DisAmt","disamt","numeric"),
                   ("LandingCost","perlandcost","numeric(18,3)"),
                   ("OpeningStock_SGSTTaxPers","sgsttaxpers","numeric"),
                   ("OpeningStock_SGSTTaxAmount","sgsttaxamount","numeric"),
                   ("OpeningStock_SGSTAmount","sgstamount","numeric"),
                   ("OpeningStock_CGSTTaxPers","cgsttaxpers","numeric"),
                   ("OpeningStock_CGSTTaxAmount","cgsttaxamount","numeric(18,3)"),
                   ("OpeningStock_CGSTAmount","cgstamount","numeric"),
                   ("OpeningStock_IGSTTaxPers","igsttaxpers","numeric"),
                   ("OpeningStock_IGSTTaxAmount","igsttaxamount","numeric"),
                   ("OpeningStock_IGSTAmount","igstamount","numeric"),
                   ("OpeningStock_CessPers","cesspers","numeric"),
                   ("OpeningStock_CessAmt","cessamt","numeric(18,3)"),
                   ("ProductId","productid","bigint"),
                   ("OpeningStockMain_Id","dcinno","bigint"),
                   ("branchid","branchid","bigint"),
                   ("mainbranchid","mainbranchid","bigint")
              },
              Constants = new Dictionary<string, object>
              {
                 {"billserid","0" },
                 {"packsellrate","0"},
                 {"packwhrate","0"},
                 {"packmrp","0"},
                 {"packsprate1","0"},
                 {"packsprate2","0"},
                 {"packsprate3","0"},
                 {"packsprate4","0"},
                 {"packsprate5","0"},
                 {"qtytype",""},
                 {"prodpack","0" },
                 {"schemepers","0"},
                 {"schemeamt","0"},
                 {"wholsalmag","0"},
                 {"retlmargin","0"},
                 {"schmeperiod",DateTime.Now.ToString("yyyy-MM-dd")},
                 {"replaceqty","0"},
                 {"looseqty","0"},
                 {"totalqty","0"},
                 {"netamtperprod","0"},
                 {"purratewithtax","0"},
                 {"freight","0"},
                 {"totlqty","0"},
                 {"neethidispers","0"},
                 {"amtbeforetax","0"},
                 {"wratedis","0"},
                 {"taxid","0"},
                 {"hsncode",""},
                 {"imppurrate","0"},
                 {"actratewithoutfre","0"},
                 {"hsnrateperunit","0"},
                 {"extracesspers","0"},
                 {"extracessamt","0"},
                 {"prodremarks",""}

              },
              condition="where   branchid ="+nFromBranchId.ToString()
            },
            new TableMap
            {
                  SqlTable = "ReceiptReturnMain",
                  PgTable  = "receiptreturnmain"+nMainBranchId.ToString(),
                  Columns = new[]
                  {
                      ("ReceiptReturnMain_Id", "billserid", "bigint"),
                      ("ReceiptReturnMain_SlNo", "receiptreturnno", "bigint"),
                      ("ReceiptReturnMain_Date", "receiptreturndate", "date"),
                      ("ReceiptReturnMain_Time", "receiptreturntme", "text"),
                      ("ReasonId", "reasonid", "bigint"),
                      ("ReceiptReturn_Cancel", "returncancel", "boolean"),
                      ("Field3", "selectionname", "text"),
                      ("ReturnSelectionId", "selectionid", "bigint"),
                      ("StaffId", "staffid", "bigint"),
                      ("branchid", "branchid", "bigint"),
                      ("mainbranchid", "mainbranchid", "bigint")
                  },
                  Constants = new Dictionary<string, object>
                  {
                  },
                  condition="where   branchid ="+nFromBranchId.ToString()
            },
            new TableMap
            {
                 SqlTable = "ReceiptReturn",
                 PgTable  = "receiptreturndetails"+nMainBranchId.ToString(),
                 Columns = new[]
                 {

                     ("ReceiptRet_Date", "billdate", "date"),
                     ("ReceiptMain_SlNo", "receiptno", "bigint"),
                     ("ReceiptReturnMain_Id", "receiptreturnmainid", "bigint"),
                     ("ReceiptRet_BatchSlNo", "batchslno", "bigint"),
                     ("ReceiptRet_Batch", "batch", "text"),
                     ("ReceiptRet_ExpDate", "expdate", "date"),
                     ("ReceiptRet_ReceiptRate", "receiptrate", "numeric"),
                     ("ReceiptRet_SellRate", "sellrate", "numeric"),
                     ("ReceiptRet_MRP", "mrp", "numeric"),
                     ("ReceiptRet_StockReturn", "stockreturn", "numeric"),
                     ("ReasonId", "reasonid", "bigint"),
                     ("AcId", "acid", "bigint"),
                     ("ProductId", "productid", "bigint"),
                     ("Receipt_AvgRate", "avgrate", "numeric"),
                     ("DebitFlag", "debitflag", "character varying(20)"),
                     ("StaffId", "staffid", "bigint"),
                     ("ReceiptSub_Date", "receiptsubdate", "date"),
                     ("ReceiptSub_TaxPers", "receiptsubtaxpers", "numeric"),
                     ("ReceiptMain_InvoNo", "invono", "text"),
                     ("ReceiptMain_InvoDate", "invodate", "date"),
                     ("ReceiptMain_Date", "receiptmaindate", "date"),
                     ("ReceiptSub_ReceiptQty", "receiptqty", "numeric"),
                     ("ReceiptSub_ReceiptFree", "receiptfree", "numeric"),
                     ("ReceiptSub_TaxAmt", "taxamt", "numeric"),
                     ("ReceiptSub_DisPer", "receiptsubdisper", "numeric"),
                     ("CurDateTime", "curdatetime", "text"),
                     ("ReceiptRet_Pack", "retpack", "integer"),
                     ("ReceiptRet_Qty", "receiptretqty", "numeric"),
                     ("ReceiptRet_LooseQty", "receiptretlooseqty", "numeric"),
                     ("ReceiptRet_SGSTTaxPers", "sgsttaxpers", "numeric"),
                     ("ReceiptRet_SGSTTaxAmount", "sgsttaxamount", "numeric"),
                     ("ReceiptRet_SGSTAmount", "sgstamount", "numeric"),
                     ("ReceiptRet_CGSTTaxPers", "cgsttaxpers", "numeric"),
                     ("ReceiptRet_CGSTTaxAmount", "cgsttaxamount", "numeric"),
                     ("ReceiptRet_CGSTAmount", "cgstamount", "numeric"),
                     ("ReceiptRet_IGSTTaxPers", "igsttaxpers", "numeric"),
                     ("ReceiptRet_IGSTTaxAmount", "igsttaxamount", "numeric"),
                     ("ReceiptRet_IGSTAmount", "igstamount", "numeric"),
                     ("ReceiptRet_PurchaseId", "receiptid", "numeric"),
                     ("ReceiptRet_RetAmount", "retamount", "numeric"),
                     ("ReceiptRet_CessPers", "cesspers", "numeric"),
                     ("ReceiptRet_CessAmount", "cessamount", "numeric"),
                     ("ReceiptRet_DebitQty", "debitqty", "numeric"),
                     ("ReceiptRet_TotDis", "totdis", "numeric"),
                     ("branchid", "branchid", "bigint"),
                     ("mainbranchid", "mainbranchid", "bigint")
                 },
                 Constants = new Dictionary<string, object>
                 {
                     {"qtytype", "NOS" },
                     { "totqty", "0"},
                     { "receiptreturnno", "0"}
                 },
                 condition="where   branchid ="+nFromBranchId.ToString()
            },
            new TableMap
            {
                SqlTable = "DebitNote",
                PgTable  = "debitnotemain"+nMainBranchId.ToString(),
                Columns = new[]
                {
                   // ("", "debitnoteid", "bigint"),
                    ("DebitNote_SlNo", "debitnoteno", "bigint"),
                    ("DebitNoteId", "billserid", "bigint"),
                    ("DebitNote_Discount", "dispers", "numeric"),
                    ("DisAmt", "disamt", "numeric"),
                    ("DebitNote_Othercharge", "othercharge", "numeric"),
                    ("DebitNote_DbDate", "dbdate", "date"),
                    ("DebitNote_ROF", "rof", "numeric"),
                    ("DebitNote_Total", "total", "numeric"),
                    ("StaffId", "staffid", "bigint"),
                    ("AC_Id", "acid", "bigint"),
                    ("DebitNote_Cancel", "dbcancel", "text"),
                    ("DebitNote_PurType", "purtype", "text"),
                    ("DebitNote_VehicleNo", "vehicleno", "text"),
                    ("DebitNote_TransportId", "transportid", "text"),
                    ("DebitNote_TransportName", "transportname", "text"),
                    ("Field1", "remarks", "text"),
                    ("DebitNote_ShippingName", "shippingname", "text"),
                    ("DebitNote_ShippingAddr1", "shippingaddr1", "text"),
                    ("DebitNote_ShippingAddr2", "shippingaddr2", "text"),
                    ("DebitNote_Shippinggstno", "shippinggstno", "text"),
                    ("DebitNote_ShippingTransporter", "shippingtransporter", "text"),
                    ("DebitNote_ShippingState", "shippingstate", "text"),
                    ("DebitNote_ShippingStateCode", "shippingstatecode", "text"),
                    ("PDvoucherno", "voucherno", "bigint"),
                    ("UniqueVoucherId", "uniquevoucherid", "bigint"),
                    ("branchid", "branchid", "bigint"),
                    ("mainbranchid", "mainbranchid", "bigint"),

                },
                Constants = new Dictionary<string, object>
                {
                     { "printheadername", "" },
                     { "vprefixid", "6"},
                     { "entrytype", "product"},
                },
                condition="where   branchid ="+nFromBranchId.ToString()
            },
            new TableMap
            {
                SqlTable = "DebitNoteDetails",
                PgTable  = "debitnotedetails"+nMainBranchId.ToString(),
                Columns = new[]
                {
                    ("DebitNoteId", "debitnoteid", "bigint"),
                    ("DebitNoteMain_SlNo", "debitnoteno", "bigint"),
                    ("DebitNoteSub_Date", "subdate", "date"),
                    ("DebitNoteSub_BatchSlNo", "batchslno", "bigint"),
                    ("DebitNoteSub_Batch", "batch", "text"),
                    ("DebitNoteSub_Pack", "pack", "integer"),
                    ("DebitNoteSub_ExpDate", "expdate", "date"),
                    ("DebitNoteSub_PurRate", "purrate", "numeric"),
                    ("DebitNoteSub_SelRate", "selrate", "numeric"),
                    ("DebitNoteSub_MRP", "mrp", "numeric"),
                    ("DebitNoteSub_Qty", "qty", "numeric"),
                    ("DebitNoteSub_FreQty", "freqty", "numeric"),
                    ("DebitNoteSub_Amount", "amount", "numeric"),
                    ("DebitNoteSub_TaxPercentage", "taxper", "numeric"),
                    ("DebitNoteSub_TaxAmt", "taxamt", "numeric"),
                    ("DebitNoteSub_ProdDiscount", "itemdisc", "numeric"),
                    ("ProductId", "productid", "bigint"),
                    ("TaxId", "taxid", "integer"),
                    ("DebitNoteSub_LandCost", "landcost", "numeric"),
                    ("ReceiptRet_Id", "receiptretid", "bigint"),
                    ("ReceiptRet_Type", "receiptrettype", "text"),
                    ("DebitNoteSub_AmountBeforeTax", "amountbeforetax", "numeric"),
                    ("DebitNoteSub_InvoNo", "invono", "text"),
                    ("DebitNoteSub_InvoDate", "invodate", "date"),
                    ("DebitNoteSub_SGSTTaxPers", "sgsttaxpers", "numeric"),
                    ("DebitNoteSub_SGSTTaxAmount", "sgsttaxamount", "numeric"),
                    ("DebitNoteSub_SGSTAmount", "sgstamount", "numeric"),
                    ("DebitNoteSub_CGSTTaxPers", "cgsttaxpers", "numeric"),
                    ("DebitNoteSub_CGSTTaxAmount", "cgsttaxamount", "numeric"),
                    ("DebitNoteSub_CGSTAmount", "cgstamount", "numeric"),
                    ("DebitNoteSub_IGSTTaxPers", "igsttaxpers", "numeric"),
                    ("DebitNoteSub_IGSTTaxAmount", "igsttaxamount", "numeric"),
                    ("DebitNoteSub_IGSTAmount", "igstamount", "numeric"),
                    ("DebitNoteSub_PurchaseId", "receiptid", "bigint"),
                    ("DebitNoteSub_ReceiptSlno", "receiptno", "bigint"),
                    ("DebitNoteSub_OldTaxAmount", "oldtaxamount", "numeric"),
                    ("DebitNoteSub_OldAmount", "oldamount", "numeric"),
                    ("DebitNoteSub_PurQty", "purqty", "numeric"),
                    ("DebitNoteSub_CessPers", "cesspers", "numeric"),
                    ("DebitNoteSub_CessAmt", "cessamt", "numeric"),
                    ("DebitNoteSub_TotDis", "totdis", "numeric"),
                    ("DebitNoteSub_DebitFreQty", "debitfreqty", "numeric"),
                    ("DebitNoteSub_ExtraCessPers", "extracesspers", "numeric"),
                    ("DebitNoteSub_ExtraCessAmt", "extracessamt", "numeric"),
                    ("branchid", "branchid", "bigint"),
                    ("mainbranchid", "mainbranchid", "bigint"),
                },
                Constants = new Dictionary<string, object>
                {
                    { "qtytype",    "NOS" },
                    {  "totqty",    "0"   },
                    {  "accid",     "0"   },
                    {  "hsnid",     "0"   },
                    {  "hsncode",   ""    },
                    { "itemdisamt", "0"   },
                    { "totdisamt",  "0"   }
                },
                condition="where   branchid ="+nFromBranchId.ToString()
            },
            new TableMap
            {
                SqlTable = "ExpiryDebitNote",
                PgTable  = "expirydebitnotemain"+nMainBranchId.ToString(),
                Columns = new[]
                {
                    ("ExpiryDebitNote_SlNo","expirydebitnoteno","bigint"),
                    ("ExpiryDebitNote_Discount","dispers","numeric"),
                    ("DisAmt","disamt","numeric"),
                    ("ExpiryDebitNote_Othercharge","othercharge","numeric"),
                    ("ExpiryDebitNote_DbNo","dbno","text"),
                    ("ExpiryDebitNote_DbDate","dbdate","date"),
                    ("ExpiryDebitNote_DbAmt","dbamt","numeric"),
                    ("ExpiryDebitNote_ROF","rof","numeric"),
                    ("ExpiryDebitNote_Total","total","numeric"),
                    ("StaffId","staffid","bigint"),
                    ("AC_Id","acid","bigint"),
                    ("PDvoucherno","voucherno","bigint"),
                    ("PDUniqueVoucherId","uniquevoucherid","bigint"),
                    ("ExpiryDebitNote_Cancel","debitcancel","text"),
                    ("ExpiryDebitNote_PurType","purtype","text"),
                    ("ExpiryDebitNote_TransportId","transportid","text"),
                    ("ExpiryDebitNote_TransportName","transportname","text"),
                    ("ExpiryDebitNote_ShippingName","shippingname","text"),
                    ("ExpiryDebitNote_ShippingAddr1","shippingaddr1","text"),
                    ("ExpiryDebitNote_ShippingAddr2","shippingaddr2","text"),
                    ("ExpiryDebitNote_Shippinggstno","shippinggstno","text"),
                    ("ExpiryDebitNote_ShippingTransporter","shippingtransporter","text"),
                    ("ExpiryDebitNote_ShippingState","shippingstate","text"),
                    ("ExpiryDebitNote_ShippingStateCode","shippingstatecode","text"),
                    ("ExpiryDebitNote_VehicleNo","vehicleno","text"),
                    ("branchid","branchid","bigint"),
                    ("mainbranchid","mainbranchid","bigint"),
                    ("ExpiryDebitNoteId","billserid","bigint")
                },
                Constants = new Dictionary<string, object>
                {
                    { "vprefixid","6" }
                },
                condition="where   branchid ="+nFromBranchId.ToString()
            },
             new TableMap
            {
                SqlTable = "ExpiryDebitNoteDetails",
                PgTable  = "expirydebitnotedetails"+nMainBranchId.ToString(),
                Columns = new[]
                {
                    ("ExpiryDebitNoteId","expirydebitnoteid","bigint"),
                    ("ExpiryDebitNoteMain_SlNo","expirydebitnotemainno","bigint"),
                    ("ExpiryDebitNoteSub_Date","expirydebitnotesubdate","date"),
                    ("ExpiryDebitNoteSub_BatchSlNo","batchslno","bigint"),
                    ("ExpiryDebitNoteSub_Batch","batch","text"),
                    ("ExpiryDebitNoteSub_Pack","pack","bigint"),
                    ("ExpiryDebitNoteSub_ExpDate","expdate","date"),
                    ("ExpiryDebitNoteSub_PurRate","purrate","numeric"),
                    ("ExpiryDebitNoteSub_SelRate","selrate","numeric"),
                    ("ExpiryDebitNoteSub_MRP","mrp","numeric"),
                    ("ExpiryDebitNoteSub_Qty","qty","numeric"),
                    ("ExpiryDebitNoteSub_Amount","amount","numeric"),
                    ("ExpiryDebitNoteSub_TaxPercentage","taxper","numeric"),
                    ("ExpiryDebitNoteSub_TaxAmt","taxamt","numeric"),
                    ("ExpiryDebitNoteSub_ProdDiscount","itemdispers","numeric"),
                    ("ProductId","productid","bigint"),
                    ("TaxId","taxid","bigint"),
                    ("ExpiryDebitNoteSub_LandCost","landcost","numeric"),
                    ("ReceiptRet_Id","receiptretid","bigint"),
                    ("ReceiptRet_Type","receiptrettype","text"),
                    ("ExpiryDebitNoteSub_AmountBeforeTax","amountbeforetax","numeric"),
                    ("ExpiryDebitNoteSub_InvoNo","invono","text"),
                    ("ExpiryDebitNoteSub_InvoDate","invodate","date"),
                    ("ExpiryDebitNoteSub_From","itemfrom","text"),
                    ("ExpiryDebitNoteSub_SGSTTaxPers","sgsttaxpers","numeric"),
                    ("ExpiryDebitNoteSub_SGSTTaxAmount","sgsttaxamount","numeric"),
                    ("ExpiryDebitNoteSub_SGSTAmount","sgstamount","numeric"),
                    ("ExpiryDebitNoteSub_CGSTTaxPers","cgsttaxpers","numeric"),
                    ("ExpiryDebitNoteSub_CGSTTaxAmount","cgsttaxamount","numeric"),
                    ("ExpiryDebitNoteSub_CGSTAmount","cgstamount","numeric"),
                    ("ExpiryDebitNoteSub_IGSTTaxPers","igsttaxpers","numeric"),
                    ("ExpiryDebitNoteSub_IGSTTaxAmount","igsttaxamount","numeric"),
                    ("ExpiryDebitNoteSub_IGSTAmount","igstamount","numeric"),
                    ("ExpiryDebitNoteSub_CessPers","cesspers","numeric"),
                    ("ExpiryDebitNoteSub_CessAmt","cessamt","numeric"),
                    ("ExpiryDebitNoteSub_ExtraCessPers","extracesspers","numeric"),
                    ("ExpiryDebitNoteSub_ExtraCessAmt","extracessamt","numeric"),
                    ("branchid","branchid","bigint"),
                    ("mainbranchid","mainbranchid","bigint")
                },
                Constants = new Dictionary<string, object>
                {
                    { "itemdisamt","0" }
                },
                condition="where   branchid ="+nFromBranchId.ToString()
             },
             new TableMap
             {
                 SqlTable = "VoucherDetails",
                 PgTable  = "voucherdetails"+nMainBranchId.ToString(),
                 Columns = new[]
                 {
                        ("Voucher_Date","voucherdate","date"),
                        ("Voucher_VoucherNo","voucherno","bigint"),
                        ("UniqueVoucherId","uniquevoucherid","bigint"),
                        ("VPrefix_No","vprefixid","integer"),
                        ("VoucherGroupId","vuchergroupid","bigint"),
                        ("Voucher_VoucherPrefix","voucherprefix","text"),
                        ("Voucher_ChequeNo","chequeno","text"),
                        ("Voucher_ChequeDate","chequedate","date"),
                        ("Voucher_Amt","voucheramt","numeric"),
                        ("Voucher_BankName","bankname","text"),
                        ("AC_Id","acid","bigint"),
                        ("RevAC_Id","revacid","bigint"),
                        ("Voucher_ReconAmt","reconamt","numeric"),
                        ("Voucher_ReconDate","recondate","date"),
                        ("Voucher_BankFlag","bankflag","text"),
                        ("RepId","repid","bigint"),
                        ("DelFlag","delflag","text"),
                        ("StaffId","staffid","integer"),
                        ("VoucherTime","vouchertime","text"),
                        ("Voucher_Description1","description1","text"),
                        ("Voucher_Description2","description2","text"),
                        ("BalanceAmt","balanceamt","numeric"),
                        ("EnterDate","enterdate","date"),
                        ("Voucher_TDSPers","tdspers","numeric"),
                        ("Voucher_TDSAmt","tdsamt","numeric"),
                        ("Voucher_Field5","adjbills","text"),
                        ("Remarks","remarks","text"),
                        ("branchid","branchid","bigint"),
                        ("mainbranchid","mainbranchid","bigint"),
                        ("Voucher_Field6","refno","text")
                 },
                 Constants = new Dictionary<string, object>
                 {
                     { "billno","0" },
                     { "billdate",DateTime.Now.ToString("yyyy-MM-dd")},
                     { "uniquebillno","0"},
                     { "billserid","0"},
                     { "remarks1",""},
                     { "bcancel","False"},
                     { "voucherorder","0"},
                     { "sourcefrom",""},
                 },
                 condition="where   branchid ="+nFromBranchId.ToString()
             },
             new TableMap
             {
                SqlTable = "ReturnAdjustmentLog",
                PgTable  = "returnadjustmentlog"+nMainBranchId.ToString(),
                Columns = new[]
                {
                    ("AcId","acid","bigint"),
                    ("BillNo","retbillno","bigint"),
                    ("UniqueNo","retuniquebillno","bigint"),
                    ("BillDate","retbilldate","date")      ,
                    ("VType_SlNo","vprefixid","integer"),
                    ("Voucher_VoucherNo","voucherno","bigint"),
                    ("UniqueVoucherId","uniquevoucherid","bigint"),
                    ("Amount","amount","numeric"),
                    ("ReceiveAmt","receiveamt","numeric"),
                    ("Narration","narration","text"),
                    ("PostFlag","postflag","text") ,
                    ("BillSerId","billserid","bigint"),
                    ("Issue_SlNo","issueno","bigint") ,
                    ("AdjustedDate","adjusteddate","date"),
                    ("AdjustedFlag","adjustedflag","text"),
                    ("branchid","branchid","bigint"),
                    ("Field1","adjustmenttype","text") ,
                    ("mainbranchid","mainbranchid","bigint"),                    
                },
                Constants = new Dictionary<string, object>
                {
                    { "postdate",DateTime.Now.ToString("yyyy-MM-dd") },
                    {"fromsource","" },
                    {"uniquebillno","" },
                    {"issuedate",DateTime.Now.ToString("yyyy-MM-dd") },
                },
                condition="where   branchid ="+nFromBranchId.ToString()
             },
              new TableMap
             {
                  SqlTable = "AccountLogFile",
                  PgTable  = "accountlogfile"+nMainBranchId.ToString(),
                  Columns = new[]
                  {
                        ("VoucherNo","voucherno","bigint"),
                        ("UniqueVoucherId","uniquevoucherid","bigint"),
                        ("Voucher_Prefix","vprefixid","integer"),
                        ("ReceiveVoucherNo","recvoucherno","bigint"),
                        ("CRUniqueVoucherId","recuniquevoucherid","bigint"),
                        ("ReceiveVoucherPrifix","recvprefixid","integer"),
                        ("BillAmount","billamount","numeric"),
                        ("AdjustAmt","adjustamt","numeric"),
                        ("LogDate","logdate","date"),
                        ("AcId","acid","bigint"),
                        ("Field1","remarks","text"),
                        ("BillSerId","billserid","bigint"),
                        ("BillNo","billno","bigint"),
                        ("UniqueBillNo","uniquebillno","bigint"),
                        ("BillDate","billdate","date"),
                        ("DisAmt","disamt","numeric"),
                        ("ReturnAmt","returnamt","numeric"),
                        ("branchid","branchid","bigint"),
                        ("mainbranchid","mainbranchid","bigint"),
                  },
                  Constants = new Dictionary<string, object>
                  {
                      { "remarks1","" },
                      { "times","" },
                  },
                  condition="where   branchid ="+nFromBranchId.ToString()
          },
          new TableMap
          {
                SqlTable = "ChequeEntry",
                PgTable  = "chequeentry"+nMainBranchId.ToString(),
                Columns = new[]
                {
                    ("ChequeEntry_TransDate","transdate","date"),
                    ("ChequeEntry_EnterDate","enterdate","date"),
                    ("ChequeEntry_ChequeNo","chequeno","text"),
                    ("ChequeEntry_ChequeDate","chequedate","date"),
                    ("ChequeEntry_NarrationName1","narrationname1","text"),
                    ("ChequeEntry_NarrationName2","narrationname2","text"),
                    ("ChequeEntry_Amt","chequeamt","numeric"),
                    ("ChequeEntry_BankName","bankname","text"),
                    ("VPrifix_Id","vprifixid","integer"),
                    ("Voucher_VoucherNo","voucherno","bigint"),
                    ("AC_Id","acid","bigint"),
                    ("Rec_Id","recid","bigint"),
                    ("StaffId","staffid","bigint"),
                    ("DelFlag","delflag","text"),
                    ("DepositFlag","depositflag","text"),
                    ("DepositDate","depositdate","date"),
                    ("BankCharge","bankcharge","numeric"),
                    ("UniqueVoucherId","uniquevoucherid","bigint"),
                    ("branchid","branchid","bigint"),
                    ("mainbranchid","mainbranchid","bigint"),
                },
                Constants = new Dictionary<string, object>
                {
                },
                condition="where   branchid ="+nFromBranchId.ToString()
          },
          new TableMap
          {
               SqlTable = "Outstanding",
               PgTable  = "outstanding"+nMainBranchId.ToString(),
               Columns = new[]
               {
                    ("AcId","acid","bigint"),
                    ("BillSerId","billserid","bigint"),
                    ("Issue_SlNo","billno","bigint"),
                    ("UniqueNo","uniquebillno","bigint"),
                    ("Issue_BillDate","billdate","date"),
                    ("Issue_Amount","billamount","numeric"),
                    ("ReceiveAmt","receiveamt","numeric"),
                    ("VType_SlNo","vprefixid","integer"),
                    ("Voucher_VoucherNo","voucherno","bigint"),
                    ("UniqueVoucherId","uniquevoucherid","bigint"),
                    ("Narration","narration","text"),
                    ("SalesManId","salesmanid","bigint"),
                    ("Field3","invdate","date"),
                    ("Field1","sourcefrom","text"),
                    ("branchid","branchid","bigint"),
                    ("mainbranchid","mainbranchid","bigint")
               },
               Constants = new Dictionary<string, object>
               {
                   {"sourcetype","" },
                   {"invno",""},
                   {"remarks",""},
               },
               condition="where   branchid ="+nFromBranchId.ToString()
          },
          new TableMap
          {
              SqlTable = "IssueReturn",
              PgTable  = "issuereturnmain"+nMainBranchId.ToString(),
              Columns = new[]
              {

                    ("IssueRetSlNo", "issuereturnno", "bigint"),
                    ("UniqueNo", "uniquereturnno", "bigint"),
                    ("IssueRetDate", "issuereturndate", "date"),
                    ("BillSerId", "salesbillserid", "bigint"),
                    ("Issue_SlNo", "issueno", "bigint"),
                    ("Issue_DoctId", "salesuniquebillno", "bigint"),
                    ("Issue_BillDate", "issuedate", "date"),
                    ("Issue_DisPers", "dispers", "numeric"),
                    ("Issue_DisAmt", "disamt", "numeric"),
                    ("AcId", "acid", "bigint"),
                    ("Issue_CustName", "custname", "text"),
                    ("Issue_DoctName", "doctname", "text"),
                    ("SalesExeId", "salesexeid", "bigint"),
                    ("IssueRet_PayTerms", "payterms", "text"),
                    ("Issue_CardNo", "cardno", "text"),
                    ("Issue_CardExpDate", "cardexpdate", "date"),
                    ("Issue_CardName", "cardname", "text"),
                    ("Issue_Transporter", "transporter", "text"),
                    ("Issue_DispDate", "dispdate", "date"),
                    ("Issue_DueDate", "duedate", "date"),
                    ("Issue_OrderNo", "orderno", "text"),
                    ("Issue_BankCharge", "bankcharge", "numeric"),
                    ("Issue_Postage", "postage", "numeric"),
                    ("Issue_DbAmt", "dbamt", "numeric"),
                    ("Issue_Freight", "freight", "numeric"),
                    ("Issue_OtherCharge", "othercharge", "numeric"),
                    ("Issue_DTotal", "dtotal", "numeric"),
                    ("Issue_ATotal", "atotal", "numeric"),
                    ("Issue_ROF", "rof", "numeric"),
                    ("Issue_CrAmt", "total", "numeric"),
                    ("Issue_Cancel", "returncancel", "text"),
                    ("Issue_Type", "returnpurtype", "text"),
                    ("DelFlag", "delflag", "text"),
                    ("StaffId", "staffid", "bigint"),
                    ("AgentId", "agentid", "bigint"),
                    ("CancelStaffId", "cancelstaffid", "integer"),
                    ("Issue_PayTerms", "chellano", "text"),
                    ("CrditNoteNos", "remarks", "text"),
                    ("VType_SlNo", "vprefixid", "bigint"),
                    ("Trans_VoucherNo", "voucherno", "bigint"),
                    ("SalesVoucherUniqueId", "uniquevoucherid", "bigint"),
                    ("GodownId", "godownid", "numeric"),
                    ("Issue_Field1", "gstinno", "text"),
                    ("branchid", "branchid", "integer"),
                    ("mainbranchid", "mainbranchid", "bigint")
              },
              Constants = new Dictionary<string, object>
              {
                        { "entrytype", "" },
                        { "agentamt", "0"},
                        { "agenttdsamt", "0"},
                        { "creditcardamt", "0"},
                        { "addcessflag", "False"},
                        { "cardservicepers", "0"},
                        { "cardserviceamt", "0"},
                        { "otherdisplayname", ""},
                        { "tcspers", "0"},
                        { "tcsamt", "0"},
                        { "transportid", ""},
                        { "transportname", ""},
                        { "cashamt", "0"},
                        { "temporderno", "0"},
                        { "currencyid", "0"},
                        { "currencyrate", "0"},
                        { "sourcefrom", ""},
                        { "doctid", "0"},
                        { "crvoucherno", "0"},
                        { "cruniquevoucherid", "0"},
                        { "agentpers", "0"},
                        { "agentratetype", "0"},
                        { "agentsalesvaue", "0"},
                        { "agentmarginamt", "0"},
                        { "vechileno", ""},
                        { "remarks1", ""},
                        { "inclusivesales", ""},
                        { "retvalue", "0"},
                        { "bankid", "0"},
                        { "smsno", ""},
                        { "phoneno", ""},
                        { "address1", ""},
                        { "issuetime", ""},
                        { "orderfrom", ""},
                        { "discname", ""},
                        { "pricemenuid", "0"},
                        { "salesbillamt", "0"},
                        { "paytermsid", "0"},

              },
              condition="where   branchid ="+nFromBranchId.ToString()
          },
          new TableMap
          {
             SqlTable = "IssueReturnDetails",
             PgTable  = "issuereturndetails"+nMainBranchId.ToString(),
             Columns = new[]
             {
                    ("IssueRetSlNo", "issuereturnno", "bigint"),
                    ("UniqueNo", "uniquereturnno", "bigint"),
                    ("BillSerId", "salesbillserid", "bigint"),
                    ("Issue_SlNo", "issueno", "bigint"),
                    ("Issue_BillDate", "issuedate", "date"),
                    ("IssueSub_Batch", "batch", "text"),
                    ("IssueSub_ExpDate", "expdate", "date"),
                    ("IssueSub_OriginalRate", "originalrate", "numeric"),
                    ("IssueSub_SelRate", "selrate", "numeric"),
                    ("IssueSub_DistRate", "whrate", "numeric"),
                    ("IssueSub_ActualRate", "actualrate", "numeric"),
                    ("IssueSub_Mrp", "mrp", "numeric"),
                    ("IssueSub_RQty", "qty", "numeric"),
                    ("IssueSub_RFreeQty", "freqty", "numeric"),
                    ("IssueSub_TaxPers", "taxpers", "numeric"),
                    ("IssueSub_TaxAmt", "taxamt", "numeric"),
                    ("IssueSub_PdodDis", "itemdispers", "numeric"),
                    ("IssueSub_Amount", "amount", "numeric"),
                    ("ProductId", "productid", "bigint"),
                    ("Store_BatchSlNo", "batchslno", "bigint"),
                    ("IssueSub_ProdDisAmt", "itemdisamt", "numeric"),
                    ("IssueRetSub_InclusiveSales", "inclusivesales", "text"),
                    ("IssueSub_SGSTTaxPers", "sgsttaxpers", "numeric"),
                    ("IssueSub_SGSTTaxAmount", "sgsttaxamount", "numeric"),
                    ("IssueSub_SGSTAmount", "sgstamount", "numeric"),
                    ("IssueSub_CGSTTaxPers", "cgsttaxpers", "numeric"),
                    ("IssueSub_CGSTTaxAmount", "cgsttaxamount", "numeric"),
                    ("IssueSub_CGSTAmount", "cgstamount", "numeric"),
                    ("IssueSub_IGSTTaxPers", "igsttaxpers", "numeric"),
                    ("IssueSub_IGSTTaxAmount", "igsttaxamount", "numeric"),
                    ("IssueSub_IGSTAmount", "igstamount", "numeric"),
                    ("IssueSub_CessPers", "cesspers", "numeric"),
                    ("IssueSub_CessAmt", "cessamt", "numeric"),
                    ("IssueSub_ExtraCessPers", "extracesspers", "numeric"),
                    ("IssueSub_ExtraCessAmt", "extracessamt", "numeric"),
                    ("IssueRetSub_CompCess", "addrateperunit", "numeric"),
                    ("IssueRetSub_CompCessAmt", "addrateunitamt", "numeric"),
                    ( "branchid","branchid", "bigint"),
                    ( "mainbranchid","mainbranchid", "bigint"),

             },
             Constants = new Dictionary<string, object>
             {
                    { "issuereturnid", "0"},
                    { "qtytype", "NOS"},
                    { "advfre", "0"},
                    { "rqty", "0"},
                    { "rfreqty", "0"},
                    { "lqty", "0"},
                    { "loosefree", "0"},
                    { "totqty", "0"},
                    { "issuereturndate",DateTime.Now.ToString("yyyy-MM-dd")},
                    { "salesuniquebillno", "0"},
                    { "oldamount", "0"},
                    { "batchslno1", "0"},
                    { "amoutbefortax", "0"},
                    { "flgspecialrate", ""},
                    { "color", ""},
                    { "unit", ""},
                    { "taxid", "0"},
                    { "schmpers", "0"},
                    { "schmamt", "0"},
                    { "prodpack", "0"},
                    { "pack", "0"},
                    { "perrate", "0"},
                    { "prodtype", ""},
                    { "amountbeforedis", "0"},
                    { "adddispers", "0"},
                    { "pricemenuid", "0"},
                    { "salesmanid", "0"},
                    { "agentprice", "0"},
                    { "purrate", "0"},
                    { "orgpurrate", "0"},
                    { "salesmanprice", "0"},
                    { "rmrp", "0"},
                    { "sprate1", "0"},
                    { "sprate2", "0"},
                    { "sprate3", "0"},
                    { "sprate4", "0"},
                    { "sprate5", "0"},
                    { "pcsselrate", "0"},
                    { "pcsmrp", "0"},
                    { "pcswhrate", "0"},
                    { "pcssprate1", "0"},
                    { "pcssprate2", "0"},
                    { "pcssprate3", "0"},
                    { "pcssprate4", "0"},
                    { "pcssprate5", "0"},
                    { "godownid", "0"},
                    { "neethidispers", "0"},
                    { "packageid", "0"},
                    { "packageuniqueno", "0"},
                    { "specialorgrate", "0"},
                    { "extraschemeamt", "0"},
                    { "prodfrom", ""},
                    { "hsnid", "0"},
                    { "hsncode", ""},
                    { "acid", "0"},
                    { "priceid", "0" }

             },
             condition="where   branchid ="+nFromBranchId.ToString()
          },
          new TableMap
          {
              SqlTable = "ExpiryReturn",
              PgTable  = "expiryreturnmain"+nMainBranchId.ToString(),
              Columns = new[]
              {
                    ("ExpiryRetSlNo", "expiryreturnno", "bigint"),
                    ("ExpiryRetDate", "expiryreturndate", "date"),
                    ("Expiry_Id", "billserid", "bigint"),
                    ("Expiry_DisPers", "dispers", "numeric"),
                    ("Expiry_DisAmt", "disamt", "numeric"),
                    ("AcId", "acid", "bigint"),
                    ("SalesExeId", "salesexeid", "bigint"),
                    ("Expiry_PayTerms", "payterms", "text"),
                    ("Expiry_OtherCharge", "othercharge", "numeric"),
                    ("Expiry_DTotal", "dtotal", "numeric"),
                    ("Expiry_ATotal", "atotal", "numeric"),
                    ("Expiry_ROF", "rof", "numeric"),
                    ("Expiry_Total", "total", "numeric"),
                    ("Expiry_Cancel", "expirycancel", "text"),
                    ("StaffId", "staffid", "bigint"),
                    ("VType_SlNo", "vprefixid", "integer"),
                    ("Trans_VoucherNo", "voucherno", "bigint"),
                    ("SalesVoucherUniqueId", "uniquevoucherid", "bigint"),
                    ("GodownId", "godownid", "numeric"),
                    ("Expiry_GstNo", "gstinno", "text"),
                    ("branchid", "branchid", "integer"),
                    ("mainbranchid", "mainbranchid", "bigint")
              },
              Constants = new Dictionary<string, object>
              {


                 {"custname", ""},
                    {"doctid", "0"},
                    {"doctname", ""},
                    {"paytermsid", "0"},
                    {"cardno", ""},
                    {"cardexpdate",DateTime.Now.ToString("yyyy-MM-dd")},
                    {"cardname", ""},
                    {"transporter", ""},
                    {"duedate", DateTime.Now.ToString("yyyy-MM-dd")},
                    {"orderno", "0"},
                    {"bankcharge", "0"},
                    {"postage", "0"},
                    {"cramt", "0"},
                    {"dbamt", "0"},
                    {"freight", "0"},
                    {"expiryamt", "0"},
                    {"expiryid", "0"},
                    {"repamt", "0"},
                    {"cstpers", "0"},
                    {"cstamt", "0"},
                    {"issuepurtype", ""},
                    {"discname", ""},
                    {"delflag", ""},
                    {"issuetime", ""},
                    {"orderfrom", ""},
                    {"agentid", "0"},
                    {"cancelstaffid", "0"},
                    {"pricemenuid", "0"},
                    {"chellano", ""},
                    {"remarks", ""},
                    {"remarks1", ""},
                    {"inclusivesales", ""},
                    {"retvalue", "0"},
                    {"bankid", "0"},
                    {"smsno", ""},
                    {"phoneno", ""},
                    {"address1", ""},
                    {"crvoucherno", "0"},
                    {"cruniquevoucherid", "0"},
                    {"vechileno", ""},
                    {"agentamt", "0"},
                    {"agenttdsamt", "0"},
                    {"creditcardamt", "0"},
                    {"addcessflag", "False"},
                    {"cardservicepers", "0"},
                    {"cardserviceamt", "0"},
                    {"otherdisplayname", ""},
                    {"tcspers", "0"},
                    {"tcsamt", "0"},
                    {"transportid", ""},
                    {"transportname", ""},
                    {"cashamt", "0"},
                    {"currencyid", "0"},
                    {"currencyrate", "0"},
                    {"sourcefrom", ""},
              },
              condition="where   branchid ="+nFromBranchId.ToString()
          },
          new TableMap
          {
             SqlTable = "ExpiryReturnDetails",
             PgTable  = "expiryreturndetails"+nMainBranchId.ToString(),
             Columns = new[]
             {
                    ("ExpiryRetSlNo", "expiryreturnno", "bigint"),
                    ("Expiry_Id", "billserid", "bigint"),
                    ("Issue_SlNo", "issueno", "bigint"),
                    ("Issue_BillDate", "issuedate", "date"),
                    ("ExpirySub_Batch", "batch", "text"),
                    ("ExpirySub_ExpDate", "expdate", "date"),
                    ("ExpirySub_OriginalRate", "originalrate", "numeric"),
                    ("ExpirySub_SelRate", "selrate", "numeric"),
                    ("ExpirySub_Mrp", "mrp", "numeric"),
                    ("ExpirySub_Qty", "qty", "numeric"),
                    ("ExpirySub_FreeQty", "freqty", "numeric"),
                    ("ExpirySub_TaxPers", "taxpers", "numeric"),
                    ("ExpirySub_TaxAmt", "taxamt", "numeric"),
                    ("ExpirySub_PdodDis", "itemdispers", "numeric"),
                    ("ExpirySub_Amount", "amount", "numeric"),
                    ("ProductId", "productid", "bigint"),
                    ("Store_BatchSlNo", "batchslno", "bigint"),
                    ("ExpirySub_ActualRate", "actualrate", "numeric"),
                    ("TaxId", "taxid", "integer"),
                    ("ExpirySub_ProdDisAmt", "itemdisamt", "numeric"),
                    ("ExpirySub_SpecialDisPers", "schmpers", "numeric"),
                    ("ExpirySub_SpecialDisAmt", "schmamt", "numeric"),
                    ("ExpirySub_Pack", "pack", "integer"),
                    ("ExpirySub_InclusiveSales", "inclusivesales", "text"),
                    ("ExpirySub_PurRate", "purrate", "numeric"),
                    ("ExpirySub_SGSTTaxPers", "sgsttaxpers", "numeric"),
                    ("ExpirySub_SGSTTaxAmount", "sgsttaxamount", "numeric"),
                    ("ExpirySub_SGSTAmount", "sgstamount", "numeric"),
                    ("ExpirySub_CGSTTaxPers", "cgsttaxpers", "numeric"),
                    ("ExpirySub_CGSTTaxAmount", "cgsttaxamount", "numeric"),
                    ("ExpirySub_CGSTAmount", "cgstamount", "numeric"),
                    ("ExpirySub_IGSTTaxPers", "igsttaxpers", "numeric"),
                    ("ExpirySub_IGSTTaxAmount", "igsttaxamount", "numeric"),
                    ("ExpirySub_IGSTAmount", "igstamount", "numeric"),
                    ("ExpirySub_CessPers", "cesspers", "numeric"),
                    ("ExpirySub_CessAmt", "cessamt", "numeric"),
                    ("BillSerId", "packageuniqueno", "numeric"),
                    ("ExpirySub_ExtraCessPers", "extracesspers", "numeric"),
                    ("ExpirySub_ExtraCessAmt", "extracessamt", "numeric"),
                    ("Receipt_InvoNo", "invno", "text"),
                    ("Receipt_InvoDate", "invdate", "text"),
                    ("Receipt_SlNo", "receiptno", "bigint"),
                    ("Receipt_Date", "receiptdate", "date"),
                    ("ExpirySub_DebitFlag", "debitflag", "text"),
                    ("ExpirySub_DebitQty", "debitqty", "numeric"),
                    ("ExpirySub_Id","salesmanid", "0"),
                    ("branchid", "branchid", "bigint"),
                    ("mainbranchid", "mainbranchid", "bigint")
             },
             Constants = new Dictionary<string, object>
             {
                    {"uniquebillno", "0"},
                    {"whrate", "0"},
                    {"qtytype", ""},
                    {"advfre", "0"},
                    {"rqty", "0"},
                    {"rfreqty", "0"},
                    {"lqty", "0"},
                    {"loosefree", "0"},
                    {"totqty", "0"},
                    {"oldamount", "0"},
                    {"batchslno1", "0"},
                    {"repl", ""},
                    {"amoutbefortax", "0"},
                    {"flgspecialrate", ""},
                    {"dcslno", "0"},
                    {"color", ""},
                    {"unit", ""},
                    {"itemweight", ""},
                    {"prodpack", "0"},
                    {"perrate", "0"},
                    {"prodtype", ""},
                    {"amountbeforedis", "0"},
                    {"adddispers", "0"},
                    {"pricemenuid", "0"},

                    {"agentprice", "0"},
                    {"orgpurrate", "0"},
                    {"salesmanprice", "0"},
                    {"rmrp", "0"},
                    {"sprate1", "0"},
                    {"sprate2", "0"},
                    {"sprate3", "0"},
                    {"sprate4", "0"},
                    {"sprate5", "0"},
                    {"packselrate", ""},
                    {"packmrp", ""},
                    {"packwhrate", ""},
                    {"packsprate1", "0"},
                    {"packsprate2", "0"},
                    {"packsprate3", "0"},
                    {"packsprate4", "0"},
                    {"packsprate5", "0"},
                    {"godownid", "0"},
                    {"packageid", "0"},
                    {"specialorgrate", "0"},
                    {"extraschemeamt", "0"},
                    {"addrateperunit", "0"},
                    {"addrateunitamt", "0"},
                    {"prodfrom", ""},
                    {"neethidispers", "0"},
                    {"receiptid", "0"},

             },
             condition="where   branchid ="+nFromBranchId.ToString()
          },
           new TableMap
           {
             SqlTable = "EBillUserDetails",
             PgTable  = "ebilluserdetails".ToString(),

              Columns = new[]
              {

                    ("EBill_ClientId", "clientid", "text"),
                    ("EBill_ClientSecretId", "clientsecretid", "text"),
                    ("EBill_UserName", "ebill_username", "text"),
                    ("EBill_Pwd", "ebill_pwd", "text"),
                    ("EBill_UserClientId", "userclientid", "text"),
                    ("EBill_UserClientPwd", "userclientpwd", "text"),
                    ("EBill_EType", "etype", "text"),
                    ("EBill_EInvoiceUserName", "einvoiceusername", "text"),
                    ("EBill_EInvoicePwd", "einvoicepwd", "text"),
                    ("branchid", "branchid", "bigint"),
                    ("mainbranchid", "mainbranchid", "bigint"),
              },
              Constants = new Dictionary<string, object>
              {
                  { "ipaddress", "" },
                  { "sourcefrom", "" },
                  { "tempid", 0 },
              },
              condition="where   branchid ="+nFromBranchId.ToString()
           },
           new TableMap
           {
             SqlTable = "EInvoiceDetails",
             PgTable  =  "einvoicedetails"+nMainBranchId.ToString(),

              Columns = new[]
              {

                   ("AckNo", "ackno", "text"),
                   ("Irn", "irn", "text"),
                   ("SignedInvoice", "signedinvoice", "text"),
                   ("SignedQRCod", "signedqrcod", "text"),
                   ("Status", "statusflag", "text"),
                   ("EwbNo", "ewbno", "text"),
                   ("EwbDt", "ewbdt", "date"),
                   ("EwbValidTill", "ewbvalidtill", "date"),
                   ("Remarks", "remarks", "text"),
                   ("BillSerId", "billserid", "bigint"),
                   ("BillNo", "billno", "bigint"),
                   ("UniqueBillNo", "uniquebillno", "bigint"),
                   ("FromType", "fromtype", "text"),
                   ("AckDt", "ackdt", "date"),
                   ("EInvoiceHoldFlag", "bholdflag", "boolean"),
                   ("branchid", "branchid", "bigint"),
                   ("mainbranchid", "mainbranchid", "bigint")
              },
              Constants = new Dictionary<string, object>
              {
              },
              condition="where   branchid ="+nFromBranchId.ToString()
           },
           new TableMap
           {
             SqlTable =  "DeliveryNoteDetails",
             PgTable  =  "deliveryoutdetails"+nMainBranchId.ToString(),

              Columns = new[]
              {

                   ("UniqueId", "deliveryoutid", "bigint"),
                    ("Delivery_SlNo", "deliveryoutno", "bigint"),
                    ("Delivery_BillDate", "deliveryoutdate", "date"),
                    ("DeliverySub_Batch", "batch", "text"),
                    ("DeliverySub_ExpDate", "expdate", "date"),
                    ("DeliverySub_OriginalRate", "originalrate", "numeric(18,3)"),
                    ("DeliverySub_SelRate", "selrate", "numeric(18,3)"),
                    ("DeliverySub_Mrp", "mrp", "numeric(18,3)"),
                    ("DeliverySub_Qty", "qty", "numeric(18,3)"),
                    ("DeliverySub_FreeQty", "freqty", "numeric(18,3)"),
                    ("DeliverySub_LQty", "lqty", "numeric(18,3)"),
                    ("DeliverySub_LooseFree", "loosefree", "numeric(18,3)"),
                    ("DeliverySub_SalQty", "dcsaleqty", "numeric(18,3)"),
                    ("DeliverySub_RetQty", "dcretqty", "numeric(18,3)"),
                    ("DeliverySub_TaxPers", "taxpers", "numeric(18,3)"),
                    ("DeliverySub_TaxAmt", "taxamt", "numeric(18,3)"),
                    ("DeliverySub_DisPers", "itemdispers", "numeric(18,3)"),
                    ("DeliverySub_Amount", "amount", "numeric(18,3)"),
                    ("ProductId", "productid", "bigint"),
                    ("DeliverySub_ActualRate", "actualrate", "numeric(18,8)"),
                    ("TaxId", "taxid", "integer"),
                    ("DeliverySub_Pack", "pack", "integer"),
                    ("DeliverySub_PerRate", "perrate", "numeric(18,3)"),
                    ("DeliverySub_PurRate", "purrate", "numeric(18,3)"),
                    ("DeliverySub_SpRate1", "sprate1", "numeric(18,3)"),
                    ("DeliverySub_SpRate2", "sprate2", "numeric(18,3)"),
                    ("DeliverySub_SpRate3", "sprate3", "numeric(18,3)"),
                    ("DeliverySub_SpRate4", "sprate4", "numeric(18,3)"),
                    ("DeliverySub_SpRate5", "sprate5", "numeric(18,3)"),
                    ("DeliverySub_SGSTTaxPers", "sgsttaxpers", "numeric(18,3)"),
                    ("DeliverySub_SGSTTaxAmount", "sgsttaxamount", "numeric(18,3)"),
                    ("DeliverySub_SGSTAmount", "sgstamount", "numeric(18,3)"),
                    ("DeliverySub_CGSTTaxPers", "cgsttaxpers", "numeric(18,3)"),
                    ("DeliverySub_CGSTTaxAmount", "cgsttaxamount", "numeric(18,3)"),
                    ("DeliverySub_CGSTAmount", "cgstamount", "numeric(18,3)"),
                    ("DeliverySub_IGSTTaxPers", "igsttaxpers", "numeric(18,3)"),
                    ("DeliverySub_IGSTTaxAmount", "igsttaxamount", "numeric(18,3)"),
                    ("DeliverySub_IGSTAmount", "igstamount", "numeric(18,3)"),
                    ("branchid", "branchid", "bigint"),
                    ("mainbranchid", "mainbranchid", "bigint"),
              },
              Constants = new Dictionary<string, object>
              {

                  { "billserid", 0 },
                  { "uniquebillno", 0},
                  { "whrate", 0},
                  { "qtytype", "Nos" },
                  { "advfre", 0 },
                  { "totqty", 0 },
                  { "batchslno1",0 },
                  { "repl", "" },
                  { "amoutbefortax", 0},
                  { "flgspecialrate", "" },
                  { "dcslno", 0},
                  { "color", "" },
                  { "unit", "" },
                  { "itemweight", "" },
                  { "itemdisamt", 0 },
                  { "schmpers", 0 },
                  { "schmamt", 0 },
                  { "prodpack", 1 },
                  { "prodtype", "" },
                  { "amountbeforedis", 0 },
                  { "adddispers", 0 },
                  { "pricemenuid", 1 },
                  { "inclusivesales", "" },
                  { "salesmanid", 0 },
                  { "agentprice", 0 },
                  { "orgpurrate", 0 },
                  { "salesmanprice", 1 },
                  { "rmrp", 0 },
                  { "pcsselrate", 0 },
                  { "pcsmrp", 0 },
                  { "pcswhrate", 0 },
                  { "pcssprate1", 0 },
                  { "pcssprate2", 0 },
                  { "pcssprate3", 0 },
                  { "pcssprate4", 0 },
                  { "pcssprate5", 0 },
                  { "godownid", 0 },
                  { "cesspers", 0 },
                  { "cessamt", 0 },
                  { "neethidispers", 0 },
                  { "packageid", 0 },
                  { "packageuniqueno", 0 },
                  { "extracesspers", 0 },
                  { "extracessamt", 0 },
                  { "specialorgrate", 0 },
                  { "extraschemeamt", 0 },
                  { "addrateperunit", 0 },
                  { "addrateunitamt", 0 },
                  { "prodfrom", "" },
                  { "packpurrate", 0 },
                  { "packorgpurrate", 0 },
                  { "packpurlandingcost", 0 },
                  { "purlandingcost", 0 }
              },
              condition="where   branchid ="+nFromBranchId.ToString()
           },
           new TableMap
           {
             SqlTable = "DeliveryNote",
             PgTable  =  "deliveryoutmain"+nMainBranchId.ToString(),

              Columns = new[]
              {
                  ("Delivery_Id", "deliveryoutid", "bigint"),
                    ("Delivery_SlNo", "deliveryoutno", "bigint"),
                    ("UniqueId", "uniquebillno", "bigint"),
                    ("Delivery_BillDate", "deliveryoutdate", "date"),
                    ("AcId", "acid", "bigint"),
                    ("SalesExeId", "salesexeid", "bigint"),
                    ("Delivery_OrderDate", "dispdate", "date"),
                    ("Delivery_OrderNo", "orderno", "text"),
                    ("Delivery_OtherCharge", "othercharge", "numeric(18,3)"),
                    ("Delivery_DTotal", "dtotal", "numeric(18,3)"),
                    ("Delivery_ATotal", "atotal", "numeric(18,3)"),
                    ("Delivery_ROF", "rof", "numeric(18,3)"),
                    ("Delivery_Total", "total", "numeric(18,3)"),
                    ("Delivery_Cancel", "bcancel", "boolean"),
                    ("DelFlag", "delflag", "text"),
                    ("StaffId", "staffid", "bigint"),
                    ("BillCancelDate", "billcanceldate", "date"),
                    ("CancelStaffId", "cancelstaffid", "bigint"),
                    ("TransVoucherNo", "voucherno", "bigint"),
                    ("UniqueVoucherNo", "uniquevoucherid", "bigint"),
                    ("Delivery_VehicleNo", "vechileno", "text"),
                    ("Delivery_TransportId", "transportid", "text"),
                    ("Delivery_TransportName", "transportname", "text"),
                    ("Delivery_ShippingName", "shippingname", "text"),
                    ("Delivery_ShippingAddr1", "shippingaddr1", "text"),
                    ("Delivery_ShippingAddr2", "shippingaddr2", "text"),
                    ("Delivery_Shippinggstno", "shippinggstno", "text"),
                    ("Delivery_ShippingTransporter", "shippingtransporter", "text"),
                    ("Delivery_ShippingState", "shippingstate", "text"),
                    ("Delivery_ShippingStateCode", "shippingstatecode", "text"),
                    ("branchid", "branchid", "bigint"),
                    ("mainbranchid", "mainbranchid", "bigint")




              },
              Constants = new Dictionary<string, object>
              {
                  { "billserid", 0 },
                  { "dispers", 0},
                  { "disamt", 0},
                  { "custname", "" },
                  { "doctid", 0 },
                  { "doctname", "" },
                  { "payterms", "" },
                  { "paytermsid", 0 },
                  { "cardno", "" },
                  { "cardexpdate", DateTime.Now.ToString("yyyy-MM-dd") },
                  { "cardname", "text" },
                  { "transporter", "text" },
                  { "duedate", DateTime.Now.ToString("yyyy-MM-dd")},
                  { "bankcharge", 0 },
                  { "postage", 0 },
                  { "cstpers", 0 },
                  { "cstamt", 0 },
                  { "issuepurtype", "" },
                  { "discname", "" },
                  { "issuetime", "" },
                  { "orderfrom", "" },
                  { "pricemenuid", 0 },
                  { "remarks", "" },
                  { "remarks1", "" },
                  { "inclusivesales", "" },
                  { "bankid", 0 },
                  { "smsno", "" },
                  { "phoneno", "" },
                  { "address1", "" },
                  { "vprefixid", 0 },
                  { "crvoucherno", 0 },
                  { "cruniquevoucherid", 0 },
                  { "godownid", 0 },
                  { "gstinno", "" },
                  { "creditcardamt", 0 },
                  { "addcessflag", false },
                  { "cardservicepers", 0 },
                  { "cardserviceamt", 0 },
                  { "otherdisplayname", "" },
                  { "cashamt", 0 },
                  { "currencyid", 0 },
                  { "currencyrate", 0 },
                  { "sourcefrom", "" },


              },
              condition="where   branchid ="+nFromBranchId.ToString()
           },
           new TableMap
           {
             SqlTable = "GodownTransfer",
             PgTable  =  "godowntransfermain"+nMainBranchId.ToString(),

              Columns = new[]
              {


                    ("GodownTransferNo","godowntransferno","bigint"),
                    ("TransDate","godowntransferdate","date"),
                    ("StaffId","staffid","bigint"),
                    ("TransTime","transtime","text"),
                    ("TransAmt","transamt","numeric(12,3)"),
                    ("GodownId","godownid","integer"),
                    ("TransferVoucherNo","voucherno","bigint"),
                    ("TransferUniqueVoucherId","uniquevoucherid","bigint"),
                    ("AcId","acid","bigint"),
                    ("GodownTransfer_Cancel","billcancel","text"),
                    ("branchid","branchid","bigint"),
                    ("mainbranchid","mainbranchid","bigint"),
              },
              Constants = new Dictionary<string, object>
              {
                  { "vprefixid",0 },
              },
              condition="where   branchid ="+nFromBranchId.ToString()
           },
           new TableMap
           {
             SqlTable = "GodownTransferDetails",
             PgTable  =  "godowntransferdetails"+nMainBranchId.ToString(),

              Columns = new[]
              {

                    ("TransId","godowntransferid","bigint"),
                    ("ProductId","productid","bigint"),
                    ("Batch","batch","text"),
                    ("ExpDate","expdate","date"),
                    ("TransQty","transqty","numeric(12,3)"),
                    ("TransRate","transrate","numeric(12,3)"),
                    ("TransAmt","transamt","numeric(12,3)"),
                    ("BatchSlno","batchslno","bigint"),
                    ("Pack","pack","integer"),
                    ("TransTaxPers","taxpers","numeric(10,3)"),
                    ("branchid","branchid","bigint"),
                    ("mainbranchid","mainbranchid","bigint"),
              },
              Constants = new Dictionary<string, object>
              {

                  { "godowntransferno",0},
              },
              condition="where   branchid ="+nFromBranchId.ToString()
           },
            new TableMap
            {
                 SqlTable = "StockTransfer",
                 PgTable  =  "stocktransfermain"+nMainBranchId.ToString(),
                  Columns = new[]
                  {
                        ("ToBranch","billserid","bigint"),
                        ("TransDate","transdate","date"),
                        ("StaffId","staffid","bigint"),
                        ("TransTime","transtime","text"),
                        ("TransAmt","transamt","numeric(18,3)"),
                        ("TransferVoucherNo","voucherno","bigint"),
                        ("TransferUniqueVoucherId","uniquevoucherid","bigint"),
                        ("AcId","acid","bigint"),
                        ("StockTransferNo","stocktransferno","bigint"),
                        ("TransferPDVoucherNo","pdvoucherno","bigint"),
                        ("TransferPDUniqueVoucherId","pduniquevoucherid","bigint"),
                        ("FromAcId","fromacid","bigint"),
                        ("StockTransfer_VerficationStaffId","verficationstaffid","bigint"),
                        ("StockTransfer_VerficationDate","verficationdate","date"),
                        ("StockTransfer_BilledFlag","billedflag","text"),
                        ("StockTransfer_Cancel","bcancel","boolean"),
                        ("StockTransfer_PriceMenuId","pricemenuid","integer"),
                        ("mainbranchid","mainbranchid","bigint"),
                  },
                  Constants = new Dictionary<string, object>
                  {
                      { "frombranch",nBranchId },
                  },
                condition="where   frombranch ="+nFromBranchId.ToString()
            },
             new TableMap
             {
                 SqlTable = "StockTransferDetails",
                 PgTable  =  "stocktransferdetails"+nMainBranchId.ToString(),

                  Columns = new[]
                  {
                        ("TransId","transid","bigint"),
                        ("ProductId","productid","bigint"),
                        ("TransQty","transqty","numeric(18,3)"),
                        ("TransRate","transrate","numeric(18,3)"),
                        ("TransAmt","transamt","numeric(18,3)"),
                        ("BatchSlno","batchslno","bigint"),
                        ("branchid","branchid","bigint"),
                        ("Pack","pack","integer"),
                        ("TransTaxPers","taxpers","numeric(18,3)"),
                        ("tockTransfer_PurRate","purrate","numeric(18,3)"),
                        ("StockTransfer_SelRate","selrate","numeric(18,3)"),
                        ("StockTransfer_Mrp","mrp","numeric(18,3)"),
                        ("StockTransfer_SpRate1","sprate1","numeric(18,3)"),
                        ("StockTransfer_SpRate2","sprate2","numeric(18,3)"),
                        ("StockTransfer_SpRate3","sprate3","numeric(18,3)"),
                        ("StockTransfer_SpRate4","sprate4","numeric(18,3)"),
                        ("StockTransfer_SpRate5","sprate5","numeric(18,3)"),
                        ("StockTransfer_WhRate","whrate","numeric(18,3)"),
                        ("StockTransfer_LandingCost","landingcost","numeric(18,3)"),
                        ("StockTransfer_FromLandingCost","fromlandingcost","numeric(18,3)"),
                        ("StockTransfer_FromPurRate","frompurrate","numeric(18,3)"),
                        ("StockTransfer_VerficationFlag","verficationflag","text"),
                        ("StockTransfer_TaxId","taxid","numeric(18,3)"),
                        ("StockTransfer_TaxAmt","taxamt","numeric(18,3)"),
                        ("StockTransfer_SGSTTaxPers","sgsttaxpers","numeric(18,3)"),
                        ("StockTransfer_SGSTTaxAmt","sgsttaxamt","numeric(18,3)"),
                        ("StockTransfer_CGSTTaxPers","cgsttaxpers","numeric(18,3)"),
                        ("StockTransfer_CGSTTaxAmt","cgsttaxamt","numeric(18,3)"),
                        ("StockTransfer_IGSTTaxPers","igsttaxpers","numeric(18,3)"),
                        ("StockTransfer_IGSTTaxAmt","igsttaxamt","numeric(18,3)"),
                        ("StockTransfer_CessPers","cesspers","numeric(18,3)"),
                        ("StockTransfer_CessAmt","cessamt","numeric(18,3)"),
                        ("StockTransfer_Batch","batch","text"),
                        ("StockTransfer_ToBatchNo","tobatchno","bigint"),
                        ("mainbranchid","mainbranchid","bigint"),
                  },
                  Constants = new Dictionary<string, object>
                  {

                  },
                  condition="where   branchid ="+nFromBranchId.ToString()
             },
             new TableMap
            {
                SqlTable = "SalesOrderMain",
                PgTable  = "salesordermain" + nMainBranchId.ToString(),
                Columns = new[]
                {
                    ("UniqueId","salesorderid","bigint"),
                    ("SalesOrderMain_Id","salesorderno","bigint"),
                    ("SalesOrderMain_CustomerName","custname","text"),
                    ("AcId","acid","bigint"),
                    ("SalesOrderMain_Total","total","numeric"),
                    ("SalesOrderMain_Date","salesorderdate","date"),
                    ("SalesOrderMain_DisPers","dispers","numeric"),
                    ("SalesOrderMain_DisAmt","disamt","numeric"),
                    ("SalesOrderMain_ShippingChrge","freight","numeric"),
                    ("SalesOrder_Remarks","remarks","text"),
                    ("SalesOrderMain_ATotal","atotal","numeric"),
                    ("SalesOrderMain_OtherChg","othercharge","numeric"),
                    ("SalesOrderMain_PriceMenuId","pricemenuid","integer"),
                    ("SalesOrderMain_InclusiveFlag","inclusivesales","text"),
                    ("SalesOrderMain_TotTaxAmt","cstamt","numeric"),
                    ("SalesOrderMain_Time","issuetime","text"),        // time -> text
                    ("SalesOrderMain_DeliverDate","duedate","date"),   // or dispdate
                    ("SalesOrder_Cancel","issuecancel","text"),        // bit -> text
                    ("SalesOrder_Flag","statusflag","text"),           // or delflag / billedflag
                    ("SalesOrder_RepId","salesexeid","bigint"),        // or agentid
                    ("SalesOrder_From","orderfrom","text"),            // or sourcefrom
                    ("SalesOrderMain_Phone","phoneno","text"),         // or smsno
                    ("branchid","branchid","bigint"),
                    ("mainbranchid","mainbranchid","bigint"),
                },
                Constants = new Dictionary<string, object>
                {

                },
                condition = "where branchid =" + nFromBranchId.ToString()
            },
             new TableMap
            {
                SqlTable = "SalesOrderDetails",
                PgTable  = "salesorderdetails" + nMainBranchId.ToString(),
                Columns = new[]
                {
                    ("UniqueId","salesordersubid","bigint"),
                    ("SalesOrderMain_Id","salesorderid","bigint"),
                    ("ProductId","productid","bigint"),
                    ("SalesOrderSub_Qty","qty","numeric"),
                    ("SalesOrderSub_TaxPers","taxpers","numeric"),
                    ("SalesOrderSub_TaxAmt","taxamt","numeric"),
                    ("SalesOrderSub_PurRate","purrate","numeric"),
                    ("SalesOrderSub_SelRate","selrate","numeric"),
                    ("SalesOrderSub_Mrp","mrp","numeric"),
                    ("SalesOrderSub_DisPers","itemdispers","numeric"),
                    ("SalesOrderSub_BeforeTax","amoutbefortax","numeric"),
                    ("SalesOrderSub_Amount","amount","numeric"),
                    ("SalesOrderSub_BatchSlNo","batchslno","bigint"),
                    ("SalesOrderMain_Date","issuedate","date"),
                    ("SalesOrderSub_FreeQty","freqty","numeric"),
                    ("SalesOrderSub_TaxId","taxid","integer"),
                    ("SalesOrderSub_PriceMenuId","pricemenuid","integer"),
                    ("SalesOrderSub_RMrp","rmrp","numeric"),
                    ("SalesOrderSub_WhRate","whrate","numeric"),
                    ("SalesOrderSub_SpRate1","sprate1","numeric"),
                    ("SalesOrderSub_SpRate2","sprate2","numeric"),
                    ("SalesOrderSub_SpRate3","sprate3","numeric"),
                    ("SalesOrderSub_SpRate4","sprate4","numeric"),
                    ("SalesOrderSub_SpRate5","sprate5","numeric"),
                    ("SalesOrderSub_InclusiveSales","inclusivesales","text"),
                    ("SalesOrderSub_ConvertBillFlag","bconvertflag","boolean"),
                    ("SalesOrderSub_Wgt","itemweight","text"),           // weak — see note
                    ("branchid","branchid","bigint"),
                    ("mainbranchid","mainbranchid","bigint"),
                },
                Constants = new Dictionary<string, object>
                {
                    { "repl","" },
                    { "prodfrom","" },
                    { "specialorgrate", 0 },

                },
                //condition = "where branchid =" + nFromBranchId.ToString()
            },
             new TableMap
            {
                SqlTable = "Quotation",
                PgTable  = "quotationmain" + nMainBranchId.ToString(),
                Columns = new[]
                {
                    ("UniqueId","quotationid","bigint"),
                    ("Quotation_Id","quotationno","bigint"),
                    ("Quotation_EnterDate","enterdate","date"),
                    ("Quotation_DisPers","dispers","numeric"),
                    ("Quotation_DisAmt","disamt","numeric"),
                    ("AcId","acid","bigint"),
                    ("SalesExeId","salesexeid","bigint"),
                    ("Quotation_DTotal","dtotal","numeric"),
                    ("Quotation_ATotal","atotal","numeric"),
                    ("Quotation_ROF","rof","numeric"),
                    ("Quotation_Total","total","numeric"),
                    ("Quotation_Cancel","quotationcancel","text"),
                    ("StaffId","staffid","bigint"),
                    ("AgentId","agentid","bigint"),
                    ("BillCancelDate","billcanceldate","date"),
                    ("Quotation_Declaration","declaration","text"),
                    ("Quotation_Date","quotationdate","date"),
                    ("Quotation_OurRef","ourref","text"),
                    ("Quotation_CusRef","cusref","text"),
                    ("Quotation_OtherChg","othercharge","numeric"),
                    ("Quotation_GstNo","gstno","text"),
                    ("Quotation_SiteName","sitename","text"),
                    ("Quotation_ClosingDate","closingdate","date"),
                    ("Quotation_Enquiry","enquiry","text"),
                    // ↓ type-changed / ambiguous — confirm ↓
                    ("DelFlag","delflag","boolean"),            // varchar(10) -> boolean
                    ("Quotation_Type","entrytype","text"),  // or entrytype
                    ("Quotation_SaleType","pricemenuid","integer"),     // or quotationpurtype
                    ("Field1","remarks","text"),                // mirrors Receipt's Field1->remarks
                    ("branchid","branchid","bigint"),
                    ("mainbranchid","mainbranchid","bigint"),
                },
                Constants = new Dictionary<string, object>
                {
                    { "billserid",          0},
                    {" duedate ",           DateTime.Now.ToString("yyyy-MM-dd") },
                    {" discname",           "" },
                    {" quotationtime ",     "" },
                    {" orderfrom ",         "" },
                    {" pricemenuid"  ,      0 },
                    {" remarks1 ",          "" },
                    {" inclusivesales ",    ""},
                    {" smsno ",             "" },
                    {" phoneno  ",          "" },
                    {" address1",           "" }
                },
                condition = "where branchid =" + nFromBranchId.ToString()
            },
             new TableMap
            {
                SqlTable = "QuotationDetails",
                PgTable  = "quotationdetails" + nMainBranchId.ToString(),
                Columns = new[]
                {
                    ("QuotationSub_Id","quotationdetailsid","bigint"),
                    ("UniqueId","quotationid","bigint"),
                    ("Quotation_Id","quotationno","bigint"),
                    ("ProductId","productid","bigint"),
                    ("QuotationSub_SelRate","selrate","numeric"),
                    ("QuotationSub_Mrp","mrp","numeric"),
                    ("QuotationSub_Qty","qty","numeric"),
                    ("QuotationSub_TaxPers","taxpers","numeric"),
                    ("QuotationSub_TaxAmt","taxamt","numeric"),
                    ("QuotationSub_Amount","amount","numeric"),
                    ("TaxId","taxid","integer"),
                    ("QuotationSub_ProdType","prodtype","text"),
                    ("QuotationSub_AmountBeforeDis","amountbeforedis","numeric"),
                    ("QuotationSub_OriginalRate","originalrate","numeric"),
                    ("QuotationSub_DisPers","itemdispers","numeric"),
                    ("QuotationSub_AddDisPers","adddispers","numeric"),
                    ("QuotationSub_AmountBeforeTax","amoutbefortax","numeric"),
                    ("QuotationSub_DisAmt","itemdisamt","numeric"),
                    ("QuotationSub_PurRate","purrate","numeric"),
                    ("QuotationSub_LandingCost","landingcost","numeric"),
                    ("QuotationSub_IncluesiveSales","inclusivesales","text"),
                    ("Quotation_Color","color","text"),
                    ("QuotationSub_CessPers","cesspers","numeric"),
                    ("QuotationSub_CessAmt","cessamt","numeric"),
                    ("QuotationSub_RemarksOne","remarks1","text"),
                    ("QuotationSub_RemarksTwo","remarks2","text"),
                    ("QuotationSub_PriceMenuId","pricemenuid","integer"),
                    ("QuotationSub_RMrp","rmrp","numeric"),
                    ("QuotationSub_WhRate","whrate","numeric"),
                    ("QuotationSub_SpRate1","sprate1","numeric"),
                    ("QuotationSub_SpRate2","sprate2","numeric"),
                    ("QuotationSub_SpRate3","sprate3","numeric"),
                    ("QuotationSub_SpRate4","sprate4","numeric"),
                    ("QuotationSub_SpRate5","sprate5","numeric"),
                    ("branchid","branchid","bigint"),
                    ("mainbranchid","mainbranchid","bigint"),
                },
                Constants = new Dictionary<string, object>
                {
                    {"orgpurrate",0 }
                },
                condition = "where branchid =" + nFromBranchId.ToString()
            },
              new TableMap
            {
                SqlTable = "AddTables",
                PgTable  = "restotable" ,
                Columns = new[]
                {
                    ("Table_Id","tempid","bigint"),
                    ("Table_Name","tablename","bigint"),
                    ("Table_Code","tablecode","bigint"),
                    ("Table_Status","tablestatus","bigint"),
                    ("TempInvoiceNo","tempinvoiceno","bigint"),
                    ("StaffId","staffid","numeric"),
                    ("branchid","branchid","bigint"),
                    ("mainbranchid","mainbranchid","bigint"),
                },
                Constants = new Dictionary<string, object>
                {
                    {"capacity",0 }
                },
                condition = "where branchid =" + nFromBranchId.ToString()
            },
               new TableMap
            {
                SqlTable = "TableDetails",
                PgTable  = "restotabledetails" ,
                Columns = new[]
                {
                    ("TableDetails_Name","tabledetailsname","bigint"),
                    ("TableDetails_Code","tabledetailscode","bigint"),
                    ("Table_Id","tableid","bigint"),
                    ("bActive","bactive","bigint"),
                    ("Allocated","allocated","bigint"),
                    ("BillNo","billno","bigint"),
                    ("StaffId","staffid","numeric"),
                    ("branchid","branchid","bigint"),
                    ("mainbranchid","mainbranchid","bigint"),
                },
                Constants = new Dictionary<string, object>
                {

                },
                condition = "where branchid =" + nFromBranchId.ToString()
            },
        new TableMap
        {
            SqlTable = "Doctor",
            PgTable  = "doctor",
            Columns = new[]
            {
                ("Doctor_Name",         "doctorname",         "text"),
                ("Doctor_Speciality",   "speciality",         "text"),
                ("Doctor_ConsultFrom",  "consultfrom",        "text"),
                ("Doctor_ConsultTo",    "consultto",          "text"),
                ("Doctor_OfficialAdd1", "add1",               "text"),
                ("Doctor_OfficialAdd2", "add2",               "text"),
                ("Doctor_OfficialAdd3", "add3",               "text"),
                ("Doctor_OfficialPhone","phone",              "text"),
                ("Doctor_Email",        "email",              "text"),
                ("Doctor_ResAdd1",      "resadd1",            "text"),
                ("Doctor_ResAdd2",      "resadd2",            "text"),
                ("Doctor_ResAdd3",      "resadd3",            "text"),
                ("Doctor_ResPhone",     "resphone",           "text"),
                ("Doctor_ResMobile",    "resmobile",          "text"),
                ("Doctor_ConsultFee",   "doctor_consultfee",  "numeric"),
                ("Department_Id",       "department_id",      "integer"),
                ("Doctor_UserName",     "username",           "text"),
                ("Doctor_Password",     "password",           "text"),
                ("Specialist_Id",       "specialistid",       "integer"),
                ("NoOf_Revisit",        "noofrevisit",        "numeric"),
                ("Revisit_Limit",       "revisitlimit",       "numeric"),
                ("branchid",            "branchid",           "numeric"),
                ("mainbranchid",        "mainbranchid",       "numeric"),
                ("Doctor_Id","tempid",  "bigint")
            },
            Constants = new Dictionary<string, object>
            {
                {"mobile","" }
            },
            condition = "where branchid =" + nFromBranchId.ToString()
        },
        new TableMap
        {
            SqlTable = "PrintImageSet",
            PgTable  = "printimageset",
            Columns = new[]
            {
                ("PrintImage_Name",         "printimagename",   "text"),
                ("PrintImage_From",         "printimagefrom",   "text"),
                ("PrintImage_PrintName",    "printfilename",    "text"),
                ("PrintImage_Position",     "imageposition",    "text"),
                ("branchid",                "branchid",         "bigint"),
                ("mainbranchid",            "mainbranchid",     "bigint"),
            },
            condition = "where branchid =" + nFromBranchId.ToString()
        },
        new TableMap
        {
            SqlTable = "PrintDisplaySettings",
            PgTable  = "printdisplaysettings",
            Columns = new[]
            {
                ("ColumnName",      "columnname",         "text"),
                ("ColumnValue",     "columnvalue",        "text"),
                ("Width",           "width",              "text"),
                ("bActive",         "bactive",            "text"),
                ("OrderNo",         "orderno",            "bigint"),
                ("PrintColumType",  "printcolumtype",     "bigint"),
                ("PrintName",       "printname",          "bigint"),
                ("RowNo",           "rowno",              "bigint"),
                ("branchid",        "branchid",           "bigint"),
                ("mainbranchid",    "mainbranchid",       "bigint"),
            },
            condition = "where branchid =" + nFromBranchId.ToString()
        },
        new TableMap
        {
            SqlTable = "ConversionMain",
            PgTable  = "stockconversionmain"+ nMainBranchId.ToString(),
            Columns = new[]
            {

                ("ConversionMain_BillNo","billno","bigint"),
                ("ConversionMain_BillDate","billdate","Date"),
                ("ConversionMain_ConvertType","converttype","text"),
                ("StaffId","staffid","bigint"),
                ("ConversionMain_CancelFlag","cancelflag","text"),
                ("branchid","branchid","bigint"),
                ("mainbranchid","mainbranchid","bigint"),
            },
            condition = "where branchid =" + nFromBranchId.ToString()
        },
        new TableMap
        {
            SqlTable = "StockConversion",
            PgTable  = "stockconversiondetails"+ nMainBranchId.ToString(),
            Columns = new[]
            {
                ("FromProductId",     "fromproductid",   "bigint"),
                ("FromPurRate",       "frompurrate",     "numeric"),
                ("FromSelRate",       "fromselrate",     "numeric"),
                ("FromMrp",           "frommrp",         "numeric"),
                ("FromWhRate",        "fromwhrate",      "numeric"),
                ("FromSpRate1",       "fromsprate1",     "numeric"),
                ("FromSpRate2",       "fromsprate2",     "numeric"),
                ("FromSpRate3",       "fromsprate3",     "numeric"),
                ("FromSpRate4",       "fromsprate4",     "numeric"),
                ("FromSpRate5",       "fromsprate5",     "numeric"),
                ("FromLandingCost",   "fromlandingcost", "numeric"),
                ("FromBatchNo",       "frombatchno",     "bigint"),
                ("FromStockQty",      "fromstockqty",    "numeric"),
                ("FromEnterQty",      "fromenterqty",    "numeric"),
                ("ToProductId",       "toproductid",     "bigint"),
                ("ToPurRate",         "topurrate",       "numeric"),
                ("ToSelRate",         "toselrate",       "numeric"),
                ("ToMrp",             "tomrp",           "numeric"),
                ("ToWhRate",          "towhrate",        "numeric"),
                ("ToSpRate1",         "tosprate1",       "numeric"),
                ("ToSpRate2",         "tosprate2",       "numeric"),
                ("ToSpRate3",         "tosprate3",       "numeric"),
                ("ToSpRate4",         "tosprate4",       "numeric"),
                ("ToSpRate5",         "tosprate5",       "numeric"),
                ("ToLandingCost",     "tolandingcost",   "numeric"),
                ("ToBatchNo",         "tobatchno",       "bigint"),
                ("ToEnterQty",        "toenterqty",      "numeric"),
                ("EnterDate",         "enterdate",       "date"),
                ("CancelFlag",        "cancelflag",      "text"),
                ("StaffId",           "staffid",         "bigint"),
                ("ToProdWeight",      "toprodweight",    "numeric"),
                ("ToBatch",           "tobatch",         "text"),
                ("ToExpDate",         "toexpdate",       "date"),
                ("ToSchemePeriod",    "toschemeperiod",  "date"),
                ("ToTaxPers",         "totaxpers",       "numeric"),
                ("branchid",          "branchid",        "bigint"),
                ("mainbranchid",      "mainbranchid",    "bigint")
            },            
            condition = "where FromBranchId =" + nFromBranchId.ToString()
        },
        new TableMap
        {
            SqlTable = "ExpenseEntryMain",
            PgTable  = "expenseentrymain"+ nMainBranchId.ToString(),
            Columns = new[]
            {
                ("ExpenseEntryMain_VoucherNo","voucherno","bigint"),
                ("UniqueVoucherId","uniquevoucherid","bigint"),
                ("VType_SlNo","vprefixid","bigint"),
                ("ExpenseEntryMainDate","entrymaindate","date"),
                ("ExpenseEntryMain_DisPers","dispers","numeric"),
                ("ExpenseEntryMain_DisAmt","disamt","numeric"),
                ("AcId","acid","bigint"),
                ("ExpenseEntryMain_PayTerms","payterms","text"),
                ("ExpenseEntryMain_OtherCharge","othercharge","numeric"),
                ("ExpenseEntryMain_DTotal","dtotal","numeric"),
                ("ExpenseEntryMain_ATotal","atotal","numeric"),
                ("ExpenseEntryMain_ROF","rof","numeric"),
                ("ExpenseEntryMain_Total","total","numeric"),
                ("ExpenseEntryMain_Cancel","billcancel","text"),
                ("ExpenseEntryMain_ClaimNo","claimno","text"),
                ("StaffId","staffid","bigint"),
                ("ExpenseEntryMain_CancelDate","canceldate","date"),
                ("ExpenseEntryMain_CancelStaffId","cancelstaffid","bigint"),
                ("Field1","purtype","text"),
                ("SalesExeId","salesexeid","bigint"),
                ("ExpenseEntryMain_Type","entrytype","text"),
                ("ExpenseEnterMain_EnterDate","enterdate","date"),
                ("ExpenseEnterMain_InvNo","invno","text"),
                ("ExpenseEnterMain_InvDate","invdate","date"),
                ("ExpenseEntryMain_PostAcId","postacid","bigint"),
                ("ExpenseEntryMain_ChequeNo", "chequeno","text"),
                ("ExpenseEntryMain_ChequeDate","chequedate","date"),
                ("ExpenseEntryMain_TransType","transtype", "text"),
                ("ExpenseEntryMain_GstNo","gstno","text"),
                ("ExpenseEntryMain_CashId","cashid","bigint"),
                ("branchid","branchid","bigint"),
                ("mainbranchid","mainbranchid","bigint"),
                ("ExpenseEntryMain_Id","tempid",  "bigint")
            },
            Constants = new Dictionary<string, object>
            {
                {"inclusive","" }
            },
            condition = "where BranchId =" + nFromBranchId.ToString()
        },

        new TableMap
        {
            SqlTable = "ExpenseEntryDetails",
            PgTable  = "expenseentrydetails"+ nMainBranchId.ToString(),
            Columns = new[]
            {
                ("ExpenseEntryMain_Id","expensemainid","bigint"),
                ("ExpiryRetSlNo","expiryretslno","bigint"),
                ("ExpenseEntryDetails_Batch","batch","text"),
                ("ExpenseEntryDetails_ExpDate","expdate","date"),
                ("ExpenseEntryDetails_SelRate","selrate","numeric"),
                ("ExpenseEntryDetails_Mrp","mrp","numeric"),
                ("ExpenseEntryDetails_PurRate","purrate","numeric"),
                ("ExpenseEntryDetails_Qty","qty","numeric"),
                ("ExpenseEntryDetails_TaxPers","taxpers","numeric"),
                ("ExpenseEntryDetails_TaxAmt","taxamt","numeric"),
                ("ExpenseEntryDetails_PdodDis","itemdispers","numeric"),
                ("ExpenseEntryDetails_AmtBeforeTax","amtbeforetax","numeric"),
                ("ExpenseEntryDetails_Amount","amount","numeric"),
                ("ProductId","productid","bigint"),
                ("EntryHeadId","entryheadid","bigint"),
                ("ExpenseEntryDetails_ActualRate","actualrate","numeric"),
                ("TaxId","taxid","bigint"),
                ("ExpenseEntryDetails_ProdDisAmt","itemdisamt","numeric"),
                ("ExpenseEntryDetails_OriginalRate","originalrate","numeric"),
                ("ExpenseEntryDetails_SGSTTaxPers","sgsttaxpers","numeric"),
                ("ExpenseEntryDetails_SGSTTaxAmount","sgsttaxamount","numeric"),
                ("ExpenseEntryDetails_SGSTAmount","sgstamount","numeric"),
                ("ExpenseEntryDetails_CGSTTaxPers","cgsttaxpers","numeric"),
                ("ExpenseEntryDetails_CGSTTaxAmount","cgsttaxamount","numeric"),
                ("ExpenseEntryDetails_CGSTAmount","cgstamount","numeric"),
                ("ExpenseEntryDetails_IGSTTaxPers", "igsttaxpers","numeric"),
                ("ExpenseEntryDetails_IGSTTaxAmount","igsttaxamount","numeric"),
                ("ExpenseEntryDetails_IGSTAmount","igstamount", "numeric"),
                ("HsnId","hsnid","bigint"),
                ("HsnCode","hsncode","text"),
                ("branchid","branchid","bigint"),
                ("mainbranchid","mainbranchid","bigint")
            },
            Constants = new Dictionary<string, object>
            {

            },
            condition = "where BranchId =" + nFromBranchId.ToString()
        },
        new TableMap
        {
            SqlTable = "ServiceMain",
            PgTable  = "servicemain" + nMainBranchId.ToString(),
            Columns = new[]
            {
               
                ("ServiceMain_BillSerId","billserid","bigint"),
                ("ServiceMain_SlNo","servicemainno","bigint"),
                ("ServiceMain_Id","uniquebillno","bigint"),
                ("ServiceMain_BillDate","servicemaindate","date"),
                ("ServiceMain_DisPers","dispers","numeric"),
                ("ServiceMain_DisAmt","disamt","numeric"),
                ("AcId","acid","bigint"),
                ("ServiceMain_CustName","custname","text"),
                ("ServiceMain_DoctorId","doctid","bigint"),
                ("ServiceMain_DocName","doctname","text"),
                ("SalesExeId","salesexeid","bigint"),
                ("ServiceMain_PayTerms","payterms","text"),
                ("ServiceMain_CardNo","cardno","text"),
                ("ServiceMain_CardExpDate","cardexpdate","date"),
                ("ServiceMain_CardName","cardname","text"),
                ("ServiceMain_ShipDate","dispdate","date"),
                ("ServiceMain_BankCharge","bankcharge","numeric"),
                ("ServiceMain_OtherCharge","othercharge","numeric"),
                ("ServiceMain_DTotal","dtotal","numeric"),
                ("ServiceMain_ATotal","atotal","numeric"),
                ("ServiceMain_ROF","rof","numeric"),
                ("ServiceMain_Total","total","numeric"),
                ("ServiceMain_Cancel","servicecancel","text"),
                ("ServiceMain_PurType","servicepurtype","text"),
                ("StaffId","staffid","bigint"),
                ("AgentId","agentid","bigint"),
                ("BillCancelDate","billcanceldate","date"),
                ("ServiceMain_PhoneNo","phoneno","text"),
                ("VType_SlNo","vprefixid","bigint"),
                ("Trans_VoucherNo","voucherno","bigint"),
                ("UniqueVoucherId","uniquevoucherid","bigint"),
                ("ServiceMain_BankId","bankid","bigint"),
                ("ServiceMain_VechileNo","vechileno","text"),
                ("ServiceMain_GstNo","gstinno","text"),
                ("ServiceMain_CreditCardAmt","creditcardamt","numeric"),
                ("ServiceMain_AddCess","addcessflag","boolean"),
                ("ServiceMain_ShippingName","shippingname","text"),
                ("ServiceMain_ShippingAddr1","shippingaddr1","text"),
                ("ServiceMain_ShippingAddr2","shippingaddr2","text"),
                ("ServiceMain_Shippinggstno","shippinggstno","text"),
                ("ServiceMain_ShippingTransporter","shippingtransporter","text"),
                ("ServiceMain_ShippingState","shippingstate","text"),
                ("ServiceMain_ShippingStateCode","shippingstatecode","text"),
                ("ServiceMain_ModelNo","modelno","text"),
                ("ServiceMain_BrandName","brandname","text"),
                ("ServiceMain_SerialNo","serialno","text"),
                ("ServiceMain_PostBankId","postbankid","bigint"),
                ("ServiceMain_Remarks","remarks","text"),
                ("JobCard_Id","jobcardid","bigint"),
                ("branchid","branchid","bigint"),
                ("mainbranchid","mainbranchid","bigint"),
            },
            Constants = new Dictionary<string, object>
            {
                // PG-only columns with no SQL Server source.
                // Every one already has a DDL default (0 / '' / false / CURRENT_DATE),
                // so these are optional — listed for completeness. Remove any you
                // want to keep at its DDL default.
                {"servicemaintime",""},
                {"orderfrom",""},
                {"duedate", DateTime.Now.ToString("yyyy-MM-dd")},        // CURRENT_DATE default
                {"orderno",""},
                {"repamt",0},
                {"cstpers",0},
                {"cstamt",0},
                {"discname",""},
                {"delflag",""},
                {"crditnotenos",""},
                {"expcrnotenos",""},
                {"adddis",0},
                {"minusdis",0},
                {"remarks1",""},
                {"inclusivesales",""},
                {"retvalue",0},
                {"pointsalevalue",0},
                {"pointamount",0},
                {"noofpoints",0},
                {"crvoucherno",0},
                {"cruniquevoucherid",0},
                {"paidamount",0},
                {"orderdate", DateTime.Now.ToString("yyyy-MM-dd")},      // CURRENT_DATE default
                {"chellano",""},
                {"agentpers",0},
                {"agentratetype",0},
                {"agentsalesvaue",0},
                {"agentmarginamt",0},
                {"godownid",0},
                {"agentamt",0},
                {"agenttdsamt",0},
                {"cardservicepers",0},
                {"cardserviceamt",0},
                {"otherdisplayname",""},
                {"tenderreceiveamt",0},
                {"tcspers",0},
                {"tcsamt",0},
                {"transportid",""},
                {"transportname",""},
                {"transporter",""},
                {"cashamt",0},
                {"temporderno",0},
                {"currencyid",0},
                {"currencyrate",0},
                {"sourcefrom",""},
                {"address1",""},
                {"smsno",""},
                {"postage",0},
                {"freight",0},
                {"advanceamt",0},
                {"supplytypecode",""},
                {"pricemenuid",0},
                {"cancelstaffid",0},
            },
            condition = "where branchid =" + nFromBranchId.ToString()
        },
        new TableMap
        {
             SqlTable = "ServiceSubDetails",
            PgTable  = "servicesubdetails" + nMainBranchId.ToString(),
            Columns = new[]
            {               
                ("Service_SlNo","servicemainno","bigint"),
                ("Service_BillDate","servicdate","date"),
                ("ServiceSub_Batch","batch","text"),
                ("ServiceSub_SelRate","selrate","numeric"),
                ("ServiceSub_Qty","qty","numeric"),
                ("ServiceSub_FreeQty","freqty","numeric"),
                ("ServiceSub_TaxPers","taxpers","numeric"),
                ("ServiceSub_TaxAmt","taxamt","numeric"),
                ("ServiceSub_PdodDis","itemdispers","numeric"),
                ("ServiceSub_ProdDisAmt","itemdisamt","numeric"),
                ("ServiceSub_Amount","amount","numeric"),
                ("Service_Id","productid","bigint"),
                ("TaxId","taxid","integer"),
                ("ServiceSub_SchmPers","schmpers","numeric"),
                ("ServiceSub_SchmAmt","schmamt","numeric"),
                ("UniqueBillNo","uniquebillno","bigint"),
                ("ServiceSub_OriginalRate","originalrate","numeric"),
                ("ServiceSub_ActualRate","actualrate","numeric"),
                ("ServiceSub_AmtBeforeTax","amoutbefortax","numeric"),
                ("ServiceSub_AmtBeforeDis","amountbeforedis","numeric"),
                ("Store_BatchSlNo","batchslno","bigint"),
                ("ServiceSub_ItemFrom","prodfrom","text"),
                ("ServiceSub_SGSTTaxPers","sgsttaxpers","numeric"),
                ("ServiceSub_SGSTTaxAmount","sgsttaxamount","numeric"),
                ("ServiceSub_SGSTAmount","sgstamount","numeric"),
                ("ServiceSub_CGSTTaxPers","cgsttaxpers","numeric"),
                ("ServiceSub_CGSTTaxAmount","cgsttaxamount","numeric"),
                ("ServiceSub_CGSTAmount","cgstamount","numeric"),
                ("ServiceSub_IGSTTaxPers","igsttaxpers","numeric"),
                ("ServiceSub_IGSTTaxAmount","igsttaxamount","numeric"),
                ("ServiceSub_IGSTAmount","igstamount","numeric"),
                ("ServiceSub_Specification","specification","text"),
                ("ServiceSub_CessPers","cesspers","numeric"),
                ("ServiceSub_CessAmt","cessamt","numeric"),
                ("ServiceSub_ChargeAmt","chargeamt","numeric"),
                ("ServiceSub_TicketDate","ticketdate","date"),
                ("ServiceSub_SerialNo","serialno","text"),
                ("ServiceSub_Sectors","sectors","text"),
                ("branchid","branchid","bigint"),
                ("mainbranchid","mainbranchid","bigint"),
            },
            Constants = new Dictionary<string, object>
            {
                // PG-only columns with no SQL Server source.
                // All have DDL defaults (0 / '' / CURRENT_DATE), so optional.
               // {"servicemainno",0},
                {"expdate",DateTime.Now.ToString("yyyy-MM-dd")},        // CURRENT_DATE default
                {"whrate",0},
                {"mrp",0},
                {"qtytype",""},
                {"advfre",0},
                {"rqty",0},
                {"rfreqty",0},
                {"lqty",0},
                {"loosefree",0},
                {"totqty",0},
                {"batchslno1",0},
                {"repl",""},
                {"flgspecialrate",""},
                {"dcslno",0},
                {"color",""},
                {"unit",""},
                {"itemweight",""},
                {"prodpack",0},
                {"pack",0},
                {"perrate",0},
                {"prodtype",""},
                {"adddispers",0},
                {"pricemenuid",1},
                {"inclusivesales",""},
                {"salesmanid",0},
                {"agentprice",0},
                {"purrate",0},
                {"orgpurrate",0},
                {"salesmanprice",0},
                {"rmrp",0},
                {"sprate1",0},
                {"sprate2",0},
                {"sprate3",0},
                {"sprate4",0},
                {"sprate5",0},
                {"pcsselrate",0},
                {"pcsmrp",0},
                {"pcswhrate",0},
                {"pcssprate1",0},
                {"pcssprate2",0},
                {"pcssprate3",0},
                {"pcssprate4",0},
                {"pcssprate5",0},
                {"godownid",0},
                {"neethidispers",0},
                {"packageid",0},
                {"packageuniqueno",0},
                {"extracesspers",0},
                {"extracessamt",0},
                {"specialorgrate",0},
                {"extraschemeamt",0},
                {"addrateperunit",0},
                {"addrateunitamt",0},
                {"packpurrate",0},
                {"packorgpurrate",0},
            },
            condition = "where branchid =" + nFromBranchId.ToString()
        },
        new TableMap
        {
            SqlTable = "ServiceBillSeries",
            PgTable  = "billseries",
            Columns = new[]
            {
                // ("billserid","","bigint"),
                 ("ServiceBillSerDescription","billserdescription","text"),
                 ("ServiceBillSerPrefix","billserprefix","text"),
                 ("PurBillSerStartNo","billserstartno","bigint"),
                 ("ServiceBillSerCurrentBillNo","billsercurrentbillno","bigint"),
                 ("ServiceBillSeriesOrderNo","billseriesorderno","bigint"),
                 ("ServiceBillSerCustomerType","billseriescustomerlist","text"),
                 ("ServiceBillSeriesStartDate","billseriesstartdate", "date"),
                 ("ServiceBillSeriesEndDate","billseriesenddate","date"),
                 ("PrintFileName","printfilename","text"),
                 ("PrintFilePreview","printfilepreview","text"),
                 ("Active","active","boolean"),                 
                 ("branchid","branchid", "integer"),
                 ("mainbranchid","mainbranchid","integer"),
                 ("ServiceBillSerId","tempid","bigint"),

            },
            Constants = new Dictionary<string, object>
            {
                 {"printfilemobile","" },
                 {"printfilenameone","" },
                 {"pricemenuid", "1" },
                 {"printtype", "Laser" },
                 {"billsersource", "SERVICE BILL" },
                 {"bdataclear","False"},
                 {"bbilled","False"},
                 {"bdownloadprint","False"},
                 {"billseriesaddcess","Flase" },
                 {"billtype",""},
                 {"billserpayterms",""},
                 {"billwithtin",""},
                 {"billhide",""},
                 {"billsersuffix", ""},
                 {"billserminussymbol",""},
                 {"billserbillnoformat",""},
                 {"billserbillinclusive","Yes"},
                 {"billsertaxadd","Yes"},
            },
            condition="where   branchid ="+nFromBranchId.ToString()
        },

            new TableMap
            {
                SqlTable = "ExpiryReceive",
                PgTable  = "expiryreceivemain"+nMainBranchId.ToString(),
                Columns = new[]
                {
                    ("ExpiryReceive_SlNo","expiryreceiveno","bigint"),
                    ("ExpiryReceive_Date","expiryreceivedate","date"),
                    ("ExpiryReceive_DisPers","dispers","numeric"),
                    ("ExpiryReceive_DisAmt","disamt","numeric"),
                    ("AcId","acid","bigint"),
                    ("Expiry_ROF","rof","numeric"),
                    ("ExpiryReceive_Total","total","numeric"),
                    ("ExpiryReceive_Cancel","expirycancel","text"),
                    ("StaffId","staffid","bigint"),
                    ("SalesExeId","salesexeid","bigint"),
                    ("ExpiryReceive_Type","issuepurtype","text"),
                    ("ExpiryReceive_Field2","inclusivesales","text"),
                    ("branchid","branchid","integer"),
                    ("mainranchId","mainbranchid","bigint"),
                },
                Constants = new Dictionary<string, object>
                {
                    // numeric(18,3) / numeric / bigint / integer  -> 0
                    {"billserid","0"},
                    {"doctid","1"},
                    {"paytermsid","0"},
                    {"bankcharge","0"},
                    {"postage","0"},
                    {"cramt","0"},
                    {"dbamt","0"},
                    {"freight","0"},
                    {"othercharge","0"},
                    {"expiryamt","0"},
                    {"expiryid","0"},
                    {"repamt","0"},
                    {"dtotal","0"},
                    {"atotal","0"},
                    {"cstpers","0"},
                    {"cstamt","0"},
                    {"agentid","0"},
                    {"cancelstaffid","0"},
                    {"pricemenuid","0"},
                    {"retvalue","0"},
                    {"bankid","0"},
                    {"vprefixid","0"},
                    {"voucherno","0"},
                    {"uniquevoucherid","0"},
                    {"crvoucherno","0"},
                    {"cruniquevoucherid","0"},
                    {"godownid","0"},
                    {"agentamt","0"},
                    {"agenttdsamt","0"},
                    {"creditcardamt","0"},
                    {"cardservicepers","0"},
                    {"cardserviceamt","0"},
                    {"tcspers","0"},
                    {"tcsamt","0"},
                    {"cashamt","0"},
                    {"currencyid","0"},
                    {"currencyrate","0"},
       

                    // text -> ''
                    {"custname",""},
                    {"doctname",""},
                    {"payterms"," BILL"},
                    {"cardno",""},
                    {"cardname",""},
                    {"transporter",""},
                    {"orderno",""},
                    {"discname",""},
                    {"delflag",""},
                    {"issuetime",""},
                    {"orderfrom",""},
                    {"chellano",""},
                    {"remarks",""},
                    {"remarks1",""},
                    {"smsno",""},
                    {"phoneno",""},
                    {"address1",""},
                    {"vechileno",""},
                    {"gstinno",""},
                    {"otherdisplayname",""},
                    {"transportid",""},
                    {"transportname",""},
                    {"sourcefrom",""},

                    // date -> CURRENT_DATE
                    {"cardexpdate",DateTime.Now.ToString("yyyy-MM-dd")},
                    {"duedate",DateTime.Now.ToString("yyyy-MM-dd")},

                    // boolean -> false
                    {"addcessflag",false},
                },
                condition="where branchid ="+nFromBranchId.ToString()
            },
            new TableMap
            {
                SqlTable = "ExpiryReceiveDetails",
                PgTable  = "expiryreceivedetails"+nMainBranchId.ToString(),
                Columns = new[]
                {
                    ("ExpiryReceive_Id","expiryreceiveid","bigint"),
                    ("ExpiryReceive_SlNo","expiryreceiveno","bigint"),
                    ("ExpiryReceiveSub_Batch","batch","text"),
                    ("ExpiryReceiveSub_ExpDate","expdate","date"),
                    ("ExpiryReceiveSub_SelRate","selrate","numeric"),
                    ("ExpiryReceiveSub_Mrp","mrp","numeric"),
                    ("ExpiryReceiveSub_PurRate","purrate","numeric"),
                    ("ExpiryReceiveSub_Qty","qty","numeric"),
                    ("ExpiryReceiveSub_TaxPers","taxpers","numeric"),
                    ("ExpiryReceiveSub_TaxAmt","taxamt","numeric"),
                    ("ExpiryReceiveSub_PdodDis","itemdispers","numeric"),
                    ("ExpiryReceiveSub_TaxOn","qtytype","text"),
                    ("ExpiryReceiveSub_Amount","amount","numeric"),
                    ("ProductId","productid","bigint"),
                    ("Store_BatchSlNo","batchslno","bigint"),
                    ("ExpiryReceiveSub_ActualRate","actualrate","numeric"),
                    ("TaxId","taxid","integer"),
                    ("ExpiryReceiveSub_ProdDisAmt","itemdisamt","numeric"),
                    ("ExpiryReceiveSub_LQty","lqty","numeric"),
                    ("ExpiryReceiveSub_Pack","pack","integer"),
                    ("ExpiryReceiveSub_PerRate","perrate","numeric"),
                    ("ExpiryReceiveSub_LooseFree","loosefree","numeric"),
                    ("ExpiryReceiveSub_OriginalRate","originalrate","numeric"),
                    ("ExpiryReceiveSub_AmtBeforeTax","amoutbefortax","numeric"),
                    ("ExpiryReceiveSub_PurType","prodtype","text"),
                    ("ExpiryReceiveSub_DebitFlag","debitflag","text"),
                    ("ExpiryReceiveSub_SGSTTaxPers","sgsttaxpers","numeric"),
                    ("ExpiryReceiveSub_SGSTTaxAmount","sgsttaxamount","numeric"),
                    ("ExpiryReceiveSub_SGSTAmount","sgstamount","numeric"),
                    ("ExpiryReceiveSub_CGSTTaxPers","cgsttaxpers","numeric"),
                    ("ExpiryReceiveSub_CGSTTaxAmount","cgsttaxamount","numeric"),
                    ("ExpiryReceiveSub_CGSTAmount","cgstamount","numeric"),
                    ("ExpiryReceiveSub_IGSTTaxPers","igsttaxpers","numeric"),
                    ("ExpiryReceiveSub_IGSTTaxAmount","igsttaxamount","numeric"),
                    ("ExpiryReceiveSub_IGSTAmount","igstamount","numeric"),
                    ("ExpiryReceiveSub_DebitQty","debitqty","numeric"),
                    ("ExpiryReceiveSub_FreeQty","freqty","numeric"),
                    ("ExpiryReceiveSub_InclusiveSales","inclusivesales","text"),
                    ("ExpiryReceiveSub_CessPers","cesspers","numeric"),
                    ("ExpiryReceiveSub_CessAmt","cessamt","numeric"),
                    ("ExpiryReceiveSub_ExtraCessPers","extracesspers","numeric"),
                    ("ExpiryReceiveSub_ExtraCessAmt","extracessamt","numeric"),
                    ("BillSerId","billserid","bigint"),
                    ("Issue_SlNo","issueno","bigint"),
                    ("Issue_BillDate","issuedate","date"),
                    ("Receipt_SlNo","receiptno","bigint"),
                    ("Receipt_InvoNo","invno","text"),
                    ("Receipt_InvoDate","invdate","text"),
                    ("Receipt_Date","receiptdate","date"),
                    ("UniqueBillNo","uniquebillno","bigint"),
                    ("ExpiryReceiveSub_DebitNoteConvertFlag","expiryconvflag","text"),
                    ("ExpiryReceiveSub_DebitNoteConvertQty","expiryconvqty","numeric"),
                    ("branchid","branchid","bigint"),
                    ("mainbranchId","mainbranchid","bigint"),
                },
                Constants = new Dictionary<string, object>
                {
                    // numeric / bigint / integer -> 0
                    {"whrate","0"},
                    {"advfre","0"},
                    {"rqty","0"},
                    {"rfreqty","0"},
                    {"totqty","0"},
                    {"oldamount","0"},
                    {"batchslno1","0"},
                    {"dcslno","0"},
                    {"schmpers","0"},
                    {"schmamt","0"},
                    {"prodpack","0"},
                    {"amountbeforedis","0"},
                    {"adddispers","0"},
                    {"pricemenuid","0"},
                    {"salesmanid","0"},
                    {"agentprice","0"},
                    {"orgpurrate","0"},
                    {"salesmanprice","0"},
                    {"rmrp","0"},
                    {"sprate1","0"},
                    {"sprate2","0"},
                    {"sprate3","0"},
                    {"sprate4","0"},
                    {"sprate5","0"},
                    {"packselrate","0"},
                    {"packmrp","0"},
                    {"packwhrate","0"},
                    {"packsprate1","0"},
                    {"packsprate2","0"},
                    {"packsprate3","0"},
                    {"packsprate4","0"},
                    {"packsprate5","0"},
                    {"godownid","0"},
                    {"neethidispers","0"},
                    {"packageid","0"},
                    {"packageuniqueno","0"},
                    {"specialorgrate","0"},
                    {"extraschemeamt","0"},
                    {"addrateperunit","0"},
                    {"addrateunitamt","0"},
                    {"receiptid","0"},
                    

                    // text -> ''
                    {"repl",""},
                    {"flgspecialrate",""},
                    {"color",""},
                    {"unit",""},
                    {"itemweight",""},
                    {"prodfrom",""},
                },
                condition="where branchid ="+nFromBranchId.ToString()
            },
            new TableMap
            {
                SqlTable = "AccountHeadSub",
                PgTable  = "accountheadsub",
                Columns = new[]
                {
                    
                    ("OP_No","opno","bigint"),
                    ("Doctor_Id","doctorid","bigint"),
                    ("AccountHeadSub_fee","subfee","numeric"),
                    ("AccountHeadSub_Consulfee","consulfee","numeric"),
                    ("AccountHeadSub_otherfee","otherfee","numeric"),
                    ("AccountHeadSub_Time","time","text"),
                    ("AccountHeadSub_RegNo","regno","bigint"),
                    ("AccountHeadSub_Total","total","numeric"),
                    ("AccountHeadSub_PR","pr","text"),
                    ("AccountHeadSub_BP","bp","text"),
                    ("AccountHeadSub_Wt","wt","text"),
                    ("AccountHeadSub_Ht","ht","text"),
                    ("AccountHeadSub_sex","sex","text"),
                    ("AccoutHead_Id","acid","bigint"),
                    ("AccountHeadSub_Age","age","bigint"),
                    ("AccountHeadSub_VisitNo","visitno","bigint"),
                    ("AccountHeadSub_RegDate","regdate","date"),
                    ("AccountHeadSub_PayTermsId","paytermsid","bigint"),
                    ("AccountHeadSub_BloodGroupId","bloodgroupid","bigint"),
                    ("AccountHeadSub_CareOfRelationship","careofrelationship","text"),
                    ("AccountHeadSub_CareOfName","careofname","text"),
                    ("AccountHeadSub_CareOfPhone","careofphone","text"),
                    ("AccountHeadSub_Month","month","bigint"),
                    ("AccountHeadSub_GpayAmt","gpayamt","numeric"),
                    ("AccountHeadSub_CashAmt","cashamt","numeric"),
                    ("AccountHeadSub_BankCardNo","bankcardno","text"),
                    ("AccountHeadSub_BankCardName","bankcardname","text"),
                    ("AccountHeadSub_BankId","bankid","bigint"),
                    ("AccountHeadSub_RefDoctor","refdoctor","text"),
                    ("AccountHeadSub_RefHospital","refhospital","text"),
                    ("AccountHeadSub_Title","title","text"),                   
                    ("mainbranchid","mainbranchid","bigint"),
                    ("AccountHeadSub_Id","tempid","bigint")
                },
                Constants = new Dictionary<string, object>
                {
                    { "branchid",nBranchId },
                },
                condition="where AccoutHead_Id in (select ac_id from  accounthead where  branchid ="+nFromBranchId.ToString()+")"
            },
            new TableMap
            {
                SqlTable = "Bloodgroup",
                PgTable  = "bloodgroup",
                Columns = new[]
                {
                    ("BloodgroupId","tempid","bigint"),
                    ("BloodgroupName","bloodgroupname","text"),
                    ("branchid","branchid","bigint"),
                    ("mainbranchid","mainbranchid","bigint")
                },
                Constants = new Dictionary<string, object>
                {
                    
                },
                condition="where branchid ="+nFromBranchId.ToString()
            },
            new TableMap
            {
                SqlTable = "Department",
                PgTable  = "department",
                Columns = new[]
                {
                    ("Department_Id","tempid","bigint"),
                    ("Department_Name","dptname","text"),
                    ("Description_Name","descname","text"),
                    ("Department_Head","dpthead","text"),
                    ("branchid","branchid","bigint"),
                    ("mainbranchid","mainbranchid","bigint")
                },
                Constants = new Dictionary<string, object>
                {
                   
                },
                condition="where branchid ="+nFromBranchId.ToString()
            },
            new TableMap
            {
                SqlTable = "Diagnosis",
                PgTable  = "diagnosis",
                Columns = new[]
                {
                    
                    ("DiagnosisName","diagnosisname","text"),
                    ("branchid","branchid","bigint"),
                    ("mainbranchid", "mainbranchid","bigint"),
                    ("DiagnosisId","tempid","bigint")
                },
                Constants = new Dictionary<string, object>
                {
                    
                },
                condition="where branchid ="+nFromBranchId.ToString()
            },
            new TableMap
            {
                SqlTable = "Diseases",
                PgTable  = "symptoms",
                Columns = new[]
                {
                    ("DiseasesId","tempid","bigint"),
                    ("DiseasesName","symptomsname","text"),
                    ("branchid","branchid","bigint"),
                    ("mainbranchid", "mainbranchid","bigint"),

                },
                Constants = new Dictionary<string, object>
                {
                    
                },
                condition="where branchid ="+nFromBranchId.ToString()
            },
            new TableMap
            {
                SqlTable = "DiseaseSub",
                PgTable  = "diseasesub",
                Columns = new[]
                {
                    
                    ("DiseasesId","diseasesid","bigint"),
                    ("DiagnosisId","diagnosisid","bigint"),
                },
                Constants = new Dictionary<string, object>
                {
                },
                condition="where DiseasesId in (select DiseasesId from  Diseases where  branchid ="+nFromBranchId.ToString()+")"
            },
            new TableMap
            {
                SqlTable = "Dosages",
                PgTable  = "dosage",
                Columns = new[]
                {
                    
                    ("DosageName","dosagename","text"),
                    ("branchid","branchid","bigint"),
                    ("mainbranchid","mainbranchid","bigint")
                },
                Constants = new Dictionary<string, object>
                {
                    
                },
                condition="where branchid ="+nFromBranchId.ToString()
            },
            new TableMap
            {
                SqlTable = "Hospital",
                PgTable  = "hospital",
                Columns = new[]
                {
                    
                    ("Hos_Name","hosname","text"),
                    ("Hos_Addr1","hosaddr1","text"),
                    ("Hos_Addr2","hosaddr2","text"),
                    ("Hos_Addr3","hosaddr3","text"),
                    ("Hos_Phone","hosphone","text"),
                    ("Hos_Mobile","hosmobile","text"),
                    ("branchid","branchid","bigint"),
                    ("mainbranchid","mainbranchid","bigint"),
                    ("Hos_Id","tempid","bigint"),
                },
                Constants = new Dictionary<string, object>
                {
                    
                },
                condition="where branchid ="+nFromBranchId.ToString()
            },
            new TableMap
            {
                SqlTable = "HosProcedure",
                PgTable  = "hosprocedure",
                Columns = new[]
                {
                
                    ("HosProcedureName","hosprocedurename","text"),
                    ("HosProcedureCharge","hosprocedurecharge","numeric"),
                    ("branchid","branchid","bigint"),
                    ("mainbranchid","mainbranchid","bigint"),
                },
                Constants = new Dictionary<string, object>
                {
               
                },
                condition="where branchid ="+nFromBranchId.ToString()
            },
            new TableMap
            {
                SqlTable = "LabBill",
                PgTable  = "labbill",
                Columns = new[]
                {
                    ("LabBill_Id","tempid","bigint"),
                    ("Reg_Id","regid","bigint"),
                    ("OPno","opno","bigint"),
                    ("LabBill_BillNo","billno","bigint"),
                    ("LabBill_Type","billtype","text"),
                    ("LabBill_PayTerms","payterms","text"),
                    ("LabBill_RateType","ratetype","text"),
                    ("LabBill_DisPers","billdispers","numeric"),
                    ("LabBill_DisAmt","billdisamt","numeric"),
                    ("LabBill_Total","billtotal","numeric"),
                    ("LabBill_date","billdate","date"),
                    ("RevisitId","revisitid","bigint"),
                    ("LabBill_Flag","billflag","text"),
                    ("Hospital_Id","hospitalid","text"),
                    ("Doctor_Id","doctorid","bigint"),
                    ("LabBill_Cancel","billcancel","text"),
                    ("StaffId","staffid","bigint"),
                    ("branchid","branchid","bigint"),
                    ("LabBill_NetAmt","billnetamt","numeric"),
                    ("IPno","ipno","bigint"),
                    ("LabBill_Status","billstatus","text"),
                    ("AcId","acid","bigint"),
                    ("LabBill_PayTermsId","paytermsid","bigint"),
                    ("LabillTime","billtime","text"),
                    ("LabBill_CustomerName","customername","text"),
                    ("LabBill_Addr","addr","text"),
                    ("LabBill_PhoneNumber","phonenumber","text"),
                    ("InPatient","inpatient","boolean"),
                    ("IP_UniqueId","ipuniqueid","bigint"),
                    ("LabBill_Age","age","bigint"),
                    ("LabBill_Sex","sex","text"),
                    ("LabBill_GpayAmt","gpayamt","numeric"),
                    ("LabBill_CashAmt","cashamt","numeric"),
                    ("LabBIll_CardNo","cardno","text"),
                    ("LabBill_CardName","cardname","text"),
                    ("LabBill_AdvAmt","advamt","numeric"),
                    ("LabBill_BankId","bankid","bigint"),
                    ("LabBill_CashRetrnTot","cashretrntot","numeric"),
                    ("CashRtnPayterms_Id","cashrtnpaytermsid","bigint"),
                    ("LabBill_Remarks","billremarks","text"),
                    ("LabBill_RefName","refname","text"),
                    ("LabBill_RefHospital","refhospital","text"),
                    ("mainbranchid","mainbranchid","bigint"),
                },
                Constants = new Dictionary<string, object>
                {
                    
                },
                condition="where branchid ="+nFromBranchId.ToString()
            },
            new TableMap
            {
                SqlTable = "Specialist",
                PgTable  = "specialist",
                Columns = new[]
                {
                    ("Specialist_Id","tempid","bigint"),
                    ("Specialist_Name","splname","text"),
                    ("branchid","branchid","bigint"),
                    ("mainbranchid","mainbranchid","bigint")
                },
                Constants = new Dictionary<string, object>
                {
                    
                },
                condition="where branchid ="+nFromBranchId.ToString()
            },
            new TableMap
            {
                SqlTable = "ReVisiting",
                PgTable  = "revisiting",
                Columns = new[]
                {
                    ("Visit_id","tempid","bigint"),
                    ("Visit_Name","visitname","text"),
                    ("Visit_sex","visitsex","text"),
                    ("Visit_DOB","visitdob","date"),
                    ("Visit_Age","visitage","text"),
                    ("Visit_Phone","visitphone","text"),
                    ("Visit_Addr1","visitaddr1","text"),
                    ("Visit_OPNo","visitopno","bigint"),
                    ("Reg_Id","regid","bigint"),
                    ("Visit_DoctorId","visitdoctorid","bigint"),
                    ("visiting_Fee","visitingfee","numeric"),
                    ("Visit_ConsultFee","visitconsultfee","numeric"),
                    ("Visit_OtherFee","visitotherfee","numeric"),
                    ("Visit_Date","visitdate","date"),
                    ("flag","flag","boolean"),
                    ("Visit_TokenNo","visittokenno","text"),
                    ("Visit_StaffId","visitstaffid","bigint"),
                    ("Visit_SplFeeFlag","visitsplfeeflag","text"),
                    ("Visit_SplFee","visitsplfee","numeric"),
                    ("Visit_EnterDate","visitenterdate","date"),
                    ("Visit_Total","visittotal","numeric"),
                    ("Visit_PR","visitpr","text"),
                    ("Visit_BP","visitbp","text"),
                    ("Visit_Wt","visitwt","text"),
                    ("Visit_Ht","visitht","text"),
                    ("branchid","branchid","bigint"),
                    ("mainbranchid","mainbranchid","bigint"),
                    ("AcId","acid","bigint"),
                    ("Visit_No","visitno","bigint"),
                    ("Visit_Flag","visitflag","text"),
                    ("PayTerms_Id","paytermsid","bigint"),
                    ("Visit_Cancel","visitcancel","boolean"),
                    ("Visit_Time","visittime","text"),
                    ("Specialist_Id","specialistid","bigint"),
                    ("Visit_AgeMonth","visitagemonth","text"),
                    ("Visit_CashAmt","visitcashamt","numeric"),
                    ("Visit_GpayAmt","visitgpayamt","numeric"),
                    ("Visit_BankCardNo","visitbankcardno","text"),
                    ("Visit_CardName","visitcardname","text"),
                    ("Visit_BankId","visitbankid","bigint"),
                },
                Constants = new Dictionary<string, object>
                {
                    
                },
                condition="where branchid ="+nFromBranchId.ToString()
            },
            new TableMap
            {
                SqlTable = "LabBillSub",
                PgTable  = "labbillsub",
                Columns = new[]
                {
                    ("LabBill_Id","labbillid","bigint"),
                    ("Test_Id","testid","bigint"),
                    ("DepartmentId","departmentid","text"),
                    ("Amount","amount","numeric"),
                    ("RefNO","refno","bigint"),
                    ("Flag","flag","text"),
                    ("branchid","branchid","bigint"),
                    ("LabBillSub_ReturnFlag","billsubreturnflag","boolean"),
                    ("mainbranchid","mainbranchid","bigint"),
                },
                Constants = new Dictionary<string, object>
                {

                },
                condition="where branchid ="+nFromBranchId.ToString()
            },
            new TableMap
            {
                SqlTable = "Test",
                PgTable  = "test",
                Columns = new[]
                {
                    ("Test_Name","testname","text"),
                    ("Test_Rate","testrate","bigint"),
                    ("Test_Srate","testsrate","bigint"),
                    ("Test_Vrate","testvrate","bigint"),
                    ("Test_Drate","testdrate","bigint"),
                    ("Department_Id","departmentid","bigint"),
                    ("TestImageLoc","imageloc","text"),
                    ("branchid","branchid","bigint"),
                    ("mainbranchid","mainbranchid","bigint"),
                    ("Test_Id","tempid","bigint")
                },
                Constants = new Dictionary<string, object>
                {

                },
                condition="where branchid ="+nFromBranchId.ToString()
            },

            new TableMap
            {
                SqlTable = "TestSub",
                PgTable  = "testsub",
                Columns = new[]
                {
                    ("Test_Id","testid","bigint"),
                    ("Test_subdivision","subdivision","text"),
                    ("Test_stdunit","stdunit","text"),
                    ("Test_Unit","unit","text"),
                    ("branchid","branchid","bigint"),
                    ("mainbranchid","mainbranchid","bigint"),
                    ("Test_normal","testnormal","text"),
                },
                Constants = new Dictionary<string, object>
                {

                },
                condition="where branchid ="+nFromBranchId.ToString()
            },
            new TableMap
            {
                SqlTable = "MedicineSchedule",
                PgTable  = "medicineschedule",
                Columns = new[]
                {
                    ("ScheduleName","schedulename","text"),
                    ("Schedule_NoOfQty","schedulenoofqty","text"),
                    ("branchid","branchid","bigint"),
                    ("mainbranchid","mainbranchid","bigint"),
                },
                Constants = new Dictionary<string, object>
                {

                },
                condition="where branchid ="+nFromBranchId.ToString()
            },
            new TableMap
            {
                SqlTable = "TestResult",
                PgTable  = "testresult",
                Columns = new[]
                {
                    ("Reg_Id","regid","bigint"),
                    ("OpNo","opno","bigint"),
                    ("Test_Date","testdate","date"),
                    ("Doctor_Id","doctorid","bigint"),
                    ("branchid","branchid","bigint"),
                    ("mainbranchid","mainbranchid","bigint"),
                    ("StaffId","staffid","bigint"),
                    ("LabBill_Id","labbillid","bigint"),
                    ("AcId","acid","bigint"),
                    ("TestResult_BillNo","billno","bigint"),
                    ("TestResult_Remarks","remarks","text"),
                    ("TestResult_PatientName","patientname","text"),
                    ("TestResult_Addr","addr","text"),
                    ("TestResult_Phone","phone","text"),
                    ("TestResult_Cancel","testresultcancel","text"),
                    ("ResultReady","resultready","date"),
                    ("ResultVerified","resultverified","date"),
                    ("Test_Time","testtime","text"),
                    ("ResultReady_Time","resultreadytime","text"),
                    ("ResultVerified_Time","resultverifiedtime","text"),
                    ("TestResult_Id","tempid","bigint"),
                },
                Constants = new Dictionary<string, object>
                {

                },
                condition="where branchid ="+nFromBranchId.ToString()
            },
            new TableMap
            {
                SqlTable = "TestResultSub",
                PgTable  = "testresultsub",
                Columns = new[]
                {
                    ("TestResult_Id","testresultid","bigint"),
                    ("Test_Id","testid","bigint"),
                    ("TestName","testname","text"),
                    ("TestDivisionName","testdivisionname","text"),
                    ("Result","result","text"),
                    ("NormalValue","normalvalue","text"),
                    ("branchid","branchid","bigint"),
                    ("mainbranchid","mainbranchid","bigint"),
                    ("TestSubId","testsubid","bigint"),
                    ("Remarks","remarks","text"),
                    ("Test_Unit","testunit","text"),
                    ("TestResult_Id","tempid","bigint")
                },
                Constants = new Dictionary<string, object>
                {

                },
                condition="where branchid ="+nFromBranchId.ToString()
            },
            new TableMap
            {
                SqlTable = "AppointmentDetails",
                PgTable  = "appointmentdetails",
                Columns = new[]
                {
                    ("Doctor_Id","doctorid","bigint"),
                    ("AppointmentDtl_Date","appointmentdtldate","date"),
                    ("OP_TokenNo","optokenno","bigint"),
                    ("WL_TokenNo","wltokenno","bigint"),
                    ("branchid","branchid","bigint"),
                    ("mainbranchid","mainbranchid","bigint"),
                    ("AcId","acid","bigint"),
                    ("FlagType","flagtype","text"),
                },
                Constants = new Dictionary<string, object>
                {

                },
                condition="where branchid ="+nFromBranchId.ToString()
            },
            new TableMap
            {
                SqlTable = "PMRAppointment",
                PgTable  = "pmrappointment",
                Columns = new[]
                {
                    ("AC_Id","acid","bigint"),
                    ("Reg_Id","regid","bigint"),
                    ("StaffId","staffid","bigint"),
                    ("branchid","branchid","bigint"),
                    ("mainbranchid","mainbranchid","bigint"),
                    ("AppointMentDate","appointmentdate","date"),
                    ("AppointMentTime","appointmenttime","time"),
                    ("Doctor_Id","doctorid","bigint"),
                    ("PMRAppointment_EnterDate","pmrappointmententerdate","date"),
                    ("PMRAppointment_EnterTime","pmrappointmententertime","time"),
                    ("AppointmentBillNo","appointmentbillno","bigint"),
                    ("FromId","fromid","bigint"),
                    ("FromType","fromtype","text"),
                    ("ConsultType","consulttype","text"),
                },
                Constants = new Dictionary<string, object>
                {

                },
                condition="where branchid ="+nFromBranchId.ToString()
            },
            new TableMap
            {
                SqlTable = "PMRSheet",
                PgTable  = "pmrsheet",
                Columns = new[]
                {
                    ("Reg_Id","regid","bigint"),
                    ("Re_VisitId","revisitid","bigint"),
                    ("OPNo","opno","bigint"),
                    ("IPNo","ipno","bigint"),
                    ("branchid","branchid","bigint"),
                    ("mainbranchid","mainbranchid","bigint"),
                    ("PMR_DoctorAdvice","pmrdoctoradvice","text"),
                    ("PMR_Allergies","pmrallergies","text"),
                    ("PMR_Notes","pmrnotes","text"),
                    ("PMR_Disease","pmrdisease","text"),
                    ("PMR_Diagnosis","pmrdiagnosis","text"),
                    ("DoctorId","doctorid","bigint"),
                    ("PMR_Date","pmrdate","date"),
                    ("PMR_Time","pmrtime","text"),
                    ("PMR_ImageLocation","pmrimagelocation","text"),
                    ("PMR_NextVisit","pmrnextvisit","date"),
                    ("PMR_PR","pmrpr","text"),
                    ("PMR_BP","pmrbp","text"),
                    ("PMR_Wt","pmrwt","text"),
                    ("PMR_Ht","pmrht","text"),
                    ("AcId","acid","bigint"),
                    ("PMR_BillNo","pmrbillno","bigint"),
                    ("StaffId","staffid","bigint"),
                    ("PMR_Remarks","pmrremarks","text"),
                    ("PMR_Cancel","pmrcancel","boolean"),
                    ("PMR_SystemicExam","pmrsystemicexam","text"),
                    ("PMR_LocalExam","pmrlocalexam","text"),
                    ("PMR_Investigation","pmrinvestigation","text"),
                    ("PMR_DevelopmentalHistory","pmrdevelopmentalhistory","text"),
                    ("PMR_Procedure","pmrprocedure","text"),
                    ("PMR_Extremities","pmrextremities","text"),
                    ("tempid","pmruniquekey","bigint")
                },
                Constants = new Dictionary<string, object>
                {

                },
                condition="where branchid ="+nFromBranchId.ToString()
            },
            new TableMap
            {
                SqlTable = "PMRDiagnosis",
                PgTable  = "pmrdiagnosis",
                Columns = new[]
                {
                    ("PMR_UniqueKey","pmruniquekey","bigint"),
                    ("DiagnosisId","diagnosisid","bigint"),
                    ("branchid","branchid","bigint"),
                    ("mainbranchid","mainbranchid","bigint"),
                },
                Constants = new Dictionary<string, object>
                {

                },
                condition="where branchid ="+nFromBranchId.ToString()
            },
            new TableMap
            {
                SqlTable = "PMRDiseases",
                PgTable  = "pmrdiseases",
                Columns = new[]
                {
                    ("PMR_UniqueKey","pmruniquekey","bigint"),
                    ("DiseasesId","diseasesid","bigint"),
                    ("branchid","branchid","bigint"),
                    ("mainbranchid","mainbranchid","bigint"),
                },
                Constants = new Dictionary<string, object>
                {

                },
                condition="where branchid ="+nFromBranchId.ToString()
            },
            new TableMap
            {
                SqlTable = "PMRMedicine",
                PgTable  = "pmrmedicine",
                Columns = new[]
                {
                    ("PMR_UniqueKey","pmruniquekey","bigint"),
                    ("ProductId","productid","bigint"),
                    ("Daily","daily","text"),
                    ("Dosage","dosage","text"),
                    ("Days","days","text"),
                    ("Medicine_Content","medicinecontent","text"),
                    ("MedicineName","medicinename","text"),
                    ("branchid","branchid","bigint"),
                    ("mainbranchid","mainbranchid","bigint"),
                    ("Medicine_Remarks","medicineremarks","text"),
                    ("ScheduleName","schedulename","text"),
                    ("Schedule_NoOfQty","schedulenoofqty","bigint"),
                    ("TotalNoOfQty","totalnoofqty","bigint"),
                },
                Constants = new Dictionary<string, object>
                {

                },
                condition="where branchid ="+nFromBranchId.ToString()
            },
        };

    }

}
