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
                    ("BankYesNo", "bankflag", "boolean"),

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

                     ("", "district", "text"),
                     ("", "statetype","text"),
                     ("", "license1", "text"),
                     ("", "license2", "text"),
                     ("", "license3", "text"),
                     ("", "license4", "text"),
                     ("0", "schedule1", "integer"),
                     ("0", "schedule2", "integer"),
                     ("0", "schedule4", "integer"),
                     ("", "introducedby", "text"),
                     ("", "acmapheadid", "text"),
                     ("", "supplytype", "text"),

                    // ---------- Branch ----------
                    ("branchid", "branchid", "bigint"),
                    ("mainbranchid", "mainbranchid", "bigint")
                },
               condition="where ac_id>55 and  branchid ="+nFromBranchId.ToString()
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
                 condition="where   branchid ="+nFromBranchId.ToString()
           },

           //new TableMap
           // {
           //     SqlTable = "Branch",
           //     PgTable  = "branch",

           //     Columns = new[]
           //     {
           //         // ---------- Identity ----------
           //         ("BranchId", "tempid", "bigint"),

           //         // ---------- Basic ----------
           //         ("BranchCode", "branchcode", "text"),
           //         ("BranchName", "branchname", "text"),
           //         ("BranchAdr1", "branchadr1", "text"),
           //         ("BranchAdr2", "branchadr2", "text"),
           //         ("BranchAdr3", "branchadr3", "text"),

           //         ("BranchFtr1", "branchftr1", "text"),
           //         ("BranchFtr2", "branchftr2", "text"),
           //         ("BranchFtr3", "branchftr3", "text"),

           //         // ---------- Contact ----------
           //         ("Phone", "branchphone", "text"),
           //         ("Mail", "branchmail", "text"),
           //         ("Active", "branchactive", "boolean"),

           //         ("MobileNo", "branchmobileno", "text"),
           //         ("MailId", "branchmailid", "text"),
           //         ("MailPwd", "branchmailpwd", "text"),

           //         // ---------- Licenses ----------
           //         ("TinNo1", "branchtinno1", "text"),
           //         ("TinNo2", "branchtinno2", "text"),
           //         ("DLNo1", "branchdlno1", "text"),
           //         ("DLNo2", "branchdlno2", "text"),

           //         // ---------- Barcode ----------
           //         ("BarCodeName", "branchbarcodename", "text"),
           //         ("BarCodeHeaderName", "barcodeheadername", "text"),
           //         ("ComImage", "branchcomimage", "text"),

           //         // ---------- State ----------
           //         ("Branch_StateCode", "branchstatecode", "integer"),
           //         ("Branch_StateName", "branchstatename", "text"),

           //         // ---------- Bank ----------
           //         ("Branch_BankName", "branchbankname", "text"),
           //         ("Branch_BankAddr1", "branchbankaddr1", "text"),
           //         ("Branch_BankAddr2", "branchbankaddr2", "text"),
           //         ("Branch_BankAcNo", "branchbankacno", "text"),
           //         ("Branch_IFSCCODE", "branchifsccode", "text"),
           //         ("Branch_PanCardNo", "branchpancardno", "text"),

           //         // ---------- QR & Declaration ----------
           //         ("Branch_QRCode", "branchqrcode", "text"),

           //         ("Branch_Declaration1", "branchdeclaration1", "text"),
           //         ("Branch_Declaration2", "branchdeclaration2", "text"),
           //         ("Branch_Declaration3", "branchdeclaration3", "text"),
           //         ("Branch_Declaration4", "branchdeclaration4", "text"),

           //         // ---------- Order login ----------
           //         ("Branch_OrderUserName", "branchorderusername", "text"),
           //         ("Branch_OrderPwd", "branchorderpwd", "text"),

           //         // ---------- WhatsApp ----------
           //         ("Branch_WhatsAppNo", "branchwhatsappno", "text"),
           //         ("Branch_WhatsAppTokenNo", "branchwhatsapptokenno", "text"),
           //         ("Branch_WhatsAppUrl", "branchwhatsappurl", "text"),

           //         // ---------- Security ----------
           //         ("Branch_SecurePwd", "branchsecurepwd", "text"),
           //         ("Branch_BarCodeDesign", "branchbarcodedesign", "text"),

           //         // ---------- Account ----------
           //         ("AcId", "acid", "bigint"),

           //         // ---------- Extra PostgreSQL only ----------
           //         ("", "branchlocation", "text"),
           //         ("", "taxtype", "text"),
           //         ("", "username", "text"),
           //         ("", "pwd", "text"),

           //         // ---------- FSSAI ----------
           //         ("", "fssai", "text"),

           //         // ---------- Constants ----------
           //         ("1", "mainbranchid", "bigint")
           //     }
           // },

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
            SqlTable = "PurBillSeries",
            PgTable  = "billseries@@@@",
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
                    ("ProductType", "producttype", "text"),

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
              ("ReceiptSub_BarCode","pcsactpurrate","numeric"),
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
                  {"receiptid","0"}
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
                 ("Store_BarCode","actpurrate","numeric"),
                 ("Store_BarCode","pcsactpurrate","numeric"),
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
                 ("LandingCost","perlandcost","numeric"),
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
                      ("StaffId","staffid","bigint"),
                      ("branchid","branchid","bigint"),
                      ("mainbranchid","mainbranchid","bigint")
               },
               Constants = new Dictionary<string, object>
               {
                      {"receipttime",TimeOnly.FromDateTime(DateTime.Now) },
                      {"total","0"},
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
                   
                   ("","openingstockid","bigint"),
                   ("OpeningStockNo","openingstockno","bigint"),
                   ("","billserid","integer"),
                   ("OpeningStock_ReceiptDate","openingstockdate","date"),
                   ("OpeningStock_BatchSlNo","batchslno","bigint"),
                   ("OpeningStock_Batch","batch","text"),
                   ("OpeningStock_Pack","pack","numeric(18,0)"),
                   ("","qtytype","text"),
                   ("","prodpack","integer"),
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
                   ("","replaceqty","numeric"),
                   ("","looseqty","numeric"), 
                   ("","totalqty","numeric"),
                   ("","netamtperprod","numeric"),
                   ("OpeningStock_Amount","amount","numeric"),
                   ("OpeningStock_BarCode","actpurrate","numeric"),
                   ("OpeningStock_BarCode","packactpurrate","numeric"),
                   ("OpeningStock_DisributRate","whrate","numeric"),
                   ("SpRate1","sprate1","numeric"),
                   ("SpRate2","sprate2","numeric"),
                   ("","sprate3","numeric"),
                   ("","sprate4","numeric"),
                   ("","sprate5","numeric"),
                   ("","packsellrate","numeric"),
                   ("","packwhrate","numeric"),
                   ("","packmrp","numeric"),
                   ("","packsprate1","numeric"),
                   ("","packsprate2","numeric"),
                   ("","packsprate3","numeric"),
                   ("","packsprate4","numeric"),
                   ("","packsprate5","numeric"),
                   ("","taxpers","numeric"),
                   ("","taxamt","numeric"),
                   ("","wholsalmag","numeric"),
                   ("","retlmargin","numeric"),
                   ("","schmeperiod","date"),
                   ("","productid","bigint"),
                   ("","taxid","integer"),
                   ("","landcost","numeric"),
                   ("","dispers","numeric"),
                   ("","disamt","numeric"),
                   ("","schemepers","numeric"),
                   ("","schemeamt","numeric"),
                   ("","purratewithtax","numeric"),
                   ("","freight","numeric"),
                   ("","totlqty","numeric"),
                   ("","neethidispers","numeric"),
                   ("","amtbeforetax","numeric"),
                   ("","wratedis","numeric"),
                   ("","perlandcost","numeric(18,3)"),
                   ("","hsncode","text"),
                   ("","sgsttaxpers","numeric"),
                   ("","sgsttaxamount","numeric"),
                   ("","sgstamount","numeric"),
                   ("","cgsttaxpers","numeric"),
                   ("","cgsttaxamount","numeric(18,3)"),
                   ("","cgstamount","numeric"),
                   ("","igsttaxpers","numeric"),
                   ("","igsttaxamount","numeric"),
                   ("","igstamount","numeric"),
                   ("","cesspers","numeric"),
                   ("","cessamt","numeric(18,3)"),
                   ("","imppurrate","numeric(18,3)"),
                   ("","extracesspers","numeric"),
                   ("","extracessamt","numeric"),
                   ("","prodremarks","text"),
                   ("OpeningStockMain_Id","dcinno","bigint"),
                   ("","actratewithoutfre","numeric"),
                   ("","hsnrateperunit","numeric"),
                   ("branchid","branchid","bigint"),
                   ("mainbranchid","mainbranchid","bigint")
              },
              Constants = new Dictionary<string, object>
              {
                 {"billsersource", "SALES" },

              },
              condition="where   branchid ="+nFromBranchId.ToString()
            },
        };

    }

}
