using System.Collections.Generic;

namespace CodeAppsDataMigration.Migration
{
    public static class FkRegistry
    {
        public static List<FkRelation> Relations => new()
        {
            // =====================================================
            // PRODUCT
            // =====================================================

            new FkRelation
            {
                ChildTable = "productsub1",
                ChildColumn = "productid",
                ParentTable = "productmain1",
                ParentPk = "productid"
            },

            // =====================================================
            // CATEGORY
            // =====================================================

            new FkRelation
            {
                ChildTable = "productmain1",
                ChildColumn = "categoryid",
                ParentTable = "category",
                ParentPk = "categoryid"
            },

            // =====================================================
            // AREA
            // =====================================================

            new FkRelation
            {
                ChildTable = "accounthead1",
                ChildColumn = "areaid",
                ParentTable = "area",
                ParentPk = "area_id"
            },

            // =====================================================
            // ACCOUNT
            // =====================================================

            new FkRelation
            {
                ChildTable = "issuemain1",
                ChildColumn = "acid",
                ParentTable = "accounthead1",
                ParentPk = "acid"
            },

            // =====================================================
            // ISSUE → PRODUCT
            // =====================================================

            new FkRelation
            {
                ChildTable = "issuereturndetails1",
                ChildColumn = "productid",
                ParentTable = "productmain1",
                ParentPk = "productid"
            },

            // =====================================================
            // ISSUE → SALESMAN
            // =====================================================

            new FkRelation
            {
                ChildTable = "issuemain1",
                ChildColumn = "salesexeid",
                ParentTable = "accounthead1",
                ParentPk = "acid"
            },

            // =====================================================
            // ISSUE → AGENT
            // =====================================================

            new FkRelation
            {
                ChildTable = "issuemain1",
                ChildColumn = "agentid",
                ParentTable = "accounthead1",
                ParentPk = "acid"
            },

            // =====================================================
            // BRANCH
            // =====================================================

            new FkRelation
            {
                ChildTable = "accounthead1",
                ChildColumn = "branchid",
                ParentTable = "branch",
                ParentPk = "branchid"
            },

            new FkRelation
            {
                ChildTable = "productmain1",
                ChildColumn = "branchid",
                ParentTable = "branch",
                ParentPk = "branchid"
            },

            new FkRelation
            {
                ChildTable = "productsub1",
                ChildColumn = "branchid",
                ParentTable = "branch",
                ParentPk = "branchid"
            },

            new FkRelation
            {
                ChildTable = "issuemain1",
                ChildColumn = "branchid",
                ParentTable = "branch",
                ParentPk = "branchid"
            },

            // =====================================================
            // ISSUE → ISSUE SUB
            // =====================================================

            new FkRelation
            {
                ChildTable = "issuesubdetails1",
                ChildColumn = "issueno",
                ParentTable = "issuemain1",
                ParentPk = "issueno"
            }
        };
    }
}
