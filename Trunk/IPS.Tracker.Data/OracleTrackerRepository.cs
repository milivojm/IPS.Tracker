using IPS.Tracker.Domain;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System;

namespace IPS.Tracker.Data
{
    public class OracleTrackerRepository : DbContext, ITrackerRepository
    {
        public OracleTrackerRepository() : base("OracleDbContext")
        {
            Database.SetInitializer<OracleTrackerRepository>(null);
#if DEBUG
            Database.Log = log => System.Diagnostics.Debug.Write(log);
#endif
        }

        public DbSet<Defect> DefectsSet { get; set; }
        public DbSet<Worker> WorkersSet { get; set; }
        public DbSet<WorkOrder> WorkOrdersSet { get; set; }
        public DbSet<Release> ReleaseSet { get; set; }

        public IQueryable<Release> Releases
        {
            get
            {
                return ReleaseSet;
            }
        }
        
        public void AddRelease(Release release)
        {
            ReleaseSet.Add(release);
        } 

        public IQueryable<Defect> Defects
        {
            get
            {
                return DefectsSet;
            }
        }

        public IQueryable<WorkOrder> WorkOrders
        {
            get
            {
                return WorkOrdersSet;
            }
        }

        public IQueryable<Worker> Workers
        {
            get
            {
                return WorkersSet;
            }
        }

        public void AddDefect(Defect defect)
        {
            DefectsSet.Add(defect);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            string schema = new System.Data.SqlClient.SqlConnectionStringBuilder(Database.Connection.ConnectionString).UserID.ToUpper();
            modelBuilder.HasDefaultSchema(schema);

            #region Defect
            modelBuilder.Entity<Defect>().HasKey(d => d.Id);
            modelBuilder.Entity<Defect>().Property(d => d.Id).HasColumnName("ID").IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<Defect>().HasRequired(d => d.Assignee).WithMany().HasForeignKey(d => d.AssigneeId);
            modelBuilder.Entity<Defect>().Property(d => d.AssigneeId).HasColumnName("ASSIGNEE_ID").IsRequired();
            modelBuilder.Entity<Defect>().HasMany(Defect.ORMappings.Comments).WithRequired(dc => dc.Defect).HasForeignKey(d => d.DefectId);
            modelBuilder.Entity<Defect>().Property(d => d.DefectState).HasColumnName("DEFECT_STATE").IsRequired();
            modelBuilder.Entity<Defect>().Property(d => d.DefectFile).HasColumnName("DEFECT_FILE");
            modelBuilder.Entity<Defect>().Property(d => d.DefectFileType).HasColumnName("DEFECT_FILE_TYPE");
            modelBuilder.Entity<Defect>().Property(d => d.DefectDate).HasColumnName("DEFECT_DATE").IsRequired();
            modelBuilder.Entity<Defect>().Property(d => d.Description).HasColumnName("DESCRIPTION").IsRequired();
            modelBuilder.Entity<Defect>().HasMany(Defect.ORMappings.Followers).WithMany().Map(m =>
            {
                m.MapLeftKey("DEFECT_ID");
                m.MapRightKey("FOLLOWER_ID");
                m.ToTable("IPS_DEFECT_FOLLOWERS");
            });
            modelBuilder.Entity<Defect>().Property(d => d.Priority).HasColumnName("PRIORITY").IsRequired();
            modelBuilder.Entity<Defect>().Property(d => d.ReporterId).HasColumnName("REPORTER_ID").IsRequired();
            modelBuilder.Entity<Defect>().HasRequired(d => d.Reporter).WithMany().HasForeignKey(d => d.ReporterId);
            modelBuilder.Entity<Defect>().Property(d => d.SprintNo).HasColumnName("SPRINT_NO");
            modelBuilder.Entity<Defect>().Ignore(d => d.StateDescription);
            modelBuilder.Entity<Defect>().Property(d => d.Summary).HasColumnName("SUMMARY").IsRequired();
            modelBuilder.Entity<Defect>().HasOptional(d => d.WorkOrder).WithMany().HasForeignKey(d => d.WorkOrderId);
            modelBuilder.Entity<Defect>().Property(d => d.WorkOrderId).HasColumnName("WORK_ORDER_ID");
            modelBuilder.Entity<Defect>().Property(d => d.ReleaseId).HasColumnName("RELEASE_ID");
            modelBuilder.Entity<Defect>().ToTable("IPS_DEFECTS");
            #endregion

            #region Worker
            modelBuilder.Entity<Worker>().HasKey(w => w.Id);
            modelBuilder.Entity<Worker>().Property(w => w.Id).HasColumnName("ID").IsRequired();
            modelBuilder.Entity<Worker>().Property(w => w.Email).HasColumnName("EMAIL").IsRequired();
            modelBuilder.Entity<Worker>().Property(w => w.Name).HasColumnName("NAME").IsRequired();
            modelBuilder.Entity<Worker>().Property(w => w.Retired).HasColumnName("RETIRED").IsRequired();
            modelBuilder.Entity<Worker>().Property(w => w.TrackerAdmin).HasColumnName("TRACKER_ADMIN").IsRequired();
            modelBuilder.Entity<Worker>().Property(w => w.Username).HasColumnName("USERNAME").IsRequired();
            modelBuilder.Entity<Worker>().ToTable("IPS_WORKERS");
            #endregion

            #region DefectComment
            modelBuilder.Entity<DefectComment>().HasKey(dc => dc.Id);
            modelBuilder.Entity<DefectComment>().Property(dc => dc.Id).HasColumnName("ID").IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<DefectComment>().Property(dc => dc.CommentatorId).HasColumnName("COMMENTATOR_ID").IsRequired();
            modelBuilder.Entity<DefectComment>().HasRequired(dc => dc.Commentator).WithMany().HasForeignKey(dc => dc.CommentatorId);
            modelBuilder.Entity<DefectComment>().Property(dc => dc.CommentDate).HasColumnName("COMMENT_DATE").IsRequired();
            modelBuilder.Entity<DefectComment>().Property(dc => dc.DefectId).HasColumnName("DEFECT_ID").IsRequired();
            modelBuilder.Entity<DefectComment>().Property(dc => dc.Text).HasColumnName("TEXT").IsRequired();
            modelBuilder.Entity<DefectComment>().ToTable("IPS_DEFECT_COMMENTS");
            #endregion

            #region WorkOrder
            modelBuilder.Entity<WorkOrder>().HasKey(w => w.Id);
            modelBuilder.Entity<WorkOrder>().Property(w => w.Completed).HasColumnName("COMPLETED").IsRequired();
            modelBuilder.Entity<WorkOrder>().Property(w => w.Description).HasColumnName("DESCRIPTION");
            modelBuilder.Entity<WorkOrder>().Property(w => w.Id).HasColumnName("ID").IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<WorkOrder>().Ignore(w => w.Label);
            modelBuilder.Entity<WorkOrder>().Property(w => w.Leader).HasColumnName("LEADER").IsRequired();
            modelBuilder.Entity<WorkOrder>().Property(w => w.Name).HasColumnName("NAME").IsRequired();
            modelBuilder.Entity<WorkOrder>().Property(w => w.OrdinalNumber).HasColumnName("ORDINAL_NUMBER").IsRequired();
            modelBuilder.Entity<WorkOrder>().Property(w => w.Subnumber).HasColumnName("SUBNUMBER");
            modelBuilder.Entity<WorkOrder>().Property(w => w.Year).HasColumnName("YEAR").IsRequired();
            modelBuilder.Entity<WorkOrder>().ToTable("IPS_WORK_ORDERS");
            #endregion

            #region Release
            modelBuilder.Entity<Release>().HasKey(k => k.Id);
            modelBuilder.Entity<Release>().Property(k => k.Id).HasColumnName("ID").IsRequired();
            modelBuilder.Entity<Release>().Property(k => k.ReleaseVersion).HasColumnName("RELEASE_VERSION");
            modelBuilder.Entity<Release>().Property(k => k.ReleaseDate).HasColumnName("RELEASE_DATE");
            modelBuilder.Entity<Release>().ToTable("IPS_RELEASE");
            #endregion
        }

        public void Save()
        {
            SaveChanges();
        }
    }
}
