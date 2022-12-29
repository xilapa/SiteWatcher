﻿// <auto-generated />
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;
using SiteWatcher.Domain.Alerts.Entities.Rules;
using SiteWatcher.Infra.Persistence.Configuration;

#pragma warning disable 219, 612, 618
#nullable enable

namespace SiteWatcher.Infra.Persistence.CompiledModels
{
    internal partial class RegexRuleEntityType
    {
        public static RuntimeEntityType Create(RuntimeModel model, RuntimeEntityType? baseEntityType = null)
        {
            var runtimeEntityType = model.AddEntityType(
                "SiteWatcher.Domain.Alerts.Entities.Rules.RegexRule",
                typeof(RegexRule),
                baseEntityType,
                discriminatorProperty: "Rule");

            var notifyOnDisappearance = runtimeEntityType.AddProperty(
                "NotifyOnDisappearance",
                typeof(bool),
                propertyInfo: typeof(RegexRule).GetProperty("NotifyOnDisappearance", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(RegexRule).GetField("<NotifyOnDisappearance>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            notifyOnDisappearance.AddAnnotation("Relational:ColumnType", "boolean");

            var regexPattern = runtimeEntityType.AddProperty(
                "RegexPattern",
                typeof(string),
                propertyInfo: typeof(RegexRule).GetProperty("RegexPattern", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(RegexRule).GetField("<RegexPattern>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            regexPattern.AddAnnotation("Relational:ColumnType", "varchar(512)");

            var _matches = runtimeEntityType.AddProperty(
                "_matches",
                typeof(Matches),
                fieldInfo: typeof(RegexRule).GetField("_matches", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                valueConverter: new RegexRuleMatchesValueConverter());
            _matches.AddAnnotation("Relational:ColumnName", "Matches");
            _matches.AddAnnotation("Relational:ColumnType", "text");

            return runtimeEntityType;
        }

        public static void CreateAnnotations(RuntimeEntityType runtimeEntityType)
        {
            runtimeEntityType.AddAnnotation("DiscriminatorValue", 'R');
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