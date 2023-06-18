﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using SiteWatcher.Infra;

#nullable disable

namespace SiteWatcher.Infra.Migrations
{
    [DbContext(typeof(SiteWatcherContext))]
    partial class SiteWatcherContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("sw")
                .HasAnnotation("ProductVersion", "7.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("SiteWatcher.Domain.Alerts.Alert", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<bool>("Active")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamptz");

                    b.Property<short>("Frequency")
                        .HasColumnType("smallint");

                    b.Property<DateTime>("LastUpdatedAt")
                        .HasColumnType("timestamptz");

                    b.Property<DateTime?>("LastVerification")
                        .HasColumnType("timestamptz");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(64)");

                    b.Property<string>("SearchField")
                        .IsRequired()
                        .HasColumnType("varchar(640)");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Alerts", "sw");
                });

            modelBuilder.Entity("SiteWatcher.Domain.Alerts.Entities.Rules.Rule", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<bool>("Active")
                        .HasColumnType("boolean");

                    b.Property<int>("AlertId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamptz");

                    b.Property<bool>("FirstWatchDone")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("LastUpdatedAt")
                        .HasColumnType("timestamptz");

                    b.Property<char>("Rule")
                        .HasColumnType("char");

                    b.HasKey("Id");

                    b.HasIndex("AlertId")
                        .IsUnique();

                    b.ToTable("Rules", "sw");

                    b.HasDiscriminator<char>("Rule");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("SiteWatcher.Domain.Alerts.Entities.Triggerings.Triggering", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("AlertId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamptz");

                    b.Property<short>("Status")
                        .HasColumnType("smallint");

                    b.HasKey("Id");

                    b.HasIndex("AlertId");

                    b.ToTable("Triggerings", "sw");
                });

            modelBuilder.Entity("SiteWatcher.Domain.Common.ValueObjects.IdempotentConsumer", b =>
                {
                    b.Property<string>("MessageId")
                        .HasColumnType("varchar(128)");

                    b.Property<string>("Consumer")
                        .HasColumnType("varchar(128)");

                    b.HasKey("MessageId", "Consumer");

                    b.ToTable("IdempotentConsumers", "sw");
                });

            modelBuilder.Entity("SiteWatcher.Domain.Emails.Email", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<bool>("Active")
                        .HasColumnType("boolean");

                    b.Property<string>("Body")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamptz");

                    b.Property<DateTime?>("DateSent")
                        .HasColumnType("timestamptz");

                    b.Property<DateTime?>("ErrorDate")
                        .HasColumnType("timestamptz");

                    b.Property<string>("ErrorMessage")
                        .HasColumnType("text");

                    b.Property<DateTime>("LastUpdatedAt")
                        .HasColumnType("timestamptz");

                    b.Property<string>("Recipient")
                        .IsRequired()
                        .HasColumnType("varchar(385)");

                    b.Property<string>("Subject")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Emails", "sw");
                });

            modelBuilder.Entity("SiteWatcher.Domain.Notifications.Notification", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamptz");

                    b.Property<Guid?>("EmailId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("EmailId");

                    b.HasIndex("UserId");

                    b.ToTable("Notifications", "sw");
                });

            modelBuilder.Entity("SiteWatcher.Domain.Notifications.NotificationAlert", b =>
                {
                    b.Property<Guid>("NotificationId")
                        .HasColumnType("uuid");

                    b.Property<int>("AlertId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("TriggeringDate")
                        .HasColumnType("timestamptz");

                    b.HasKey("NotificationId", "AlertId");

                    b.HasIndex("AlertId");

                    b.ToTable("NotificationAlerts", "sw");
                });

            modelBuilder.Entity("SiteWatcher.Domain.Users.User", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<bool>("Active")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamptz");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("varchar(320)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("boolean");

                    b.Property<string>("GoogleId")
                        .IsRequired()
                        .HasColumnType("varchar(30)");

                    b.Property<short>("Language")
                        .HasColumnType("smallint");

                    b.Property<DateTime>("LastUpdatedAt")
                        .HasColumnType("timestamptz");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(64)");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("varchar(120)");

                    b.Property<short>("Theme")
                        .HasColumnType("smallint");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "GoogleId" }, "IX_Unique_User_GoogleId")
                        .IsUnique();

                    b.HasIndex(new[] { "SecurityStamp" }, "IX_User_SecurityStamp");

                    b.ToTable("Users", "sw");
                });

            modelBuilder.Entity("SiteWatcher.Domain.Alerts.Entities.Rules.AnyChangesRule", b =>
                {
                    b.HasBaseType("SiteWatcher.Domain.Alerts.Entities.Rules.Rule");

                    b.Property<string>("HtmlHash")
                        .IsRequired()
                        .HasColumnType("varchar(64)");

                    b.HasDiscriminator().HasValue('A');
                });

            modelBuilder.Entity("SiteWatcher.Domain.Alerts.Entities.Rules.RegexRule", b =>
                {
                    b.HasBaseType("SiteWatcher.Domain.Alerts.Entities.Rules.Rule");

                    b.Property<bool>("NotifyOnDisappearance")
                        .HasColumnType("boolean");

                    b.Property<string>("RegexPattern")
                        .IsRequired()
                        .HasColumnType("varchar(512)");

                    b.Property<string>("_matches")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("Matches");

                    b.HasDiscriminator().HasValue('R');
                });

            modelBuilder.Entity("SiteWatcher.Domain.Alerts.Entities.Rules.TermRule", b =>
                {
                    b.HasBaseType("SiteWatcher.Domain.Alerts.Entities.Rules.Rule");

                    b.Property<string>("Term")
                        .IsRequired()
                        .HasColumnType("varchar(64)");

                    b.HasDiscriminator().HasValue('T');
                });

            modelBuilder.Entity("SiteWatcher.Domain.Alerts.Alert", b =>
                {
                    b.HasOne("SiteWatcher.Domain.Users.User", "User")
                        .WithMany("Alerts")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.OwnsOne("SiteWatcher.Domain.Alerts.ValueObjects.Site", "Site", b1 =>
                        {
                            b1.Property<int>("AlertId")
                                .HasColumnType("integer");

                            b1.Property<string>("Name")
                                .IsRequired()
                                .HasColumnType("varchar(64)");

                            b1.Property<string>("Uri")
                                .IsRequired()
                                .HasColumnType("varchar(512)");

                            b1.HasKey("AlertId");

                            b1.ToTable("Alerts", "sw");

                            b1.WithOwner()
                                .HasForeignKey("AlertId");
                        });

                    b.Navigation("Site")
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("SiteWatcher.Domain.Alerts.Entities.Rules.Rule", b =>
                {
                    b.HasOne("SiteWatcher.Domain.Alerts.Alert", null)
                        .WithOne("Rule")
                        .HasForeignKey("SiteWatcher.Domain.Alerts.Entities.Rules.Rule", "AlertId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("SiteWatcher.Domain.Alerts.Entities.Triggerings.Triggering", b =>
                {
                    b.HasOne("SiteWatcher.Domain.Alerts.Alert", null)
                        .WithMany("Triggerings")
                        .HasForeignKey("AlertId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("SiteWatcher.Domain.Emails.Email", b =>
                {
                    b.HasOne("SiteWatcher.Domain.Users.User", null)
                        .WithMany("Emails")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("SiteWatcher.Domain.Notifications.Notification", b =>
                {
                    b.HasOne("SiteWatcher.Domain.Emails.Email", "Email")
                        .WithMany()
                        .HasForeignKey("EmailId");

                    b.HasOne("SiteWatcher.Domain.Users.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId");

                    b.Navigation("Email");

                    b.Navigation("User");
                });

            modelBuilder.Entity("SiteWatcher.Domain.Notifications.NotificationAlert", b =>
                {
                    b.HasOne("SiteWatcher.Domain.Alerts.Alert", null)
                        .WithMany()
                        .HasForeignKey("AlertId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SiteWatcher.Domain.Notifications.Notification", null)
                        .WithMany("NotificationAlerts")
                        .HasForeignKey("NotificationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("SiteWatcher.Domain.Alerts.Entities.Rules.TermRule", b =>
                {
                    b.OwnsMany("SiteWatcher.Domain.Alerts.Entities.Rules.TermOccurrence", "Occurrences", b1 =>
                        {
                            b1.Property<int>("TermRuleId")
                                .HasColumnType("integer");

                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("integer");

                            NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b1.Property<int>("Id"));

                            b1.Property<string>("Context")
                                .IsRequired()
                                .HasColumnType("varchar(512)");

                            b1.HasKey("TermRuleId", "Id");

                            b1.ToTable("TermOccurrences", "sw");

                            b1.WithOwner()
                                .HasForeignKey("TermRuleId");
                        });

                    b.Navigation("Occurrences");
                });

            modelBuilder.Entity("SiteWatcher.Domain.Alerts.Alert", b =>
                {
                    b.Navigation("Rule")
                        .IsRequired();

                    b.Navigation("Triggerings");
                });

            modelBuilder.Entity("SiteWatcher.Domain.Notifications.Notification", b =>
                {
                    b.Navigation("NotificationAlerts");
                });

            modelBuilder.Entity("SiteWatcher.Domain.Users.User", b =>
                {
                    b.Navigation("Alerts");

                    b.Navigation("Emails");
                });
#pragma warning restore 612, 618
        }
    }
}
