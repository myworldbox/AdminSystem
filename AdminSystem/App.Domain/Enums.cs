namespace AdminSystem.Domain
{
    public class Enums
    {
        public enum Category
        {
            VIP,
            一般,
            黑名單
        }

        public enum Shared {
            _Table,
            _Pagination
        }

        public enum Order
        {
            Asc,
            Desc
        }

        public enum Role
        {
            Clerk,
            Supervisor,
            Manager,
            Admin
        }

        public enum Database
        {
            // Relational Databases (SQL)
            SqlServer,
            PostgreSQL,
            MySQL,
            MariaDB,
            Oracle,
            SQLite,
            IBMDb2,
            Firebird,
            SAPHana,
            Teradata,
            Sybase,

            // NoSQL Databases
            MongoDB,
            Cassandra,
            Couchbase,
            Redis,
            Neo4j,
            DynamoDB,
            CosmosDB,
            ArangoDB,
            RavenDB,
            OrientDB,
            InfluxDB,
            FaunaDB,
            Firebase,
            Firestore,

            // Cloud/Hybrid Databases
            AmazonAurora,
            GoogleCloudSpanner,
            AzureSQL,
            AzureCosmosDB,
            PlanetScale,
            YugabyteDB,
            TiDB
        }

    }
}