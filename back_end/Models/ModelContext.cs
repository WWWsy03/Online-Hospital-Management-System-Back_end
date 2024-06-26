﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace back_end.Models
{
    public partial class ModelContext : DbContext
    {
        public ModelContext()
        {
        }

        public ModelContext(DbContextOptions<ModelContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Administrator> Administrators { get; set; } = null!;
        public virtual DbSet<Chatrecord> Chatrecords { get; set; } = null!;
        public virtual DbSet<ConsultationInfo> ConsultationInfos { get; set; } = null!;
        public virtual DbSet<ConsultingRoom> ConsultingRooms { get; set; } = null!;
        public virtual DbSet<Department2> Department2s { get; set; } = null!;
        public virtual DbSet<Doctor> Doctors { get; set; } = null!;
        public virtual DbSet<LeaveApplication> LeaveApplications { get; set; } = null!;
        public virtual DbSet<MedicineDescription> MedicineDescriptions { get; set; } = null!;
        public virtual DbSet<MedicineOut> MedicineOuts { get; set; } = null!;
        public virtual DbSet<MedicinePurchase> MedicinePurchases { get; set; } = null!;
        public virtual DbSet<MedicineSell> MedicineSells { get; set; } = null!;
        public virtual DbSet<MedicineStock> MedicineStocks { get; set; } = null!;
        public virtual DbSet<OutpatientOrder> OutpatientOrders { get; set; } = null!;
        public virtual DbSet<Patient> Patients { get; set; } = null!;
        public virtual DbSet<Prescription> Prescriptions { get; set; } = null!;
        public virtual DbSet<PrescriptionMedicine> PrescriptionMedicines { get; set; } = null!;
        public virtual DbSet<Registration> Registrations { get; set; } = null!;
        public virtual DbSet<Template> Templates { get; set; } = null!;
        public virtual DbSet<TreatmentFeedback> TreatmentFeedbacks { get; set; } = null!;
        public virtual DbSet<TreatmentRecord> TreatmentRecords { get; set; } = null!;
        public virtual DbSet<TreatmentRecord2> TreatmentRecord2s { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseOracle("Data Source=124.223.143.21/xe;Password=TEST_PASSWORD;User ID=TEST_ACCOUNT;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("TEST_ACCOUNT");

            modelBuilder.Entity<Administrator>(entity =>
            {
                entity.ToTable("ADMINISTRATOR");

                entity.Property(e => e.AdministratorId)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("ADMINISTRATOR_ID");

                entity.Property(e => e.Birthdate)
                    .HasColumnType("DATE")
                    .HasColumnName("BIRTHDATE");

                entity.Property(e => e.Contact)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("CONTACT");

                entity.Property(e => e.Gender)
                    .HasPrecision(1)
                    .HasColumnName("GENDER");

                entity.Property(e => e.Name)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("NAME");

                entity.Property(e => e.Password)
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasColumnName("PASSWORD");
            });

            modelBuilder.Entity<Chatrecord>(entity =>
            {
                entity.HasKey(e => new { e.DoctorId, e.PatientId, e.Timestamp })
                    .HasName("CHATRECORD_PK");

                entity.ToTable("CHATRECORD");

                entity.Property(e => e.DoctorId)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("DOCTOR_ID");

                entity.Property(e => e.PatientId)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("PATIENT_ID");

                entity.Property(e => e.Timestamp)
                    .HasPrecision(6)
                    .HasColumnName("TIMESTAMP");

                entity.Property(e => e.Message)
                    .HasMaxLength(2000)
                    .IsUnicode(false)
                    .HasColumnName("MESSAGE");

                entity.Property(e => e.ReadStatus)
                    .HasColumnType("NUMBER")
                    .HasColumnName("READ_STATUS");

                entity.Property(e => e.Recordid)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("RECORDID");

                entity.Property(e => e.SenderType)
                    .HasColumnType("NUMBER")
                    .HasColumnName("SENDER_TYPE");

                entity.HasOne(d => d.Doctor)
                    .WithMany(p => p.Chatrecords)
                    .HasForeignKey(d => d.DoctorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("CHATRECORD_FK1");

                entity.HasOne(d => d.Patient)
                    .WithMany(p => p.Chatrecords)
                    .HasForeignKey(d => d.PatientId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("CHATRECORD_FK2");
            });

            modelBuilder.Entity<ConsultationInfo>(entity =>
            {
                entity.HasKey(e => new { e.DoctorId, e.ClinicName, e.DateTime, e.Period })
                    .HasName("CONSULTATION_INFO_PK");

                entity.ToTable("CONSULTATION_INFO");

                entity.Property(e => e.DoctorId)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("DOCTOR_ID");

                entity.Property(e => e.ClinicName)
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasColumnName("CLINIC_NAME");

                entity.Property(e => e.DateTime)
                    .HasColumnType("DATE")
                    .HasColumnName("DATE_TIME");

                entity.Property(e => e.Period)
                    .HasColumnType("NUMBER(38)")
                    .HasColumnName("PERIOD");

                entity.HasOne(d => d.ClinicNameNavigation)
                    .WithMany(p => p.ConsultationInfos)
                    .HasForeignKey(d => d.ClinicName)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("CONSULTATION_INFO_CONSULT_FK1");

                entity.HasOne(d => d.Doctor)
                    .WithMany(p => p.ConsultationInfos)
                    .HasForeignKey(d => d.DoctorId)
                    .HasConstraintName("CONSULTATION_INFO_DOCTOR_FK1");
            });

            modelBuilder.Entity<ConsultingRoom>(entity =>
            {
                entity.HasKey(e => e.ConsultingRoomName)
                    .HasName("CONSULTING_ROOM_PK");

                entity.ToTable("CONSULTING_ROOM");

                entity.Property(e => e.ConsultingRoomName)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("CONSULTING_ROOM_NAME");

                entity.Property(e => e.ConsultantCapacity)
                    .HasColumnType("NUMBER(38)")
                    .HasColumnName("CONSULTANT_CAPACITY");
            });

            modelBuilder.Entity<Department2>(entity =>
            {
                entity.HasKey(e => e.DepartmentName)
                    .HasName("DEPARTMENT2_PK");

                entity.ToTable("DEPARTMENT2");

                entity.Property(e => e.DepartmentName)
                    .HasMaxLength(80)
                    .IsUnicode(false)
                    .HasColumnName("DEPARTMENT_NAME");

                entity.Property(e => e.DepartmentDescription)
                    .HasMaxLength(800)
                    .IsUnicode(false)
                    .HasColumnName("DEPARTMENT_DESCRIPTION");
            });

            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.ToTable("DOCTOR");

                entity.Property(e => e.DoctorId)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("DOCTOR_ID");

                entity.Property(e => e.Birthdate)
                    .HasColumnType("DATE")
                    .HasColumnName("BIRTHDATE");

                entity.Property(e => e.Contact)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("CONTACT");

                entity.Property(e => e.Gender)
                    .HasPrecision(1)
                    .HasColumnName("GENDER");

                entity.Property(e => e.Name)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("NAME");

                entity.Property(e => e.Password)
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasColumnName("PASSWORD");

                entity.Property(e => e.Photourl)
                    .HasMaxLength(600)
                    .IsUnicode(false)
                    .HasColumnName("PHOTOURL");

                entity.Property(e => e.SecondaryDepartment)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("SECONDARY_DEPARTMENT");

                entity.Property(e => e.Skilledin)
                    .HasMaxLength(1000)
                    .IsUnicode(false)
                    .HasColumnName("SKILLEDIN");

                entity.Property(e => e.Title)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("TITLE");
            });

            modelBuilder.Entity<LeaveApplication>(entity =>
            {
                entity.HasKey(e => e.LeaveNoteId)
                    .HasName("LEAVE_APPLICATION_PK");

                entity.ToTable("LEAVE_APPLICATION");

                entity.Property(e => e.LeaveNoteId)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("LEAVE_NOTE_ID");

                entity.Property(e => e.LeaveApplicationTime)
                    .HasPrecision(6)
                    .HasColumnName("LEAVE_APPLICATION_TIME");

                entity.Property(e => e.LeaveEndDate)
                    .HasPrecision(6)
                    .HasColumnName("LEAVE_END_DATE");

                entity.Property(e => e.LeaveNoteRemark)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("LEAVE_NOTE_REMARK");

                entity.Property(e => e.LeaveStartDate)
                    .HasPrecision(6)
                    .HasColumnName("LEAVE_START_DATE");
            });

            modelBuilder.Entity<MedicineDescription>(entity =>
            {
                entity.HasKey(e => e.MedicineName)
                    .HasName("MEDICINE_DESCRIPTION_PK");

                entity.ToTable("MEDICINE_DESCRIPTION");

                entity.Property(e => e.MedicineName)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("MEDICINE_NAME");

                entity.Property(e => e.Administration)
                    .HasMaxLength(400)
                    .IsUnicode(false)
                    .HasColumnName("ADMINISTRATION");

                entity.Property(e => e.ApplicableSymptom)
                    .HasMaxLength(600)
                    .IsUnicode(false)
                    .HasColumnName("APPLICABLE_SYMPTOM");

                entity.Property(e => e.Attention)
                    .HasMaxLength(400)
                    .IsUnicode(false)
                    .HasColumnName("ATTENTION");

                entity.Property(e => e.Frequency)
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasColumnName("FREQUENCY");

                entity.Property(e => e.MedicineType)
                    .HasMaxLength(40)
                    .IsUnicode(false)
                    .HasColumnName("MEDICINE_TYPE");

                entity.Property(e => e.Singledose)
                    .HasMaxLength(300)
                    .IsUnicode(false)
                    .HasColumnName("SINGLEDOSE");

                entity.Property(e => e.Specification)
                    .HasMaxLength(500)
                    .IsUnicode(false)
                    .HasColumnName("SPECIFICATION");

                entity.Property(e => e.Vulgo)
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasColumnName("VULGO");
            });

            modelBuilder.Entity<MedicineOut>(entity =>
            {
                entity.HasKey(e => new { e.MedicineName, e.Manufacturer, e.ProductionDate, e.PurchaseAmount, e.DeliverDate, e.PatientId })
                    .HasName("MEDICINE_OUT_PK");

                entity.ToTable("MEDICINE_OUT");

                entity.Property(e => e.MedicineName)
                    .HasMaxLength(40)
                    .IsUnicode(false)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("MEDICINE_NAME");

                entity.Property(e => e.Manufacturer)
                    .HasMaxLength(40)
                    .IsUnicode(false)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("MANUFACTURER");

                entity.Property(e => e.ProductionDate)
                    .HasColumnType("DATE")
                    .ValueGeneratedOnAdd()
                    .HasColumnName("PRODUCTION_DATE");

                entity.Property(e => e.PurchaseAmount)
                    .HasColumnType("NUMBER(38)")
                    .ValueGeneratedOnAdd()
                    .HasColumnName("PURCHASE_AMOUNT");

                entity.Property(e => e.DeliverDate)
                    .HasPrecision(0)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("DELIVER_DATE");

                entity.Property(e => e.PatientId)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("PATIENT_ID");

                entity.HasOne(d => d.Patient)
                    .WithMany(p => p.MedicineOuts)
                    .HasForeignKey(d => d.PatientId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("MEDICINE_OUT_PATIENT_FK1");
            });

            modelBuilder.Entity<MedicinePurchase>(entity =>
            {
                entity.HasKey(e => new { e.MedicineName, e.Manufacturer, e.ProductionDate, e.PurchaseDate })
                    .HasName("MEDICINE_PURCHASE_PK");

                entity.ToTable("MEDICINE_PURCHASE");

                entity.Property(e => e.MedicineName)
                    .HasMaxLength(40)
                    .IsUnicode(false)
                    .HasColumnName("MEDICINE_NAME");

                entity.Property(e => e.Manufacturer)
                    .HasMaxLength(460)
                    .IsUnicode(false)
                    .HasColumnName("MANUFACTURER");

                entity.Property(e => e.ProductionDate)
                    .HasColumnType("DATE")
                    .HasColumnName("PRODUCTION_DATE");

                entity.Property(e => e.PurchaseDate)
                    .HasColumnType("DATE")
                    .HasColumnName("PURCHASE_DATE");

                entity.Property(e => e.AdministratorId)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("ADMINISTRATOR_ID");

                entity.Property(e => e.PurchaseAmount)
                    .HasColumnType("NUMBER(38)")
                    .HasColumnName("PURCHASE_AMOUNT");

                entity.Property(e => e.PurchasePrice)
                    .HasColumnType("NUMBER(6,2)")
                    .HasColumnName("PURCHASE_PRICE");

                entity.HasOne(d => d.Administrator)
                    .WithMany(p => p.MedicinePurchases)
                    .HasForeignKey(d => d.AdministratorId)
                    .HasConstraintName("MEDICINE_PURCHASE_ADMINIS_FK1");
            });

            modelBuilder.Entity<MedicineSell>(entity =>
            {
                entity.HasKey(e => new { e.MedicineName, e.Manufacturer })
                    .HasName("MEDICINE_SELL_PK");

                entity.ToTable("MEDICINE_SELL");

                entity.Property(e => e.MedicineName)
                    .HasMaxLength(40)
                    .IsUnicode(false)
                    .HasColumnName("MEDICINE_NAME");

                entity.Property(e => e.Manufacturer)
                    .HasMaxLength(80)
                    .IsUnicode(false)
                    .HasColumnName("MANUFACTURER");

                entity.Property(e => e.SellingPrice)
                    .HasColumnType("NUMBER(6,2)")
                    .HasColumnName("SELLING_PRICE");
            });

            modelBuilder.Entity<MedicineStock>(entity =>
            {
                entity.HasKey(e => new { e.MedicineName, e.Manufacturer, e.ProductionDate })
                    .HasName("MEDICINE_STOCK_PK");

                entity.ToTable("MEDICINE_STOCK");

                entity.Property(e => e.MedicineName)
                    .HasMaxLength(40)
                    .IsUnicode(false)
                    .HasColumnName("MEDICINE_NAME");

                entity.Property(e => e.Manufacturer)
                    .HasMaxLength(40)
                    .IsUnicode(false)
                    .HasColumnName("MANUFACTURER");

                entity.Property(e => e.ProductionDate)
                    .HasColumnType("DATE")
                    .HasColumnName("PRODUCTION_DATE");

                entity.Property(e => e.CleanAdministrator)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("CLEAN_ADMINISTRATOR");

                entity.Property(e => e.CleanDate)
                    .HasColumnType("DATE")
                    .HasColumnName("CLEAN_DATE");

                entity.Property(e => e.MedicineAmount)
                    .HasColumnType("NUMBER(38)")
                    .HasColumnName("MEDICINE_AMOUNT");

                entity.Property(e => e.MedicineShelflife)
                    .HasColumnType("NUMBER(38)")
                    .HasColumnName("MEDICINE_SHELFLIFE");

                entity.Property(e => e.ThresholdValue)
                    .HasColumnType("NUMBER(38)")
                    .HasColumnName("THRESHOLD_VALUE");

                entity.HasOne(d => d.CleanAdministratorNavigation)
                    .WithMany(p => p.MedicineStocks)
                    .HasForeignKey(d => d.CleanAdministrator)
                    .HasConstraintName("MEDICINE_STOCK_ADMINISTRA_FK1");
            });

            modelBuilder.Entity<OutpatientOrder>(entity =>
            {
                entity.HasKey(e => e.OrderId)
                    .HasName("OUTPATIENT_ORDER_PK");

                entity.ToTable("OUTPATIENT_ORDER");

                entity.Property(e => e.OrderId)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("ORDER_ID");

                entity.Property(e => e.OrderTime)
                    .HasPrecision(6)
                    .HasColumnName("ORDER_TIME");

                entity.Property(e => e.PatientId)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("PATIENT_ID");

                entity.HasOne(d => d.Patient)
                    .WithMany(p => p.OutpatientOrders)
                    .HasForeignKey(d => d.PatientId)
                    .HasConstraintName("OUTPATIENT_ORDER_PATIENT_FK1");
            });

            modelBuilder.Entity<Patient>(entity =>
            {
                entity.ToTable("PATIENT");

                entity.Property(e => e.PatientId)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("PATIENT_ID");

                entity.Property(e => e.BirthDate)
                    .HasColumnType("DATE")
                    .HasColumnName("BIRTH_DATE");

                entity.Property(e => e.College)
                    .HasMaxLength(40)
                    .IsUnicode(false)
                    .HasColumnName("COLLEGE");

                entity.Property(e => e.Contact)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("CONTACT");

                entity.Property(e => e.Counsellor)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("COUNSELLOR");

                entity.Property(e => e.Gender)
                    .HasPrecision(1)
                    .HasColumnName("GENDER");

                entity.Property(e => e.Name)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("NAME");

                entity.Property(e => e.Password)
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasColumnName("PASSWORD");
            });

            modelBuilder.Entity<Prescription>(entity =>
            {
                entity.ToTable("PRESCRIPTION");

                entity.Property(e => e.PrescriptionId)
                    .HasMaxLength(40)
                    .IsUnicode(false)
                    .HasColumnName("PRESCRIPTION_ID");

                entity.Property(e => e.DoctorId)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("DOCTOR_ID");

                entity.Property(e => e.Paystate)
                    .HasColumnType("NUMBER(38)")
                    .HasColumnName("PAYSTATE")
                    .HasDefaultValueSql("0 ");

                entity.Property(e => e.TotalPrice)
                    .HasColumnType("NUMBER(6,2)")
                    .HasColumnName("TOTAL_PRICE");

                entity.HasOne(d => d.Doctor)
                    .WithMany(p => p.Prescriptions)
                    .HasForeignKey(d => d.DoctorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("PRESCRIPTION_DOCTOR_FK1");
            });

            modelBuilder.Entity<PrescriptionMedicine>(entity =>
            {
                entity.HasKey(e => new { e.PrescriptionId, e.MedicineName })
                    .HasName("PRESCRIPTION_MEDICINE_PK");

                entity.ToTable("PRESCRIPTION_MEDICINE");

                entity.Property(e => e.PrescriptionId)
                    .HasMaxLength(40)
                    .IsUnicode(false)
                    .HasColumnName("PRESCRIPTION_ID");

                entity.Property(e => e.MedicineName)
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasColumnName("MEDICINE_NAME");

                entity.Property(e => e.MedicationInstruction)
                    .HasMaxLength(800)
                    .IsUnicode(false)
                    .HasColumnName("MEDICATION_INSTRUCTION");

                entity.Property(e => e.MedicinePrice)
                    .HasColumnType("NUMBER(6,2)")
                    .HasColumnName("MEDICINE_PRICE");

                entity.Property(e => e.Quantity)
                    .HasColumnType("NUMBER(38)")
                    .HasColumnName("QUANTITY")
                    .HasDefaultValueSql("1 ");

                entity.HasOne(d => d.MedicineNameNavigation)
                    .WithMany(p => p.PrescriptionMedicines)
                    .HasForeignKey(d => d.MedicineName)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("PRESCRIPTION_MEDICINE_MED_FK1");

                entity.HasOne(d => d.Prescription)
                    .WithMany(p => p.PrescriptionMedicines)
                    .HasForeignKey(d => d.PrescriptionId)
                    .HasConstraintName("PRESCRIPTION_MEDICINE_PRE_FK1");
            });

            modelBuilder.Entity<Registration>(entity =>
            {
                entity.HasKey(e => new { e.PatientId, e.DoctorId, e.AppointmentTime, e.State, e.Period })
                    .HasName("REGISTRATION_PK");

                entity.ToTable("REGISTRATION");

                entity.Property(e => e.PatientId)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("PATIENT_ID");

                entity.Property(e => e.DoctorId)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("DOCTOR_ID");

                entity.Property(e => e.AppointmentTime)
                    .HasColumnType("DATE")
                    .HasColumnName("APPOINTMENT_TIME");

                entity.Property(e => e.State)
                    .HasColumnType("NUMBER(38)")
                    .HasColumnName("STATE");

                entity.Property(e => e.Period)
                    .HasColumnType("NUMBER(38)")
                    .HasColumnName("PERIOD");

                entity.Property(e => e.Checkin)
                    .HasColumnType("NUMBER(38)")
                    .HasColumnName("CHECKIN")
                    .HasDefaultValueSql("0 ");

                entity.Property(e => e.Ordertime)
                    .HasPrecision(0)
                    .HasColumnName("ORDERTIME");

                entity.Property(e => e.Prescriptionid)
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasColumnName("PRESCRIPTIONID");

                entity.Property(e => e.Qrcodeurl)
                    .HasMaxLength(2000)
                    .IsUnicode(false)
                    .HasColumnName("QRCODEURL");

                entity.Property(e => e.Registorder)
                    .HasColumnType("NUMBER(38)")
                    .HasColumnName("REGISTORDER");

                entity.HasOne(d => d.Doctor)
                    .WithMany(p => p.Registrations)
                    .HasForeignKey(d => d.DoctorId)
                    .HasConstraintName("REGISTRATION_DOCTOR_FK1");

                entity.HasOne(d => d.Patient)
                    .WithMany(p => p.Registrations)
                    .HasForeignKey(d => d.PatientId)
                    .HasConstraintName("REGISTRATION_PATIENT_FK1");

                entity.HasOne(d => d.Prescription)
                    .WithMany(p => p.Registrations)
                    .HasForeignKey(d => d.Prescriptionid)
                    .HasConstraintName("REGISTRATION_PRESCRIPTION_FK1");
            });

            modelBuilder.Entity<Template>(entity =>
            {
                entity.HasKey(e => e.Name)
                    .HasName("TEMPLATE_PK");

                entity.ToTable("TEMPLATE");

                entity.Property(e => e.Name)
                    .HasMaxLength(40)
                    .IsUnicode(false)
                    .HasColumnName("NAME");

                entity.Property(e => e.Column1)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("COLUMN1");

                entity.Property(e => e.Diagnose)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("DIAGNOSE");

                entity.Property(e => e.Illness)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("ILLNESS");

                entity.Property(e => e.Medicine)
                    .HasMaxLength(70)
                    .IsUnicode(false)
                    .HasColumnName("MEDICINE");

                entity.Property(e => e.Prescription)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("PRESCRIPTION");

                entity.Property(e => e.Problem)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("PROBLEM");

                entity.Property(e => e.Symptom)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("SYMPTOM");
            });

            modelBuilder.Entity<TreatmentFeedback>(entity =>
            {
                entity.HasKey(e => e.Diagnosedid)
                    .HasName("TREATMENT_FEEDBACK_PK");

                entity.ToTable("TREATMENT_FEEDBACK");

                entity.Property(e => e.Diagnosedid)
                    .HasMaxLength(40)
                    .IsUnicode(false)
                    .HasColumnName("DIAGNOSEDID");

                entity.Property(e => e.DoctorId)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("DOCTOR_ID");

                entity.Property(e => e.Evaluation)
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasColumnName("EVALUATION");

                entity.Property(e => e.PatientId)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("PATIENT_ID");

                entity.Property(e => e.TreatmentScore)
                    .HasColumnType("NUMBER(38)")
                    .HasColumnName("TREATMENT_SCORE");

                entity.HasOne(d => d.Diagnosed)
                    .WithOne(p => p.TreatmentFeedback)
                    .HasForeignKey<TreatmentFeedback>(d => d.Diagnosedid)
                    .HasConstraintName("TREATMENT_FEEDBACK_TREATM_FK1");
            });

            modelBuilder.Entity<TreatmentRecord>(entity =>
            {
                entity.HasKey(e => e.DiagnosisRecordId)
                    .HasName("TRANSFER_TREATMENT_PK");

                entity.ToTable("TREATMENT_RECORD");

                entity.Property(e => e.DiagnosisRecordId)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("DIAGNOSIS_RECORD_ID");

                entity.Property(e => e.DoctorId)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("DOCTOR_ID");

                entity.Property(e => e.LeaveNoteId)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("LEAVE_NOTE_ID");

                entity.Property(e => e.PatientId)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("PATIENT_ID");

                entity.HasOne(d => d.Doctor)
                    .WithMany(p => p.TreatmentRecords)
                    .HasForeignKey(d => d.DoctorId)
                    .HasConstraintName("TREATMENT_RECORD_DOCTOR_FK1");

                entity.HasOne(d => d.LeaveNote)
                    .WithMany(p => p.TreatmentRecords)
                    .HasForeignKey(d => d.LeaveNoteId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("TREATMENT_RECORD_LEAVE_AP_FK1");

                entity.HasOne(d => d.Patient)
                    .WithMany(p => p.TreatmentRecords)
                    .HasForeignKey(d => d.PatientId)
                    .HasConstraintName("TREATMENT_RECORD_PATIENT_FK1");
            });

            modelBuilder.Entity<TreatmentRecord2>(entity =>
            {
                entity.HasKey(e => e.DiagnoseId)
                    .HasName("CONSULTING_RECORD_PK");

                entity.ToTable("TREATMENT_RECORD2");

                entity.Property(e => e.DiagnoseId)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("DIAGNOSE_ID");

                entity.Property(e => e.Advice)
                    .HasMaxLength(2000)
                    .IsUnicode(false)
                    .HasColumnName("ADVICE");

                entity.Property(e => e.Anamnesis)
                    .HasMaxLength(2000)
                    .IsUnicode(false)
                    .HasColumnName("ANAMNESIS");

                entity.Property(e => e.Clinicdia)
                    .HasMaxLength(2000)
                    .IsUnicode(false)
                    .HasColumnName("CLINICDIA");

                entity.Property(e => e.Commentstate)
                    .HasColumnType("NUMBER(38)")
                    .HasColumnName("COMMENTSTATE")
                    .HasDefaultValueSql("0 ");

                entity.Property(e => e.DiagnoseTime)
                    .HasPrecision(6)
                    .HasColumnName("DIAGNOSE_TIME");

                entity.Property(e => e.Kindquantity)
                    .HasColumnType("NUMBER(38)")
                    .HasColumnName("KINDQUANTITY");

                entity.Property(e => e.Presenthis)
                    .HasMaxLength(2000)
                    .IsUnicode(false)
                    .HasColumnName("PRESENTHIS");

                entity.Property(e => e.Selfreported)
                    .HasMaxLength(2000)
                    .IsUnicode(false)
                    .HasColumnName("SELFREPORTED");

                entity.Property(e => e.Sign)
                    .HasMaxLength(1000)
                    .IsUnicode(false)
                    .HasColumnName("SIGN");

                entity.HasOne(d => d.Diagnose)
                    .WithOne(p => p.TreatmentRecord2)
                    .HasForeignKey<TreatmentRecord2>(d => d.DiagnoseId)
                    .HasConstraintName("TREATMENT_RECORD2_TREATME_FK1");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
