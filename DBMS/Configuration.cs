using System;
using System.Collections.Generic;
using System.Data;

namespace DBMS
{
    class Configuration
    {
        public string parentTableName { get; }
        public string childTableName { get; }

        public List<string> parentTableColumns { get; }
        public List<string> childTableColumns { get; }

        public string parentSelectString { get; }
        public string childSelectString { get; }

        public string connectionString { get; }
        public int scenarioId { get; }

        public string parentPrimaryKeyName { get; }
        public string parentForeignKeyName { get; }

        public string childPrimaryKeyName { get; }
        public string childForeignKeyName { get; }

        private Configuration(int scenarioId, string connectionString, string parentTableName, List<string> parentTableColumns, string parentSelectString,
            string childTableName, List<string> childTableColumns, string childSelectString, string parentPrimaryKeyName, string parentForeignKeyName,
            string childPrimaryKeyName, string childForeignKeyName)
        {
            this.scenarioId = scenarioId;
            this.connectionString = connectionString;
            this.parentTableColumns = parentTableColumns;
            this.parentSelectString = parentSelectString;
            this.parentTableName = parentTableName;

            this.childTableName = childTableName;
            this.childTableColumns = childTableColumns;
            this.childSelectString = childSelectString;

            this.parentPrimaryKeyName = parentPrimaryKeyName;
            this.parentForeignKeyName = parentForeignKeyName;

            this.childPrimaryKeyName = childPrimaryKeyName;
            this.childForeignKeyName = childForeignKeyName;
        }

        public static Configuration readConfiguration(string filename)
        {
            if(!filename.EndsWith(".xml"))
            {
                throw new InvalidOperationException("Cannot load a configuration from file:" + filename);
            }
            
            DataSet ds = new DataSet();
            ds.ReadXml(filename);
            string connectionString="", childTableName="", parentTableName="", parentSelectString="", childSelectString="", parentPK="", parentFK="",
                childPK="", childFK="";
            List<string> childTableColumns = new List<string>(), parentTableColumns = new List<string>();
            int scenarioId;

            // read the run configuration for all scenarios
            connectionString = ds.Tables["configuration"].Rows[0]["connectionString"].ToString();
            scenarioId = Int32.Parse(ds.Tables["configuration"].Rows[0]["currentScenarioId"].ToString());

            //Console.WriteLine("data=" + Int32.Parse(ds.Tables["configuration"].Rows[0]["currentScenarioId"].ToString()));

            // find the scenario with the current ID and load it
            foreach (DataRow row in ds.Tables["scenario"].Rows)
            {
                // skip the scenarios we're not interested in
                if (Int32.Parse(row["id"].ToString()) != scenarioId)
                    continue;

                // found it. Load its values

                // get parentTable and childTable, both are children of scenario
                DataRow[] data = row.GetChildRows("scenario_parentTable");
                DataRow parent = data[0];

                data = row.GetChildRows("scenario_childTable");
                DataRow child = data[0];

                // process the parent
                parentTableName = parent["name"].ToString();

                DataRow[] rows = parent.GetChildRows("parentTable_selectString");
                parentSelectString = parent.GetChildRows("parentTable_selectString")[0]["value"].ToString();     // 0 - selectString

                // get the PK and FK
                parentPK = parent["primary-key"].ToString();
                parentFK = parent["foreign-key"].ToString();

                // load the column names
                DataRow[] columns = parent.GetChildRows("parentTable_columns")[0].GetChildRows("columns_column");

                foreach (DataRow colNameRow in columns)
                {
                    parentTableColumns.Add(colNameRow["name"].ToString());
                }

                // process the child
                childTableName = child["name"].ToString();

                childSelectString = child.GetChildRows("childTable_selectString")[0]["value"].ToString();

                childPK = child["primary-key"].ToString();
                childFK = child["foreign-key"].ToString();

                columns = child.GetChildRows("childTable_columns")[0].GetChildRows("columns_column");
                foreach (DataRow colNameRow in columns)
                {
                    childTableColumns.Add(colNameRow["name"].ToString());
                }
            }

            // return the extracted data into a Configuration object
            return new Configuration(scenarioId, connectionString, parentTableName, parentTableColumns, parentSelectString,
                childTableName, childTableColumns, childSelectString, parentPK, parentFK, childPK, childFK);
        }
    }
}
