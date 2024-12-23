using Approval;
using Approval.Entities;
using CyberStone.Core.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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
        entity.Property(e => e.ConditionFields).HasJsonConversion();
        entity.Property(e => e.Applicants).HasJsonConversion();
        entity.Property(e => e.Workflow).HasJsonConversion();
        entity.Property(e => e.Fields).HasJsonConversion();
        entity.Property(e => e.DepartmentIds).HasJsonConversion();
        entity.Property(e => e.Group).HasConversion(new EnumToStringConverter<TemplateGroup>());
      });
      builder.Entity<ApprovalItemEntity>(entity =>
      {
        entity.ToTable("Approval_Item");
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Content).HasJsonConversion();
        entity.Property(e => e.FinalFiles).HasJsonConversion();
        entity.Property(e => e.VerifiedFiles).HasJsonConversion();
        entity.HasMany(e => e.Nodes).WithOne(e => e.Item).HasForeignKey(x => x.ItemId);
        entity.HasOne(e => e.Template).WithMany(e => e.Items).HasForeignKey(x => x.TemplateId);
        entity.Property(e => e.Status).HasConversion(new EnumToStringConverter<ApprovalItemStatus>());

        entity.Property(e => e.Purview).HasJsonConversion();

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
        entity.Property(e => e.Comments).HasJsonConversion();
        entity.Property(e => e.Attachments).HasJsonConversion();
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
