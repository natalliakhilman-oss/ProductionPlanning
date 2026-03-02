using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using ProductionPlanning.Extensions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using static ProductionPlanning.Models.DbGuid;

namespace ProductionPlanning.Models
{
    #region Users
    // Пользователи
    public partial class User : IdentityUser
    {
        public User(){}

        public int UserId { get; set; }

        [Display(Name = "Полное имя пользователя")]
        public string UserFullName { get; set; } = "";
    }
    #endregion Users

    #region Data
    
    // Заявка
    public partial class ProductRequest
    {
        public enum ProductionRequestType
        {
            [Description("Отсутствует")]
            No = 0,
            [Description("Месячный план")]
            MonthPlanning = 1,
            [Description("Оперативное планирование")]
            OperationPlanning = 2,
        }
        public enum ProductionRequestUrgency
        {
            [Description("Низкий (не срочная внеплановая заявка)")]
            Low = 0,
            [Description("Средний (плаовая заявка)")]
            Medium = 1,
            [Description("Высокий (срочная внеплановая заявка)")]
            High = 2,
        }
        public enum ProductRequestStatus
        {
            [Description("Нет статуса")]
            No = 0,
            [Description("Создана")]
            Created,
            [Description("Принята")]    //[Description("Заявка принята производством")]
            Accepted,
            [Description("В работе")]
            AtWork,
            [Description("Выполнена")]
            Completed,
            [Description("Отменена")]
            Canceled,
            [Description("Удалена")]
            Removed,
        }

        public Guid Id { get; set; } = Guid.NewGuid();

        [Display(Name = "Статус заявки")]
        public ProductRequestStatus Status { get; set; } = ProductRequestStatus.No;

        [Display(Name = "Дата создания заявки")]
        public DateTime DateCreate { get; set; } = DateTime.Now;
        
        [Display(Name = "Планируемая дата начала производства")]
        public DateTime? DatePlaningStart { get; set; } // Производство

        //[Display(Name = "Планируемая дата отгрузки")]
        //public DateTime? DatePlaningFinish { get; set; } // Производство

        [Display(Name = "Дата поставки")]
        [Required(ErrorMessage = "Укажите дату")]
        public DateTime DateMaxFinish { get; set; } // Сбыт

        [Display(Name = "Фактическая дата отгрузки")]
        public DateTime? DateFinish { get; set; }   // Автоматически

        [Display(Name = "Месячный план")]
        public DateTime MonthDatePlanning { get; set; } = DateTime.Now;

        [Display(Name = "Номер заявки в конкретный день")]
        public int DayNumber { get; set; } = 0;

        [Display(Name = "Изделие")]
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        public int EquipmentId { get; set; }

        [Display(Name = "Ссылка на прибор")]
        public Equipment? Equipment { get; set; }

        [Display(Name = "Количество")]
        [Range(1, int.MaxValue, ErrorMessage = "Количество должно быть больше 0")]
        public int Count { get; set; } = 0;

        [Display(Name ="Атрибут срочности")]
        public ProductionRequestUrgency AppUrgency {  get; set; } = ProductionRequestUrgency.Medium; 

        [Display(Name = "Тип заявки")]
        public ProductionRequestType AppType { get; set; } = ProductionRequestType.MonthPlanning;

        [Display(Name = "Метка")]
        public Guid? NoteId { get; set; }

        [Display(Name = "Ссылка на метку")]
        public Note? Note { get; set; }

        [Display(Name = "Признак удаления")]
        public bool IsDeleted { get; set; } = false;

        public List<Order> Orders { get; set; } = new();
       

        public class ProductionRequestConfiguration : IEntityTypeConfiguration<ProductRequest>
        {
            public void Configure(EntityTypeBuilder<ProductRequest> builder)
            {
                // Связи
                //builder.HasOne<Equipment>()
                builder.HasOne(p => p.Equipment)
                       .WithMany()
                       .HasForeignKey(a => a.EquipmentId)
                       .OnDelete(DeleteBehavior.Restrict);

                //builder.HasOne<Note>()
                builder.HasOne(p => p.Note)
                       .WithMany()
                       .HasForeignKey(a => a.NoteId)
                       .OnDelete(DeleteBehavior.SetNull);

                // Связь с Orders
                builder.HasMany(p => p.Orders)
                       .WithOne(o => o.ProductRequest)
                       .HasForeignKey(o => o.RequestId)
                       .OnDelete(DeleteBehavior.Restrict);

                // Индексы для оптимизации
                builder.HasIndex(a => a.EquipmentId);
                builder.HasIndex(a => a.NoteId);
                builder.HasIndex(a => a.DateCreate);
                builder.HasIndex(a => a.DateFinish);
                builder.HasIndex(a => a.DateMaxFinish);
                builder.HasIndex(a => a.DatePlaningStart);

            }
        }
    }

    // Оборудование
    public class Equipment
    {
        public enum EquipmentType
        {
            [Description("Отсутствует")]
            No = 0,
            [Description("Прибор")]
            Device = 1,
            [Description("Комплектующие")]
            Components = 2,
        }

        public int Id { get; set; }
        public EquipmentType Type { get; set; } = EquipmentType.Device;

        [Display(Name = "Наменование")]
        [Required(ErrorMessage = "Наименование обязательно")]
        public string Name { get; set; } = "";

        [Display(Name = "Артикул")]
        public string? Article { get; set; } = "";

        [Display(Name = "Признак удаления")]
        public bool IsDeleted { get; set; } = false;

        // Configuration
        public class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
        {
            public void Configure(EntityTypeBuilder<Equipment> builder)
            {
                builder.HasKey(e => e.Id);

                builder.Property(e => e.Type)
                       .HasConversion<string>()
                       .IsRequired();

                builder.Property(e => e.Article)
                       .HasMaxLength(50);

                builder.Property(e => e.IsDeleted)
                       .HasDefaultValue(false);

                builder.HasData(
                    new Equipment[]
                    {
                        new Equipment {Id = 1,Type = Equipment.EquipmentType.Device, Name = "А6-04" },
                        new Equipment {Id = 2,Type = Equipment.EquipmentType.Device, Name = "А6-06" },
                        new Equipment {Id = 3,Type = Equipment.EquipmentType.Device, Name = "А16-512" },
                        new Equipment {Id = 4,Type = Equipment.EquipmentType.Device, Name = "А6-04 + адаптер GSM" },
                        new Equipment {Id = 5,Type = Equipment.EquipmentType.Device, Name = "А6-06 + адаптер GSM" },
                        new Equipment {Id = 6,Type = Equipment.EquipmentType.Device, Name = "А6-512 + адаптер GSM" },
                        new Equipment {Id = 7,Type = Equipment.EquipmentType.Device, Name = "АМС-8" },
                        new Equipment {Id = 8,Type = Equipment.EquipmentType.Device, Name = "РМ-68-2" },
                        new Equipment {Id = 9,Type = Equipment.EquipmentType.Device, Name = "SIM800 1 - сим" },
                        new Equipment {Id = 10,Type = Equipment.EquipmentType.Device, Name = "РМ-64" },
                        new Equipment {Id = 11,Type = Equipment.EquipmentType.Device, Name = "ИС-485" },
                        new Equipment {Id = 12,Type = Equipment.EquipmentType.Device, Name = "ИС-USB" }

                    });
            }

        }
    }   

    // Метки
    public class Note
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Display(Name = "Название")]
        public string Name { get; set; } = "";
       
        [Display(Name = "Описание")]
        public string Description { get; set; } = "";

        [Display(Name = "Номер")]
        public int Number { get; set; } = 0;

        [Display(Name = "Заказчик")]
        public string Customer { get; set; } = "";

        [Display(Name = "Признак удаления")]
        public bool IsDeleted { get; set; } = false;

        [Display(Name = "Дата создания")]
        public DateTime DateCreate { get; set; } = DateTime.Now;

        //[Display(Name = "Ответственный")]
        //public string ResponsiblePerson { get; set; } = "";

        public class NoteConfiguration : IEntityTypeConfiguration<Note>
        {
            public void Configure(EntityTypeBuilder<Note> builder)
            {
                builder.ToTable("Notes");

                builder.HasKey(n => n.Id);

                builder.Property(n => n.Name)
                       .IsRequired()
                       .HasMaxLength(200);

                builder.Property(n => n.Number)
                       .HasMaxLength(50);

                builder.Property(n => n.Customer)
                       .HasMaxLength(150);

                //builder.Property(n => n.ResponsiblePerson)
                //       .HasMaxLength(100);

                // Индексы для оптимизации поиска
                builder.HasIndex(n => n.Number);
                builder.HasIndex(n => n.Customer);
                //builder.HasIndex(n => n.ResponsiblePerson);
            }
        }

    }

    // Orders by manufacture
    public class Order
    {
        // Статусы для Олега
        public enum OrderStatus
        {
            [Description("Нет статуса")]
            No = 0,
            [Description("Создана производством")]
            Created,
            [Description("Принята на учет")]
            Accepted,
            [Description("Отменена")]
            Canceled,
        }

        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid RequestId { get; set; }
        //public ProductRequest? Request { get; set; }

        [Display(Name = "Примечание")]
        public string? Description { get; set; } = "";

        [Display(Name = "Дата создания")]
        public DateTime DateCreate { get; set; } = DateTime.Now;
        
        [Display(Name = "Дата вставки")]
        public DateTime DateInsert { get; set; } = DateTime.Now;
        
        [Display(Name = "Количество изделий")]
        public int Count { get; set; }

        [Display(Name = "Исполнитель (пользователь создавший заказ)")]
        public string? UserId {  get; set; }
        public User? User { get; set; }

        [Display(Name = "Признак удаления")]
        public bool IsDeleted { get; set; } = false;

        [Display(Name = "Ссылка на зявку")]
        public ProductRequest? ProductRequest { get; set; }

        [Display(Name = "Статус ")]
        public OrderStatus StatusOrder { get; set; } = OrderStatus.Created;


        public class OrderConfiguration : IEntityTypeConfiguration<Order>
        {
            public void Configure(EntityTypeBuilder<Order> builder)
            {
                builder.ToTable("Orders");

                builder.HasKey(n => n.Id);

                // Связи
                builder.HasOne(p => p.ProductRequest)
                       .WithMany(pr => pr.Orders)
                       .HasForeignKey(a => a.RequestId)
                       .OnDelete(DeleteBehavior.Restrict);

                builder.HasOne(p => p.User)
                      .WithMany()
                      .HasForeignKey(a => a.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Индексы для оптимизации поиска
                builder.HasIndex(n => n.RequestId);
            }
        }
    }

    #endregion Data





    public partial class DBContext : IdentityDbContext<User>
    {
        public DbSet<Equipment> Equipments { get; set; }
        public DbSet<ProductRequest> ProductRequests { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<Order> Orders { get; set; }

        public DBContext(DbContextOptions<DBContext> options)
            : base(options)
        {

            // Add-Migration InitialCreate      - инициализация миграции
            // Update-Database                  - обновить базу данных (без имени миграции) - тоже самое происходит когда запускаем приложение и делаем Database.Migrate();

            // Add-Migration test               - добавить новую миграцию с именем test

            // Для удаления последней миграции:
            // 1. Revert migration from database: PM> Update-Database <PRIOR-migration-name>
            // 2. Remove migration file from project(or it will be reapplied again on next step)
            // 3. Update model snapshot: PM > Remove-Migration

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (!Database.IsSqlite())
                modelBuilder.UseGuidCollation(string.Empty);

            // Применение конфигураций
            modelBuilder.ApplyConfiguration(new ProductRequest.ProductionRequestConfiguration());
            modelBuilder.ApplyConfiguration(new Equipment.EquipmentConfiguration());
            modelBuilder.ApplyConfiguration(new Note.NoteConfiguration());
            modelBuilder.ApplyConfiguration(new Order.OrderConfiguration());

            base.OnModelCreating(modelBuilder);
        }

    }
}
