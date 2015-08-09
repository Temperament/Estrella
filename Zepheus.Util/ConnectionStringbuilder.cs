using System.Data.EntityClient;
using MySql.Data.MySqlClient;

namespace Zepheus.Util
{
    public class ConnectionStringbuilder
    {
        public static string CreateEntityString(EntitySetting setting)
        {
            // Initialize the EntityConnectionStringBuilder.
            EntityConnectionStringBuilder entityBuilder =
                new EntityConnectionStringBuilder();
            //Set the provider name.
            entityBuilder.Provider = "MySql.Data.MySqlClient;";

            // Set the provider-specific connection string.
            entityBuilder.ProviderConnectionString = CreateConnectionString(setting);
            // Set the Metadata location.
            entityBuilder.Metadata = setting.Metadata;
            return entityBuilder.ToString();
        }

        public static string CreateConnectionString(EntitySetting setting, bool security = true, bool multi = true)
        {
            // Initialize the connection string builder for the
            // underlying provider.
            /*SqlConnectionStringBuilder sqlBuilder =
                new SqlConnectionStringBuilder();
           
            // Set the properties for the data source.
            sqlBuilder.DataSource = setting.DataSource;
            sqlBuilder.InitialCatalog = setting.DataCatalog;
            if (!(string.IsNullOrEmpty(setting.Username) && string.IsNullOrEmpty(setting.Password)))
            {
                sqlBuilder.UserID = setting.Username;
                sqlBuilder.Password = setting.Password;
            }
            sqlBuilder.IntegratedSecurity = security;
            sqlBuilder.MultipleActiveResultSets = multi; //allows you to have multiple datareaders at once
            */
            string myConnectionString = "SERVER=localhost;" +
                            "DATABASE=zep2;" +
                            "UID=root;" +
                            "PASSWORD=;";
            // Build the SqlConnection connection string.
            //return sqlBuilder.ToString();
            return myConnectionString;
        }
    }
}
