﻿// <auto-generated />
using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using SiteWatcher.Domain.Alerts;
using SiteWatcher.Domain.Alerts.Entities.Rules;
using SiteWatcher.Domain.Common;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Infra.Persistence.Configuration;

#pragma warning disable 219, 612, 618
#nullable enable

namespace SiteWatcher.Infra.Persistence.CompiledModels
{
    internal partial class RuleEntityType
    {
        public static RuntimeEntityType Create(RuntimeModel model, RuntimeEntityType? baseEntityType = null)
        {
            var runtimeEntityType = model.AddEntityType(
                "SiteWatcher.Domain.Alerts.Entities.Rules.Rule",
                typeof(Rule),
                baseEntityType,
                discriminatorProperty: "Rule");

            var id = runtimeEntityType.AddProperty(
                "Id",
                typeof(RuleId),
                propertyInfo: typeof(BaseModel<RuleId>).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(BaseModel<RuleId>).GetField("<Id>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                valueGenerated: ValueGenerated.OnAdd,
                afterSaveBehavior: PropertySaveBehavior.Throw,
                valueGeneratorFactory: new RuleIdValueGeneratorFactory().Create,
                valueConverter: new RuleId.EfCoreValueConverter());
            id.AddAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
            id.AddAnnotation("Relational:ColumnType", "integer");

            var active = runtimeEntityType.AddProperty(
                "Active",
                typeof(bool),
                propertyInfo: typeof(BaseModel<RuleId>).GetProperty("Active", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(BaseModel<RuleId>).GetField("<Active>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            active.AddAnnotation("Relational:ColumnType", "boolean");

            var alertId = runtimeEntityType.AddProperty(
                "AlertId",
                typeof(AlertId));

            var createdAt = runtimeEntityType.AddProperty(
                "CreatedAt",
                typeof(DateTime),
                propertyInfo: typeof(BaseModel<RuleId>).GetProperty("CreatedAt", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(BaseModel<RuleId>).GetField("<CreatedAt>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            createdAt.AddAnnotation("Relational:ColumnType", "timestamptz");

            var firstWatchDone = runtimeEntityType.AddProperty(
                "FirstWatchDone",
                typeof(bool),
                propertyInfo: typeof(Rule).GetProperty("FirstWatchDone", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(Rule).GetField("<FirstWatchDone>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            firstWatchDone.AddAnnotation("Relational:ColumnType", "boolean");

            var lastUpdatedAt = runtimeEntityType.AddProperty(
                "LastUpdatedAt",
                typeof(DateTime),
                propertyInfo: typeof(BaseModel<RuleId>).GetProperty("LastUpdatedAt", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(BaseModel<RuleId>).GetField("<LastUpdatedAt>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            lastUpdatedAt.AddAnnotation("Relational:ColumnType", "timestamptz");

            var rule = runtimeEntityType.AddProperty(
                "Rule",
                typeof(char),
                afterSaveBehavior: PropertySaveBehavior.Throw,
                valueGeneratorFactory: new DiscriminatorValueGeneratorFactory().Create);
            rule.AddAnnotation("Relational:ColumnType", "char");

            var key = runtimeEntityType.AddKey(
                new[] { id });
            runtimeEntityType.SetPrimaryKey(key);

            var index = runtimeEntityType.AddIndex(
                new[] { alertId },
                unique: true);

            return runtimeEntityType;
        }

        public static RuntimeForeignKey CreateForeignKey1(RuntimeEntityType declaringEntityType, RuntimeEntityType principalEntityType)
        {
            var runtimeForeignKey = declaringEntityType.AddForeignKey(new[] { declaringEntityType.FindProperty("AlertId")! },
                principalEntityType.FindKey(new[] { principalEntityType.FindProperty("Id")! })!,
                principalEntityType,
                deleteBehavior: DeleteBehavior.Cascade,
                unique: true,
                required: true,
                requiredDependent: true);

            var rule = principalEntityType.AddNavigation("Rule",
                runtimeForeignKey,
                onDependent: false,
                typeof(Rule),
                propertyInfo: typeof(Alert).GetProperty("Rule", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(Alert).GetField("<Rule>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            return runtimeForeignKey;
        }

        public static void CreateAnnotations(RuntimeEntityType runtimeEntityType)
        {
            runtimeEntityType.AddAnnotation("Relational:FunctionName", null);
            runtimeEntityType.AddAnnotation("Relational:Schema", "sw");
            runtimeEntityType.AddAnnotation("Relational:SqlQuery", null);
            runtimeEntityType.AddAnnotation("Relational:TableName", "Rules");
            runtimeEntityType.AddAnnotation("Relational:ViewName", null);
            runtimeEntityType.AddAnnotation("Relational:ViewSchema", null);

            Customize(runtimeEntityType);
        }

        static partial void Customize(RuntimeEntityType runtimeEntityType);
    }
}
