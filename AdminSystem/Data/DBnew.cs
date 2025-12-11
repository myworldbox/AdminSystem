using System;
using System.Collections.Generic;
using AdminSystem.Models.Tables_new;
using Microsoft.EntityFrameworkCore;

namespace AdminSystem.Data;

public partial class DBnew : DbContext
{
    public DBnew()
    {
    }

    public DBnew(DbContextOptions<DBnew> options)
        : base(options)
    {
    }

    public virtual DbSet<TBL_BANK> TBL_BANK { get; set; }

    public virtual DbSet<TBL_CHILD> TBL_CHILD { get; set; }

    public virtual DbSet<TBL_CNTR> TBL_CNTR { get; set; }

    public virtual DbSet<TBL_CNTR_TX> TBL_CNTR_TX { get; set; }

    public virtual DbSet<TBL_PTCNTR> TBL_PTCNTR { get; set; }

    public virtual DbSet<TBL_PYRLPROC_CTRL> TBL_PYRLPROC_CTRL { get; set; }

    public virtual DbSet<TBL_PYRL_TX> TBL_PYRL_TX { get; set; }

    public virtual DbSet<TBL_STAFF> TBL_STAFF { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseOracle("User Id=hrdb03;Password=HRdev#3;data source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=10.0.128.108)(PORT=2521))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME = PDB19C1)));");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasDefaultSchema("HRDB03")
            .UseCollation("USING_NLS_COMP");

        modelBuilder.Entity<TBL_BANK>(entity =>
        {
            entity.HasKey(e => e.BNK_CODE).HasName("PK_BNK_CODE");

            entity.Property(e => e.BNK_CODE)
                .HasMaxLength(3)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.BNK_NAME)
                .HasMaxLength(60)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.TIMESTAMP)
                .HasDefaultValueSql("SYSDATE ")
                .HasColumnType("DATE");
        });

        modelBuilder.Entity<TBL_CHILD>(entity =>
        {
            entity.HasKey(e => new { e.CHD_STFNO, e.CHD_CHILDNO }).HasName("PK_CHD_STFNO");

            entity.Property(e => e.CHD_STFNO)
                .HasMaxLength(6)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CHD_CHILDNO)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER(1)");
            entity.Property(e => e.CHD_ACTN)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.CHD_ACTNDATE).HasColumnType("DATE");
            entity.Property(e => e.CHD_ACTNUSER)
                .HasMaxLength(20)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.CHD_ACTNUSER_SSO)
                .HasMaxLength(20)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.CHD_DOB)
                .ValueGeneratedOnAdd()
                .HasColumnType("DATE");
            entity.Property(e => e.CHD_HEALTH)
                .HasMaxLength(1)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CHD_ID_ISSCNTY)
                .HasMaxLength(20)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.CHD_ID_NO)
                .HasMaxLength(20)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.CHD_ID_TYP)
                .HasMaxLength(1)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CHD_MARITAL_STATUS)
                .HasMaxLength(1)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CHD_NAME)
                .HasMaxLength(30)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.CHD_SEX)
                .HasMaxLength(1)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.TIMESTAMP)
                .HasDefaultValueSql("SYSDATE ")
                .HasColumnType("DATE");

            entity.HasOne(d => d.CHD_STFNONavigation).WithMany(p => p.TBL_CHILD)
                .HasForeignKey(d => d.CHD_STFNO)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CHD_STFNO");
        });

        modelBuilder.Entity<TBL_CNTR>(entity =>
        {
            entity.HasKey(e => new { e.CNT_CNTR_CTR, e.CNT_CNTR_YR, e.CNT_CNTR_SQN }).HasName("PK_CNT_CNTR_NO");

            entity.HasIndex(e => e.CNT_STFNO, "IND_CNT_STFNO");

            entity.Property(e => e.CNT_CNTR_CTR)
                .HasMaxLength(6)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CNT_CNTR_YR)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CNT_CNTR_SQN)
                .HasMaxLength(4)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CNT_2NDSCH_CNTR)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CNT_CESS_CODE)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CNT_CESS_DATE).HasColumnType("DATE");
            entity.Property(e => e.CNT_CNTR_ACTN)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.CNT_CNTR_END_DATE).HasColumnType("DATE");
            entity.Property(e => e.CNT_CNTR_START_DATE).HasColumnType("DATE");
            entity.Property(e => e.CNT_CNTR_STATUS)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CNT_CURPST_DATE).HasColumnType("DATE");
            entity.Property(e => e.CNT_CURPT)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CNT_CURRNK_DATE).HasColumnType("DATE");
            entity.Property(e => e.CNT_CUR_PF_PVDR)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CNT_DEFREND_DATE).HasColumnType("DATE");
            entity.Property(e => e.CNT_EMPPKG)
                .HasMaxLength(6)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CNT_EMPTERMS)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CNT_EMP_DATE).HasColumnType("DATE");
            entity.Property(e => e.CNT_FR_DATE).HasColumnType("DATE");
            entity.Property(e => e.CNT_FSTAPPT_DATE).HasColumnType("DATE");
            entity.Property(e => e.CNT_FUNDSRC)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CNT_GRAT_PERC).HasPrecision(2);
            entity.Property(e => e.CNT_INCRDATE).HasColumnType("DATE");
            entity.Property(e => e.CNT_INCR_CHG_CODE)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CNT_INCR_STATUS)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CNT_MAXPT)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CNT_MAXPT_DATE).HasColumnType("DATE");
            entity.Property(e => e.CNT_MPFSRV_STARTDATE).HasColumnType("DATE");
            entity.Property(e => e.CNT_MPF_EESC_DATE).HasColumnType("DATE");
            entity.Property(e => e.CNT_MPF_EESC_PERC).HasPrecision(2);
            entity.Property(e => e.CNT_MPF_LB_EEC_FLG)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CNT_MPF_LSTC_DATE).HasColumnType("DATE");
            entity.Property(e => e.CNT_MPF_NHAN_FLG)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValueSql("null")
                .IsFixedLength();
            entity.Property(e => e.CNT_MPF_PAYCTR)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CNT_MPF_STATUS)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CNT_MPF_TW_1STDATE).HasColumnType("DATE");
            entity.Property(e => e.CNT_MPF_TW_2NDDATE).HasColumnType("DATE");
            entity.Property(e => e.CNT_MPF_TW_FLG)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CNT_MPF_UB_EEC_FLG)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CNT_MPF_UB_ERC_FLG)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CNT_NPS_DATE).HasColumnType("DATE");
            entity.Property(e => e.CNT_NPS_NOTE)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CNT_OMITPT1)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CNT_OMITPT2)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CNT_OMITPT3)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CNT_OMITPT4)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CNT_OMITPT5)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CNT_OMITPT6)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CNT_OMITPT7)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CNT_OMITPT8)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CNT_PAYSCALE)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CNT_PAY_LVL)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.CNT_PF10DATE).HasColumnType("DATE");
            entity.Property(e => e.CNT_PF15DATE).HasColumnType("DATE");
            entity.Property(e => e.CNT_PF5DATE).HasColumnType("DATE");
            entity.Property(e => e.CNT_PFJOIN_DATE).HasColumnType("DATE");
            entity.Property(e => e.CNT_PF_CTYP)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CNT_PF_SCHEM)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CNT_POST)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.CNT_POST_FRACT)
                .HasDefaultValueSql("1")
                .HasColumnType("NUMBER(6,4)");
            entity.Property(e => e.CNT_PRJEND_DATE).HasColumnType("DATE");
            entity.Property(e => e.CNT_PROBEND_DATE).HasColumnType("DATE");
            entity.Property(e => e.CNT_PSR_DATE).HasColumnType("DATE");
            entity.Property(e => e.CNT_RANK)
                .HasMaxLength(6)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CNT_REC_STATUS)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CNT_REFNO)
                .HasMaxLength(8)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CNT_REMARKS).IsUnicode(false);
            entity.Property(e => e.CNT_REMINDER_DATE).HasColumnType("DATE");
            entity.Property(e => e.CNT_REMINDER_NOTE)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CNT_SALAMT).HasColumnType("NUMBER(15,2)");
            entity.Property(e => e.CNT_SECONDMENT_END_DATE).HasColumnType("DATE");
            entity.Property(e => e.CNT_SERV_CTR)
                .HasMaxLength(6)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CNT_SR_NO)
                .HasMaxLength(8)
                .IsUnicode(false);
            entity.Property(e => e.CNT_STFNO)
                .HasMaxLength(6)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CNT_STFTYP)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CNT_STF_CATG)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.CNT_STF_GRP)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.CNT_STOPEND_DATE).HasColumnType("DATE");
            entity.Property(e => e.CNT_TO_DATE).HasColumnType("DATE");
            entity.Property(e => e.CNT_TRIALEND_DATE).HasColumnType("DATE");
            entity.Property(e => e.TIMESTAMP)
                .HasDefaultValueSql("SYSDATE ")
                .HasColumnType("DATE");

            entity.HasOne(d => d.CNT_STFNONavigation).WithMany(p => p.TBL_CNTR)
                .HasForeignKey(d => d.CNT_STFNO)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CNT_STFNO");
        });

        modelBuilder.Entity<TBL_CNTR_TX>(entity =>
        {
            entity.HasKey(e => e.CTT_REFNO).HasName("PK_CTT_REFNO");

            entity.HasIndex(e => e.CTT_STFNO, "IND_CTT_STFNO");

            entity.Property(e => e.CTT_REFNO)
                .HasMaxLength(8)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CTT_2NDSCH_CNTR)
                .HasMaxLength(1)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CTT_APRV_BY)
                .HasMaxLength(10)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.CTT_APRV_DATE)
                .ValueGeneratedOnAdd()
                .HasColumnType("DATE");
            entity.Property(e => e.CTT_CESS_CODE)
                .HasMaxLength(2)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CTT_CESS_DATE)
                .ValueGeneratedOnAdd()
                .HasColumnType("DATE");
            entity.Property(e => e.CTT_CNTR_ACTN)
                .HasMaxLength(10)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.CTT_CNTR_CTR)
                .HasMaxLength(6)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CTT_CNTR_END_DATE)
                .ValueGeneratedOnAdd()
                .HasColumnType("DATE");
            entity.Property(e => e.CTT_CNTR_SQN)
                .HasMaxLength(4)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CTT_CNTR_START_DATE)
                .ValueGeneratedOnAdd()
                .HasColumnType("DATE");
            entity.Property(e => e.CTT_CNTR_STATUS)
                .HasMaxLength(1)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CTT_CNTR_YR)
                .HasMaxLength(2)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CTT_COMP_GEN)
                .HasMaxLength(2)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CTT_CURPST_DATE)
                .ValueGeneratedOnAdd()
                .HasColumnType("DATE");
            entity.Property(e => e.CTT_CURPT)
                .HasMaxLength(2)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CTT_CURRNK_DATE)
                .ValueGeneratedOnAdd()
                .HasColumnType("DATE");
            entity.Property(e => e.CTT_CUR_PF_PVDR)
                .HasMaxLength(2)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CTT_DCHK_BY)
                .HasMaxLength(10)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.CTT_DCHK_DATE)
                .ValueGeneratedOnAdd()
                .HasColumnType("DATE");
            entity.Property(e => e.CTT_DEFREND_DATE)
                .ValueGeneratedOnAdd()
                .HasColumnType("DATE");
            entity.Property(e => e.CTT_EMPPKG)
                .HasMaxLength(6)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CTT_EMPTERMS)
                .HasMaxLength(2)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CTT_EMP_DATE).HasColumnType("DATE");
            entity.Property(e => e.CTT_ENTRY_BY)
                .HasMaxLength(10)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.CTT_ENTRY_DATE)
                .ValueGeneratedOnAdd()
                .HasColumnType("DATE");
            entity.Property(e => e.CTT_FR_DATE)
                .ValueGeneratedOnAdd()
                .HasColumnType("DATE");
            entity.Property(e => e.CTT_FSTAPPT_DATE)
                .ValueGeneratedOnAdd()
                .HasColumnType("DATE");
            entity.Property(e => e.CTT_FUNDSRC)
                .HasMaxLength(3)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CTT_GRAT_PERC)
                .HasPrecision(2)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.CTT_INCRDATE)
                .ValueGeneratedOnAdd()
                .HasColumnType("DATE");
            entity.Property(e => e.CTT_INCR_CHG_CODE)
                .HasMaxLength(2)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CTT_INCR_STATUS)
                .HasMaxLength(1)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CTT_MAXPT)
                .HasMaxLength(2)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CTT_MAXPT_DATE)
                .ValueGeneratedOnAdd()
                .HasColumnType("DATE");
            entity.Property(e => e.CTT_MPF_EESC_DATE)
                .ValueGeneratedOnAdd()
                .HasColumnType("DATE");
            entity.Property(e => e.CTT_MPF_EESC_PERC)
                .HasPrecision(2)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.CTT_MPF_LB_EEC_FLG)
                .HasMaxLength(1)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CTT_MPF_LSTC_DATE)
                .ValueGeneratedOnAdd()
                .HasColumnType("DATE");
            entity.Property(e => e.CTT_MPF_NHAN_FLG)
                .HasMaxLength(1)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CTT_MPF_PAYCTR)
                .HasMaxLength(3)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CTT_MPF_STATUS)
                .HasMaxLength(1)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CTT_MPF_TW_1STDATE)
                .ValueGeneratedOnAdd()
                .HasColumnType("DATE");
            entity.Property(e => e.CTT_MPF_TW_2NDDATE)
                .ValueGeneratedOnAdd()
                .HasColumnType("DATE");
            entity.Property(e => e.CTT_MPF_TW_FLG)
                .HasMaxLength(1)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CTT_MPF_UB_EEC_FLG)
                .HasMaxLength(1)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CTT_MPF_UB_ERC_FLG)
                .HasMaxLength(1)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CTT_MRK_DELE)
                .HasMaxLength(1)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("'N' ")
                .IsFixedLength();
            entity.Property(e => e.CTT_NPS_DATE)
                .ValueGeneratedOnAdd()
                .HasColumnType("DATE");
            entity.Property(e => e.CTT_NPS_NOTE)
                .HasMaxLength(100)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.CTT_OMITPT1)
                .HasMaxLength(2)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CTT_OMITPT2)
                .HasMaxLength(2)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CTT_OMITPT3)
                .HasMaxLength(2)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CTT_OMITPT4)
                .HasMaxLength(2)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CTT_OMITPT5)
                .HasMaxLength(2)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CTT_OMITPT6)
                .HasMaxLength(2)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CTT_OMITPT7)
                .HasMaxLength(2)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CTT_OMITPT8)
                .HasMaxLength(2)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CTT_OVRWRI_RMK_OPT)
                .HasMaxLength(1)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CTT_PAYR_MTH)
                .ValueGeneratedOnAdd()
                .HasColumnType("DATE");
            entity.Property(e => e.CTT_PAYSCALE)
                .HasMaxLength(1)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CTT_PAY_LVL)
                .HasMaxLength(30)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.CTT_PF10DATE)
                .ValueGeneratedOnAdd()
                .HasColumnType("DATE");
            entity.Property(e => e.CTT_PF15DATE)
                .ValueGeneratedOnAdd()
                .HasColumnType("DATE");
            entity.Property(e => e.CTT_PF5DATE)
                .ValueGeneratedOnAdd()
                .HasColumnType("DATE");
            entity.Property(e => e.CTT_PF_CTYP)
                .HasMaxLength(1)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CTT_PF_JOINDATE)
                .ValueGeneratedOnAdd()
                .HasColumnType("DATE");
            entity.Property(e => e.CTT_PF_SCHEM)
                .HasMaxLength(1)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CTT_POST)
                .HasMaxLength(40)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.CTT_POST_FRACT)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("1")
                .HasColumnType("NUMBER(6,4)");
            entity.Property(e => e.CTT_PRJEND_DATE)
                .ValueGeneratedOnAdd()
                .HasColumnType("DATE");
            entity.Property(e => e.CTT_PROBEND_DATE)
                .ValueGeneratedOnAdd()
                .HasColumnType("DATE");
            entity.Property(e => e.CTT_PROC_BY)
                .HasMaxLength(10)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.CTT_PROC_DATE)
                .ValueGeneratedOnAdd()
                .HasColumnType("DATE");
            entity.Property(e => e.CTT_PSR_DATE)
                .ValueGeneratedOnAdd()
                .HasColumnType("DATE");
            entity.Property(e => e.CTT_RANK)
                .HasMaxLength(6)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CTT_REC_STATUS)
                .HasMaxLength(1)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CTT_REMARKS)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.CTT_REMINDER_DATE)
                .ValueGeneratedOnAdd()
                .HasColumnType("DATE");
            entity.Property(e => e.CTT_REMINDER_NOTE)
                .HasMaxLength(100)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.CTT_REVERT)
                .HasMaxLength(1)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("'N' ")
                .IsFixedLength();
            entity.Property(e => e.CTT_REVT_BY)
                .HasMaxLength(10)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.CTT_REVT_DATE)
                .ValueGeneratedOnAdd()
                .HasColumnType("DATE");
            entity.Property(e => e.CTT_SALAMT)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER(15,2)");
            entity.Property(e => e.CTT_SECONDMENT_END_DATE)
                .ValueGeneratedOnAdd()
                .HasColumnType("DATE");
            entity.Property(e => e.CTT_SERV_CTR)
                .HasMaxLength(6)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CTT_SR_NO)
                .HasMaxLength(8)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.CTT_STFNO)
                .HasMaxLength(6)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CTT_STFTYP)
                .HasMaxLength(3)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CTT_STF_CATG)
                .HasMaxLength(3)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.CTT_STF_GRP)
                .HasMaxLength(2)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.CTT_STOPEND_DATE)
                .ValueGeneratedOnAdd()
                .HasColumnType("DATE");
            entity.Property(e => e.CTT_TO_DATE)
                .ValueGeneratedOnAdd()
                .HasColumnType("DATE");
            entity.Property(e => e.CTT_TRIALEND_DATE)
                .ValueGeneratedOnAdd()
                .HasColumnType("DATE");
            entity.Property(e => e.CTT_TX_TYP)
                .HasMaxLength(1)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.CTT_VRFY_BY)
                .HasMaxLength(10)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.CTT_VRFY_DATE)
                .ValueGeneratedOnAdd()
                .HasColumnType("DATE");
            entity.Property(e => e.CTT_WITHDRAW_RSGN)
                .HasMaxLength(1)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.TIMESTAMP)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("SYSDATE ")
                .HasColumnType("DATE");
        });

        modelBuilder.Entity<TBL_PTCNTR>(entity =>
        {
            entity.HasKey(e => new { e.PCT_CNTR_CTR, e.PCT_CNTR_SQN, e.PCT_CNTR_YR }).HasName("PK_PCT_CNTR_NO");

            entity.ToTable("TBL_PTCNTR", "PTDB03");

            entity.Property(e => e.PCT_CNTR_CTR)
                .HasMaxLength(6)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.PCT_CNTR_SQN)
                .HasMaxLength(4)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.PCT_CNTR_YR)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.PCT_APRV_BY)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.PCT_APRV_BY_SSO)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PCT_APRV_DATE).HasColumnType("DATE");
            entity.Property(e => e.PCT_CESS_DATE).HasColumnType("DATE");
            entity.Property(e => e.PCT_CNTR_CPOST).HasMaxLength(20);
            entity.Property(e => e.PCT_CNTR_END).HasColumnType("DATE");
            entity.Property(e => e.PCT_CNTR_POST)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.PCT_CNTR_START).HasColumnType("DATE");
            entity.Property(e => e.PCT_DAY_AMT).HasColumnType("NUMBER(15,2)");
            entity.Property(e => e.PCT_DEL_FLG)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValueSql("'N' ")
                .IsFixedLength();
            entity.Property(e => e.PCT_EMPNO).HasPrecision(7);
            entity.Property(e => e.PCT_EMP_REASON)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.PCT_EMP_STATUS)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.PCT_ENTRY_BY)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.PCT_ENTRY_BY_SSO)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PCT_ENTRY_DATE).HasColumnType("DATE");
            entity.Property(e => e.PCT_HRB_CHK1_BY)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.PCT_HRB_CHK1_BY_SSO)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PCT_HRB_CHK1_DATE).HasColumnType("DATE");
            entity.Property(e => e.PCT_HRB_CHK2_BY)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.PCT_HRB_CHK2_BY_SSO)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PCT_HRB_CHK2_DATE).HasColumnType("DATE");
            entity.Property(e => e.PCT_HR_AMT).HasColumnType("NUMBER(15,2)");
            entity.Property(e => e.PCT_IMP_WKR)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.PCT_MTH_AMT).HasColumnType("NUMBER(15,2)");
            entity.Property(e => e.PCT_PAYM_FLG)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.PCT_POSTID)
                .HasMaxLength(6)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.PCT_POST_FRACT1).HasColumnType("NUMBER(6,4)");
            entity.Property(e => e.PCT_POST_FRACT2).HasPrecision(2);
            entity.Property(e => e.PCT_QUALIFY)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.PCT_REMARKS)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PCT_REVERT)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValueSql("'N' ")
                .IsFixedLength();
            entity.Property(e => e.PCT_REVERT_AFT_CUTOFF)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValueSql("'N' ")
                .IsFixedLength();
            entity.Property(e => e.PCT_REVERT_BY)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.PCT_REVERT_BY_SSO)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PCT_REVERT_DATE).HasColumnType("DATE");
            entity.Property(e => e.PCT_SRV_CTR)
                .HasMaxLength(6)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.PCT_STATUS)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.PCT_STD_POST)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.PCT_STFNO)
                .HasMaxLength(6)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.PCT_STF_LAST_TS).HasColumnType("DATE");
            entity.Property(e => e.PCT_SUB_TECHR)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.PCT_SUPL_TECHR)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.PCT_WATCHMAN)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.TIMESTAMP)
                .HasDefaultValueSql("SYSDATE ")
                .HasColumnType("DATE");
        });

        modelBuilder.Entity<TBL_PYRLPROC_CTRL>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.PPC_BATCH_NO).HasPrecision(2);
            entity.Property(e => e.PPC_BATCH_STATUS)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.PPC_CNTR_INPROGRESS)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.PPC_FPD_INPROGRESS)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.PPC_LCK_STF_BNKAC)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.PPC_LVD_INPROGRESS)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.PPC_NCL_INPROGRESS)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.PPC_OPD_INPROGRESS)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.PPC_OT_INPROGRESS)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
        });

        modelBuilder.Entity<TBL_PYRL_TX>(entity =>
        {
            entity.HasKey(e => e.PRX_SQ).HasName("PK_PRX_SQ");

            entity.HasIndex(e => new { e.PRX_CNTR_CTR, e.PRX_CNTR_YR, e.PRX_CNTR_SQN }, "IDX_PRX_CNTRNO");

            entity.Property(e => e.PRX_SQ).HasPrecision(10);
            entity.Property(e => e.PRX_BATCH_NO)
                .HasPrecision(2)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.PRX_CHRG_AC)
                .HasMaxLength(9)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.PRX_CHRG_CTR)
                .HasMaxLength(6)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.PRX_CNTR_CTR)
                .HasMaxLength(6)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.PRX_CNTR_SQN)
                .HasMaxLength(4)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.PRX_CNTR_YR)
                .HasMaxLength(2)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.PRX_EEPF_TOTAL)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER(15,2)");
            entity.Property(e => e.PRX_ERPF_TOTAL)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("0 ")
                .HasColumnType("NUMBER(15,2)");
            entity.Property(e => e.PRX_FRACT)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER(6,4)");
            entity.Property(e => e.PRX_FR_DATE)
                .ValueGeneratedOnAdd()
                .HasColumnType("DATE");
            entity.Property(e => e.PRX_GROSS_AMT)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER(15,2)");
            entity.Property(e => e.PRX_L_AMT)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER(15,2)");
            entity.Property(e => e.PRX_L_PAYSCALE)
                .HasMaxLength(1)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.PRX_L_PT)
                .HasMaxLength(2)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.PRX_L_SCLAMT)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER(15,2)");
            entity.Property(e => e.PRX_MRK_DELE)
                .HasMaxLength(1)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("'N' ")
                .IsFixedLength();
            entity.Property(e => e.PRX_NET_AMT)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER(15,2)");
            entity.Property(e => e.PRX_PAYM_CODE)
                .HasMaxLength(5)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.PRX_PFCHRG_AC)
                .HasMaxLength(9)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.PRX_POST_FRACT)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER(6,4)");
            entity.Property(e => e.PRX_PYRL_MTH)
                .ValueGeneratedOnAdd()
                .HasColumnType("DATE");
            entity.Property(e => e.PRX_REVERT)
                .HasMaxLength(1)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.PRX_STFNO)
                .HasMaxLength(6)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.PRX_STF_TYP)
                .HasMaxLength(3)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.PRX_TO_DATE)
                .ValueGeneratedOnAdd()
                .HasColumnType("DATE");
            entity.Property(e => e.PRX_TX_REFNO)
                .HasMaxLength(8)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.PRX_U_AMT)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER(15,2)");
            entity.Property(e => e.PRX_U_PAYSCALE)
                .HasMaxLength(1)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.PRX_U_PT)
                .HasMaxLength(2)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.PRX_U_SCLAMT)
                .ValueGeneratedOnAdd()
                .HasColumnType("NUMBER(15,2)");
            entity.Property(e => e.TIMESTAMP)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("SYSDATE ")
                .HasColumnType("DATE");
        });

        modelBuilder.Entity<TBL_STAFF>(entity =>
        {
            entity.HasKey(e => e.STF_NO).HasName("PK_STF_NO");

            entity.HasIndex(e => e.STF_HKID, "AK_STF_HKID").IsUnique();

            entity.HasIndex(e => e.STF_PP_NO, "AK_STF_PP_NO").IsUnique();

            entity.Property(e => e.STF_NO)
                .HasMaxLength(6)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.STF_ACTN)
                .HasMaxLength(10)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.STF_ACTNDATE)
                .HasPrecision(7)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.STF_ACTNUSER)
                .HasMaxLength(20)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.STF_ACTNUSER_SSO)
                .HasMaxLength(20)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.STF_AC_BNK_CODE)
                .HasMaxLength(3)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.STF_AC_CODE)
                .HasMaxLength(20)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.STF_ADDR1)
                .HasMaxLength(30)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.STF_ADDR2)
                .HasMaxLength(30)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.STF_ADDR3)
                .HasMaxLength(30)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.STF_ADDR4)
                .HasMaxLength(30)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.STF_ADDR_AREA)
                .HasMaxLength(1)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.STF_CNAME)
                .HasMaxLength(6)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.STF_DAD_HEALTH)
                .HasMaxLength(1)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.STF_DAD_HKID)
                .HasMaxLength(12)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.STF_DAD_NAME)
                .HasMaxLength(30)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.STF_DAD_PP_ISCNTY)
                .HasMaxLength(20)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.STF_DAD_PP_NO)
                .HasMaxLength(20)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.STF_DOB)
                .ValueGeneratedOnAdd()
                .HasColumnType("DATE");
            entity.Property(e => e.STF_EMAIL)
                .HasMaxLength(100)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.STF_EMPVISA_XDATE)
                .ValueGeneratedOnAdd()
                .HasColumnType("DATE");
            entity.Property(e => e.STF_GIVENNAME)
                .HasMaxLength(75)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.STF_HKID)
                .HasMaxLength(12)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.STF_MARITAL_STAT)
                .HasMaxLength(1)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.STF_MOM_HEALTH)
                .HasMaxLength(1)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.STF_MOM_HKID)
                .HasMaxLength(12)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.STF_MOM_NAME)
                .HasMaxLength(30)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.STF_MOM_PP_ISCNTY)
                .HasMaxLength(20)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.STF_MOM_PP_NO)
                .HasMaxLength(20)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.STF_NAME)
                .HasMaxLength(75)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.STF_NAT)
                .HasMaxLength(10)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.STF_PERMITNO)
                .HasMaxLength(20)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.STF_PERMIT_XDATE)
                .ValueGeneratedOnAdd()
                .HasColumnType("DATE");
            entity.Property(e => e.STF_PHONE1)
                .HasMaxLength(8)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.STF_PHONE1AREACODE)
                .HasMaxLength(4)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.STF_PHONE2)
                .HasMaxLength(8)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.STF_PP_ISCNTY)
                .HasMaxLength(20)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.STF_PP_NO)
                .HasMaxLength(20)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.STF_SEX)
                .HasMaxLength(1)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.STF_SPS_HEALTH)
                .HasMaxLength(1)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.STF_SPS_HKID)
                .HasMaxLength(12)
                .IsUnicode(false)
                .ValueGeneratedOnAdd()
                .IsFixedLength();
            entity.Property(e => e.STF_SPS_NAME)
                .HasMaxLength(30)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.STF_SPS_PP_ISCNTY)
                .HasMaxLength(20)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.STF_SPS_PP_NO)
                .HasMaxLength(20)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.STF_SURNAME)
                .HasMaxLength(75)
                .IsUnicode(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.TIMESTAMP)
                .HasPrecision(7)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("SYSTIMESTAMP\n");

            entity.HasOne(d => d.STF_AC_BNK_CODENavigation).WithMany(p => p.TBL_STAFF)
                .HasForeignKey(d => d.STF_AC_BNK_CODE)
                .HasConstraintName("AK_STF_AC_BNK_CODE");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
