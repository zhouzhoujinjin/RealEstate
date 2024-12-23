using Approval.Entities;
using Approval.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Approval
{
  public class ApprovalDbContext : DbContext
  {
    public DbSet<ApprovalItemEntity> ApprovalItems { get; set; }
    public DbSet<ApprovalTemplateEntity> ApprovalTemplates { get; set; }
    public DbSet<ApprovalNodeEntity> ApprovalNodes { get; set; }
    public DbSet<ApprovalReadLogEntity> ApprovalReadLogs { get; set; }


    public ApprovalDbContext(DbContextOptions<ApprovalDbContext> options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
      base.OnModelCreating(builder);

      builder.Entity<ApprovalTemplateEntity>(entity =>
      {
        entity.ToTable("Approval_Template");
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.HasKey(e => e.Id);
        entity.Property(e => e.ConditionFields).HasConversion(
          v => JsonConvert.SerializeObject(v),
          v => JsonConvert.DeserializeObject<List<ConditionField>>(v)
        );
        entity.Property(e => e.Applicants).HasConversion(
          v => JsonConvert.SerializeObject(v),
          v => JsonConvert.DeserializeObject<DepartmentsAndUsers>(v)
        );
        entity.Property(e => e.Workflow).HasConversion(
          v => JsonConvert.SerializeObject(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
          v => JsonConvert.DeserializeObject<Workflow>(v)
        );
        entity.Property(e => e.Fields).HasConversion(
          v => JsonConvert.SerializeObject(v),
          v => JsonConvert.DeserializeObject<List<FormField>>(v)
        );
        entity.Property(e => e.DepartmentIds).HasConversion(
          v => JsonConvert.SerializeObject(v),
          v => JsonConvert.DeserializeObject<List<int>>(v)
        );
        entity.Property(e => e.Group).HasConversion(new EnumToStringConverter<TemplateGroup>());
      });
      builder.Entity<ApprovalItemEntity>(entity =>
      {
        entity.ToTable("Approval_Item");
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Content).HasConversion(
          v => JsonConvert.SerializeObject(v),
          v => JsonConvert.DeserializeObject<Dictionary<string, string>>(v)
          );
        entity.Property(e => e.FinalFiles).HasConversion(
          v => JsonConvert.SerializeObject(v),
          v => JsonConvert.DeserializeObject<List<AttachFile>>(v)
          );
        entity.Property(e => e.VerifiedFiles).HasConversion(
         v => JsonConvert.SerializeObject(v),
         v => JsonConvert.DeserializeObject<List<AttachFile>>(v)
         );
        entity.HasMany(e => e.Nodes).WithOne(e => e.Item).HasForeignKey(x => x.ItemId);
        entity.HasOne(e => e.Template).WithMany(e => e.Items).HasForeignKey(x => x.TemplateId);
        entity.Property(e => e.Status).HasConversion(new EnumToStringConverter<ApprovalItemStatus>());

        entity.Property(e => e.Purview).HasConversion(
        v => JsonConvert.SerializeObject(v),
        v => JsonConvert.DeserializeObject<List<string>>(v)
        );

      });
      builder.Entity<ApprovalNodeEntity>(entity =>
      {
        entity.ToTable("Approval_Node");
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.HasKey(e => e.Id);
        entity.HasOne(e => e.PreviousNode).WithMany().HasPrincipalKey("Id").HasForeignKey("PreviousId")
        .OnDelete(DeleteBehavior.Restrict)
        .IsRequired(false);
        entity.HasOne(e => e.NextNode).WithMany().HasPrincipalKey("Id").HasForeignKey("NextId")
        .OnDelete(DeleteBehavior.Restrict)
        .IsRequired(false);
        entity.Property(e => e.Comments).HasConversion(
          v => JsonConvert.SerializeObject(v),
          v => JsonConvert.DeserializeObject<List<BriefComment>>(v));
        entity.Property(e => e.Attachments).HasConversion(
          v => JsonConvert.SerializeObject(v),
          v => JsonConvert.DeserializeObject<List<AttachFile>>(v)
          );
        entity.Property(e => e.ActionType).HasConversion(new EnumToStringConverter<ApprovalActionType>());
        entity.Property(e => e.NodeType).HasConversion(new EnumToStringConverter<ApprovalFlowNodeType>());

      });
      builder.Entity<ApprovalReadLogEntity>(entity =>
      {
        entity.ToTable("Approval_ReadLog");
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.HasKey(e => e.Id);
      });
    }

  }
}
