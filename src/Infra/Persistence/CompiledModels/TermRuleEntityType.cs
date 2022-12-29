﻿// <auto-generated />
using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;
using SiteWatcher.Domain.Alerts.Entities.Rules;

#pragma warning disable 219, 612, 618
#nullable enable

namespace SiteWatcher.Infra.Persistence.CompiledModels
{
    internal partial class TermRuleEntityType
    {
        public static RuntimeEntityType Create(RuntimeModel model, RuntimeEntityType? baseEntityType = null)
        {
            var runtimeEntityType = model.AddEntityType(
                "SiteWatcher.Domain.Alerts.Entities.Rules.TermRule",
                typeof(TermRule),
                baseEntityType,
                discriminatorProperty: "Rule");

            var term = runtimeEntityType.AddProperty(
                "Term",
                typeof(string),
                propertyInfo: typeof(TermRule).GetProperty("Term", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(TermRule).GetField("<Term>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            term.AddAnnotation("Relational:ColumnType", "varchar(64)");

            return runtimeEntityType;
        }

        public static void CreateAnnotations(RuntimeEntityType runtimeEntityType)
        {
            runtimeEntityType.AddAnnotation("DiscriminatorValue", 'T');
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