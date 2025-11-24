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
