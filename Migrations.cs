using System;
using System.Data;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.UI.Notify;

namespace Proxy {

    public class Migrations : DataMigrationImpl {
        protected INotifier Notifier { get; set; }
        protected Localizer T { get; set; }
        protected ILogger Logger { get; set; }

        public Migrations(INotifier notifier) {
            Notifier = notifier;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public int Create() {

            try {

                SchemaBuilder.CreateTable("ProxyPartRecord", table => table
                    .ContentPartRecord()
                    .Column("ServiceUrl", DbType.String)
                );

                var proxy = new ContentTypeDefinition("Proxy", "Proxy");
                ContentDefinitionManager.StoreTypeDefinition(proxy);
                ContentDefinitionManager.AlterTypeDefinition("Proxy", cfg => cfg.Creatable()
                    .WithPart("ProxyPart")
                    .WithPart("CommonPart")
                    .WithPart("TitlePart")
                    .WithPart("IdentityPart")
                    .WithPart("ContentPermissionsPart", builder => builder
                        .WithSetting("ContentPermissionsPartSettings.View", "Administrator")
                        .WithSetting("ContentPermissionsPartSettings.Publish", "Administrator")
                        .WithSetting("ContentPermissionsPartSettings.Edit", "Administrator")
                        .WithSetting("ContentPermissionsPartSettings.Delete", "Administrator")
                        .WithSetting("ContentPermissionsPartSettings.DisplayedRoles", "Administrator,Authenticated")
                    )
                );

            } catch (Exception e) {
                var message = string.Format("Error creating Proxy module. {0}", e.Message);
                Logger.Warning(message);
                Notifier.Warning(T(message));
                return 0;
            }
            return 1;
        }

        public int UpdateFrom1() {
            SchemaBuilder.AlterTable("ProxyPartRecord",
                table => table
                    .AddColumn("ForwardHeaders", DbType.Boolean));

            return 2;
        }


    }
}